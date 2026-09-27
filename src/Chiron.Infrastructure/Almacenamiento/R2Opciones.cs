namespace Chiron.Infrastructure.Almacenamiento;

/// <summary>
/// Configuración del almacenamiento de objetos Cloudflare R2 (compatible con S3).
/// Los valores vienen de variables de entorno (nunca del código):
/// R2_ACCESS_KEY_ID, R2_SECRET_ACCESS_KEY, R2_ENDPOINT, R2_BUCKET, R2_PUBLIC_URL.
/// </summary>
public sealed class R2Opciones
{
    public string AccessKeyId { get; init; } = "";
    public string SecretAccessKey { get; init; } = "";

    /// <summary>Endpoint S3 de R2, sin el nombre del bucket. Ej: https://&lt;account&gt;.r2.cloudflarestorage.com</summary>
    public string Endpoint { get; init; } = "";

    /// <summary>Nombre del bucket. Ej: chiron-fotos</summary>
    public string Bucket { get; init; } = "";

    /// <summary>URL pública base para servir los objetos (CDN r2.dev), sin barra final.</summary>
    public string PublicUrl { get; init; } = "";

    /// <summary>True si están presentes las credenciales mínimas para operar.</summary>
    public bool EstaConfigurado =>
        !string.IsNullOrWhiteSpace(AccessKeyId)
        && !string.IsNullOrWhiteSpace(SecretAccessKey)
        && !string.IsNullOrWhiteSpace(Endpoint)
        && !string.IsNullOrWhiteSpace(Bucket)
        && !string.IsNullOrWhiteSpace(PublicUrl);
}
