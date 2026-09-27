namespace Chiron.Infrastructure.Seguridad;

/// <summary>
/// Configuración del JWT. Los valores (especialmente la clave) se leen de configuración
/// / variables de entorno, NUNCA se codifican en el fuente.
/// </summary>
public sealed class JwtOpciones
{
    /// <summary>Clave secreta para firmar el token (mínimo 32 caracteres). Viene de variable de entorno.</summary>
    public string Clave { get; set; } = string.Empty;

    /// <summary>Emisor del token (quién lo genera).</summary>
    public string Emisor { get; set; } = "Chiron";

    /// <summary>Audiencia del token (para quién es).</summary>
    public string Audiencia { get; set; } = "ChironApi";

    /// <summary>Minutos de validez del token.</summary>
    public int MinutosValidez { get; set; } = 480; // 8 horas (jornada laboral)
}
