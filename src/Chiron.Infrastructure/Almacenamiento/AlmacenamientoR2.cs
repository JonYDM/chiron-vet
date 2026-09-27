using Amazon.S3;
using Amazon.S3.Model;
using Chiron.Application.Mascotas;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Chiron.Infrastructure.Almacenamiento;

/// <summary>
/// Implementación de IAlmacenamientoArchivos sobre Cloudflare R2 (API compatible con S3).
///
/// Antes de subir, comprime la imagen con ImageSharp: corrige orientación EXIF,
/// redimensiona al lado máximo configurado y recodifica a WebP con calidad moderada.
/// Una foto de celular de varios MB queda en ~50-120 KB, lo que hace la galería
/// fluida y económica en almacenamiento/egress.
/// </summary>
public sealed class AlmacenamientoR2 : IAlmacenamientoArchivos
{
    private const int LadoMaximoPx = 1000;
    private const int CalidadWebp = 78;

    private readonly IAmazonS3 _s3;
    private readonly R2Opciones _opciones;

    public AlmacenamientoR2(IAmazonS3 s3, R2Opciones opciones)
    {
        _s3 = s3;
        _opciones = opciones;
    }

    public async Task<ArchivoSubido> SubirImagenAsync(
        byte[] contenido, string prefijo, CancellationToken cancellationToken = default)
    {
        // 1) Comprimir/normalizar a WebP en memoria.
        using var entrada = new MemoryStream(contenido);
        using Image imagen = await Image.LoadAsync(entrada, cancellationToken);

        imagen.Mutate(x => x
            .AutoOrient() // corrige la rotación según metadatos EXIF (típico en fotos de celular)
            .Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(LadoMaximoPx, LadoMaximoPx)
            }));
        imagen.Metadata.ExifProfile = null; // quita metadatos (privacidad + peso)

        using var salida = new MemoryStream();
        await imagen.SaveAsWebpAsync(salida, new WebpEncoder { Quality = CalidadWebp }, cancellationToken);
        salida.Position = 0;

        // 2) Subir a R2 con una clave única.
        string clave = $"{prefijo.Trim('/')}/{Guid.NewGuid():N}.webp";
        var request = new PutObjectRequest
        {
            BucketName = _opciones.Bucket,
            Key = clave,
            InputStream = salida,
            ContentType = "image/webp",
            DisablePayloadSigning = true // requerido por R2 (no soporta el signing por chunks de AWS)
        };
        await _s3.PutObjectAsync(request, cancellationToken);

        // 3) Componer la URL pública (CDN r2.dev).
        string url = $"{_opciones.PublicUrl.TrimEnd('/')}/{clave}";
        return new ArchivoSubido(clave, url);
    }

    public async Task EliminarAsync(string clave, CancellationToken cancellationToken = default)
    {
        await _s3.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _opciones.Bucket,
            Key = clave
        }, cancellationToken);
    }
}
