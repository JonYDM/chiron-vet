using Chiron.Application.Mascotas;
using Chiron.Domain.Cobros;
using Chiron.Domain.Common;
using Chiron.Domain.Mascotas;

namespace Chiron.Application.Cobros;

/// <summary>Datos para generar un cargo (cuenta por cobrar) desde una consulta/servicio.</summary>
public sealed record GenerarCargoComando(
    Guid VeterinariaId,
    Guid MascotaId,
    string Concepto,
    decimal Monto,
    Guid? RegistroMedicoId);

/// <summary>
/// Caso de uso: el staff clínico genera un cargo PENDIENTE por un servicio. El clienteId
/// se deriva de la mascota (no se pide). Valida que la mascota sea de la veterinaria.
/// </summary>
public sealed class GenerarCargo
{
    private readonly ICargoRepository _cargos;
    private readonly IMascotaRepository _mascotas;

    public GenerarCargo(ICargoRepository cargos, IMascotaRepository mascotas)
    {
        _cargos = cargos;
        _mascotas = mascotas;
    }

    public async Task<Result<Guid>> EjecutarAsync(
        GenerarCargoComando comando, CancellationToken cancellationToken = default)
    {
        Mascota? mascota = await _mascotas.ObtenerPorIdAsync(comando.MascotaId, cancellationToken);
        if (mascota is null || mascota.VeterinariaId != comando.VeterinariaId)
            return Result<Guid>.Falla("La mascota no existe en tu veterinaria.");

        Result<Cargo> cargoResult = Cargo.Crear(
            comando.VeterinariaId, comando.MascotaId, mascota.ClienteId,
            comando.Concepto, comando.Monto, comando.RegistroMedicoId);
        if (!cargoResult.EsExito)
            return Result<Guid>.Falla(cargoResult.Error!);

        await _cargos.AgregarAsync(cargoResult.Valor!, cancellationToken);
        return Result<Guid>.Exito(cargoResult.Valor!.Id);
    }
}

/// <summary>Cargo pendiente enriquecido para la caja (con nombre de mascota y dueño).</summary>
public sealed record CargoPendienteDto(
    Guid Id,
    Guid MascotaId,
    string MascotaNombre,
    Guid ClienteId,
    string ClienteNombre,
    string Concepto,
    decimal Monto,
    DateTime FechaCreacion);

/// <summary>
/// Caso de uso: listar los cargos PENDIENTES de la veterinaria para la caja, con el
/// nombre de la mascota y del dueño resueltos en el servidor.
/// </summary>
public sealed class ListarCargosPendientes
{
    private readonly ICargoRepository _cargos;
    private readonly IMascotaRepository _mascotas;
    private readonly Chiron.Application.Clientes.IClienteRepository _clientes;

    public ListarCargosPendientes(
        ICargoRepository cargos,
        IMascotaRepository mascotas,
        Chiron.Application.Clientes.IClienteRepository clientes)
    {
        _cargos = cargos;
        _mascotas = mascotas;
        _clientes = clientes;
    }

    public async Task<IReadOnlyList<CargoPendienteDto>> EjecutarAsync(
        Guid veterinariaId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Cargo> pendientes = await _cargos.ObtenerPendientesAsync(veterinariaId, cancellationToken);

        var cacheMascotas = new Dictionary<Guid, Mascota?>();
        var cacheClientes = new Dictionary<Guid, Chiron.Domain.Clientes.Cliente?>();
        var resultado = new List<CargoPendienteDto>(pendientes.Count);

        foreach (Cargo c in pendientes)
        {
            if (!cacheMascotas.TryGetValue(c.MascotaId, out Mascota? mascota))
            {
                mascota = await _mascotas.ObtenerPorIdAsync(c.MascotaId, cancellationToken);
                cacheMascotas[c.MascotaId] = mascota;
            }
            if (!cacheClientes.TryGetValue(c.ClienteId, out Chiron.Domain.Clientes.Cliente? cliente))
            {
                cliente = await _clientes.ObtenerPorIdAsync(c.ClienteId, cancellationToken);
                cacheClientes[c.ClienteId] = cliente;
            }

            resultado.Add(new CargoPendienteDto(
                c.Id, c.MascotaId, mascota?.Nombre ?? "—",
                c.ClienteId, cliente?.Nombre ?? "—",
                c.Concepto, c.Monto, c.FechaCreacion));
        }

        return resultado;
    }
}
