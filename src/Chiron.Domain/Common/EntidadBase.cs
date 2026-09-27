namespace Chiron.Domain.Common;

/// <summary>
/// Clase base para todas las entidades del dominio.
/// Aporta un identificador único (Guid) generado al crearse.
/// Se usa Guid para que los identificadores no dependan de la base de datos
/// (útil para sistemas distribuidos y pruebas sin persistencia real).
/// </summary>
public abstract class EntidadBase
{
    /// <summary>Identificador único de la entidad.</summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
