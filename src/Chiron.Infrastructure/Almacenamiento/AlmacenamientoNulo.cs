using Chiron.Application.Mascotas;

namespace Chiron.Infrastructure.Almacenamiento;

/// <summary>
/// Almacenamiento de fallback para desarrollo/consola cuando R2 no está configurado.
/// Falla explícitamente al intentar subir (no hay dónde guardar), de modo que quede
/// claro que faltan las variables de entorno de R2.
/// </summary>
public sealed class AlmacenamientoNulo : IAlmacenamientoArchivos
{
    public Task<ArchivoSubido> SubirImagenAsync(
        byte[] contenido, string prefijo, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(
            "El almacenamiento de fotos no está configurado. Define las variables R2_* en el entorno.");

    public Task EliminarAsync(string clave, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
