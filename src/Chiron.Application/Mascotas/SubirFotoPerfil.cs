using Chiron.Domain.Common;
using Chiron.Domain.Mascotas;

namespace Chiron.Application.Mascotas;

/// <summary>
/// Caso de uso: sube (o reemplaza) la foto de PERFIL (avatar) de una mascota. Es una sola
/// imagen, distinta de la galería de consultas. Comprime y sube a R2, y guarda la URL en
/// la mascota. Acotado a la veterinaria del solicitante (aislamiento multi-tenant).
/// </summary>
public sealed class SubirFotoPerfil
{
    private const int MaxBytesEntrada = 5 * 1024 * 1024;

    private readonly IMascotaRepository _mascotas;
    private readonly IAlmacenamientoArchivos _almacenamiento;

    public SubirFotoPerfil(IMascotaRepository mascotas, IAlmacenamientoArchivos almacenamiento)
    {
        _mascotas = mascotas;
        _almacenamiento = almacenamiento;
    }

    public async Task<Result<string>> EjecutarAsync(
        Guid mascotaId, Guid veterinariaId, byte[] contenido, CancellationToken ct = default)
    {
        if (contenido is null || contenido.Length == 0)
            return Result<string>.Falla("La imagen está vacía.");
        if (contenido.Length > MaxBytesEntrada)
            return Result<string>.Falla("La imagen supera el tamaño máximo permitido (5 MB).");

        Mascota? mascota = await _mascotas.ObtenerPorIdAsync(mascotaId, ct);
        if (mascota is null || mascota.VeterinariaId != veterinariaId)
            return Result<string>.Falla("La mascota no existe en tu veterinaria.");

        ArchivoSubido subido = await _almacenamiento.SubirImagenAsync(
            contenido, prefijo: $"mascotas/{mascotaId}/perfil", ct);

        mascota.EstablecerFotoPerfil(subido.Url);
        await _mascotas.ActualizarAsync(mascota, ct);

        return Result<string>.Exito(subido.Url);
    }
}
