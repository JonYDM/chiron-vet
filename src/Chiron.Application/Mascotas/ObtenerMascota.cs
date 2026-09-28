using Chiron.Domain.Common;
using Chiron.Domain.Mascotas;

namespace Chiron.Application.Mascotas;

/// <summary>
/// Caso de uso: obtener una mascota por id, acotada a la veterinaria del solicitante
/// (aislamiento multi-tenant). Devuelve todos sus campos (peso, esterilizado, etc.).
/// </summary>
public sealed class ObtenerMascota
{
    private readonly IMascotaRepository _mascotas;

    public ObtenerMascota(IMascotaRepository mascotas) => _mascotas = mascotas;

    public async Task<Result<Mascota>> EjecutarAsync(
        Guid mascotaId, Guid veterinariaId, CancellationToken cancellationToken = default)
    {
        Mascota? mascota = await _mascotas.ObtenerPorIdAsync(mascotaId, cancellationToken);
        if (mascota is null || mascota.VeterinariaId != veterinariaId)
            return Result<Mascota>.Falla("La mascota no existe en tu veterinaria.");
        return Result<Mascota>.Exito(mascota);
    }
}
