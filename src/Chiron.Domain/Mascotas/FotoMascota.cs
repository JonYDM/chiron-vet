using Chiron.Domain.Common;

namespace Chiron.Domain.Mascotas;

/// <summary>
/// Foto de una mascota (galería). Cada mascota puede acumular varias fotos a lo
/// largo del tiempo (p. ej. una por consulta). El archivo binario NO vive aquí:
/// se almacena en un servicio de objetos (Cloudflare R2) y esta entidad solo
/// guarda la referencia (clave del objeto + URL pública) y sus metadatos.
///
/// Diseño rico: constructor privado + fábrica Crear que valida.
/// </summary>
public sealed class FotoMascota : EntidadBase
{
    /// <summary>Veterinaria (tenant) a la que pertenece el registro.</summary>
    public Guid VeterinariaId { get; private set; }

    /// <summary>Mascota a la que pertenece la foto.</summary>
    public Guid MascotaId { get; private set; }

    /// <summary>
    /// Registro médico (consulta) al que se liga la foto, si aplica (opcional).
    /// Permite armar un "collage por consulta".
    /// </summary>
    public Guid? RegistroMedicoId { get; private set; }

    /// <summary>Clave del objeto en el almacenamiento (R2). Se usa para borrarlo.</summary>
    public string ClaveObjeto { get; private set; }

    /// <summary>URL pública para mostrar la foto (CDN de R2).</summary>
    public string Url { get; private set; }

    /// <summary>Momento en que se subió (UTC).</summary>
    public DateTime FechaSubida { get; private set; }

    /// <summary>Usuario que subió la foto.</summary>
    public Guid SubidaPorUsuarioId { get; private set; }

    private FotoMascota(
        Guid veterinariaId,
        Guid mascotaId,
        Guid? registroMedicoId,
        string claveObjeto,
        string url,
        Guid subidaPorUsuarioId)
    {
        VeterinariaId = veterinariaId;
        MascotaId = mascotaId;
        RegistroMedicoId = registroMedicoId;
        ClaveObjeto = claveObjeto;
        Url = url;
        SubidaPorUsuarioId = subidaPorUsuarioId;
        FechaSubida = DateTime.UtcNow;
    }

    /// <summary>Crea una FotoMascota validando las reglas de negocio.</summary>
    public static Result<FotoMascota> Crear(
        Guid veterinariaId,
        Guid mascotaId,
        string claveObjeto,
        string url,
        Guid subidaPorUsuarioId,
        Guid? registroMedicoId = null)
    {
        if (veterinariaId == Guid.Empty)
            return Result<FotoMascota>.Falla("La foto debe pertenecer a una veterinaria válida.");
        if (mascotaId == Guid.Empty)
            return Result<FotoMascota>.Falla("La foto debe estar asociada a una mascota válida.");
        if (string.IsNullOrWhiteSpace(claveObjeto))
            return Result<FotoMascota>.Falla("La clave del objeto es obligatoria.");
        if (string.IsNullOrWhiteSpace(url))
            return Result<FotoMascota>.Falla("La URL de la foto es obligatoria.");
        if (subidaPorUsuarioId == Guid.Empty)
            return Result<FotoMascota>.Falla("Se requiere el usuario que sube la foto.");

        var foto = new FotoMascota(
            veterinariaId, mascotaId, registroMedicoId,
            claveObjeto.Trim(), url.Trim(), subidaPorUsuarioId);
        return Result<FotoMascota>.Exito(foto);
    }
}
