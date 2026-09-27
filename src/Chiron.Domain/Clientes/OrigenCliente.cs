namespace Chiron.Domain.Clientes;

/// <summary>
/// Canal por el que el cliente conoció la veterinaria.
/// Lista cerrada (enum) para garantizar datos consistentes y poder generar
/// métricas de marketing fiables (qué canal capta más clientes).
/// </summary>
public enum OrigenCliente
{
    /// <summary>No se especificó el origen.</summary>
    NoEspecificado = 0,

    /// <summary>Recomendación de otro cliente (boca a boca).</summary>
    Recomendacion = 1,

    /// <summary>Redes sociales (Facebook, Instagram, etc.).</summary>
    RedesSociales = 2,

    /// <summary>El cliente pasó físicamente por el local.</summary>
    PasoPorLocal = 3,

    /// <summary>Búsqueda en Google / internet.</summary>
    Google = 4,

    /// <summary>Otro canal no listado.</summary>
    Otro = 5
}
