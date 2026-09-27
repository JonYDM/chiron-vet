namespace Chiron.Application.Mascotas;

/// <summary>Resultado de subir un archivo al almacenamiento de objetos.</summary>
/// <param name="Clave">Clave del objeto (para poder borrarlo después).</param>
/// <param name="Url">URL pública para mostrarlo.</param>
public readonly record struct ArchivoSubido(string Clave, string Url);

/// <summary>
/// Abstracción del almacenamiento de objetos (fotos). La capa de Aplicación depende
/// de esta interfaz, no de un proveedor concreto (Cloudflare R2, S3, etc.), respetando
/// la inversión de dependencias. La implementación vive en Infrastructure.
/// </summary>
public interface IAlmacenamientoArchivos
{
    /// <summary>
    /// Sube una imagen (ya recibida como bytes crudos del cliente). La implementación
    /// se encarga de comprimir/redimensionar antes de almacenar y de devolver la
    /// clave del objeto y su URL pública.
    /// </summary>
    /// <param name="contenido">Bytes de la imagen original.</param>
    /// <param name="prefijo">Prefijo/carpeta lógica (p. ej. "mascotas/{id}").</param>
    Task<ArchivoSubido> SubirImagenAsync(
        byte[] contenido, string prefijo, CancellationToken cancellationToken = default);

    /// <summary>Elimina un objeto por su clave.</summary>
    Task EliminarAsync(string clave, CancellationToken cancellationToken = default);
}
