using Chiron.Application.Clientes;
using Chiron.Domain.Common;
using Chiron.Domain.Mascotas;

namespace Chiron.Application.Mascotas;

/// <summary>Datos para agregar una mascota a un cliente existente.</summary>
public sealed record AgregarMascotaComando(
    Guid VeterinariaId,
    Guid ClienteId,
    string Nombre,
    EspecieMascota Especie,
    SexoMascota Sexo,
    string? Raza,
    DateOnly? FechaNacimiento,
    decimal? PesoKg,
    string? Padecimientos,
    bool? Esterilizado);

/// <summary>
/// Caso de uso: agregar una mascota a un cliente ya registrado. Valida que el cliente
/// exista y pertenezca a la veterinaria (aislamiento multi-tenant).
/// </summary>
public sealed class AgregarMascota
{
    private readonly IMascotaRepository _mascotas;
    private readonly IClienteRepository _clientes;

    public AgregarMascota(IMascotaRepository mascotas, IClienteRepository clientes)
    {
        _mascotas = mascotas;
        _clientes = clientes;
    }

    public async Task<Result<Guid>> EjecutarAsync(
        AgregarMascotaComando comando, CancellationToken cancellationToken = default)
    {
        var cliente = await _clientes.ObtenerPorIdAsync(comando.ClienteId, cancellationToken);
        if (cliente is null)
            return Result<Guid>.Falla("El cliente indicado no existe.");
        if (cliente.VeterinariaId != comando.VeterinariaId)
            return Result<Guid>.Falla("El cliente no pertenece a tu veterinaria.");

        Result<Mascota> res = Mascota.Crear(
            comando.VeterinariaId, comando.ClienteId, comando.Nombre, comando.Especie,
            comando.Sexo, comando.Raza, comando.FechaNacimiento, comando.PesoKg,
            comando.Padecimientos, comando.Esterilizado);
        if (!res.EsExito)
            return Result<Guid>.Falla(res.Error!);

        Mascota mascota = res.Valor!;
        await _mascotas.AgregarAsync(mascota, cancellationToken);
        return Result<Guid>.Exito(mascota.Id);
    }
}

/// <summary>Datos para editar una mascota.</summary>
public sealed record EditarMascotaComando(
    Guid MascotaId,
    Guid VeterinariaId,
    string Nombre,
    EspecieMascota Especie,
    SexoMascota Sexo,
    string? Raza,
    DateOnly? FechaNacimiento,
    decimal? PesoKg,
    string? Padecimientos,
    bool? Esterilizado);

/// <summary>Caso de uso: editar los datos de una mascota (multi-tenant).</summary>
public sealed class EditarMascota
{
    private readonly IMascotaRepository _mascotas;

    public EditarMascota(IMascotaRepository mascotas) => _mascotas = mascotas;

    public async Task<Result<bool>> EjecutarAsync(
        EditarMascotaComando comando, CancellationToken cancellationToken = default)
    {
        Mascota? mascota = await _mascotas.ObtenerPorIdAsync(comando.MascotaId, cancellationToken);
        if (mascota is null)
            return Result<bool>.Falla("La mascota no existe.");
        if (mascota.VeterinariaId != comando.VeterinariaId)
            return Result<bool>.Falla("La mascota no pertenece a tu veterinaria.");

        Result<bool> res = mascota.ActualizarDatos(
            comando.Nombre, comando.Especie, comando.Sexo, comando.Raza,
            comando.FechaNacimiento, comando.PesoKg, comando.Padecimientos, comando.Esterilizado);
        if (!res.EsExito) return res;

        await _mascotas.ActualizarAsync(mascota, cancellationToken);
        return Result<bool>.Exito(true);
    }
}
