using Chiron.Application.Common;
using Chiron.Application.Mascotas;
using Chiron.Domain.Clientes;
using Chiron.Domain.Common;
using Chiron.Domain.Mascotas;
using Chiron.Domain.Veterinarias;

namespace Chiron.Application.Clientes;

/// <summary>
/// Caso de uso: registro rápido de un cliente junto con su primera mascota.
/// Escenario de recepción: llega un cliente nuevo con su mascota y se dan de alta
/// ambos en una sola operación.
///
/// Responsabilidad única (SRP): orquestar la creación consistente de cliente + mascota.
/// Recibe sus dependencias por constructor (inyección de dependencias).
/// </summary>
public sealed class RegistrarClienteConMascota
{
    private readonly IClienteRepository _clientes;
    private readonly IMascotaRepository _mascotas;
    private readonly IRepository<Veterinaria> _veterinarias;

    public RegistrarClienteConMascota(
        IClienteRepository clientes,
        IMascotaRepository mascotas,
        IRepository<Veterinaria> veterinarias)
    {
        _clientes = clientes;
        _mascotas = mascotas;
        _veterinarias = veterinarias;
    }

    /// <summary>
    /// Ejecuta el registro rápido. Devuelve los IDs creados o el motivo de la falla.
    /// </summary>
    public async Task<Result<RegistroRapidoResultado>> EjecutarAsync(
        RegistrarClienteConMascotaComando comando,
        CancellationToken cancellationToken = default)
    {
        // 1. La veterinaria (tenant) debe existir y estar activa.
        Veterinaria? veterinaria = await _veterinarias.ObtenerPorIdAsync(comando.VeterinariaId, cancellationToken);
        if (veterinaria is null)
            return Result<RegistroRapidoResultado>.Falla("La veterinaria indicada no existe.");
        if (!veterinaria.Activa)
            return Result<RegistroRapidoResultado>.Falla("La veterinaria está inactiva (suscripción no vigente).");

        // 2. Crear el cliente (aplica sus propias validaciones de dominio).
        Result<Cliente> clienteResult = Cliente.Crear(
            comando.VeterinariaId, comando.NombreCliente, comando.TelefonoCliente, comando.OrigenCliente);
        if (!clienteResult.EsExito)
            return Result<RegistroRapidoResultado>.Falla(clienteResult.Error!);

        Cliente cliente = clienteResult.Valor!;

        // 3. Crear la mascota asociada al cliente recién creado.
        Result<Mascota> mascotaResult = Mascota.Crear(
            comando.VeterinariaId, cliente.Id, comando.NombreMascota, comando.Especie,
            comando.Sexo, comando.Raza, comando.FechaNacimiento);
        if (!mascotaResult.EsExito)
            return Result<RegistroRapidoResultado>.Falla(mascotaResult.Error!);

        Mascota mascota = mascotaResult.Valor!;

        // 4. Persistir ambos. El cliente primero para respetar la relación.
        await _clientes.AgregarAsync(cliente, cancellationToken);
        await _mascotas.AgregarAsync(mascota, cancellationToken);

        return Result<RegistroRapidoResultado>.Exito(
            new RegistroRapidoResultado(cliente.Id, mascota.Id));
    }
}
