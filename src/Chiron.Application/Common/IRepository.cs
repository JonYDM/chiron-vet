using Chiron.Domain.Common;

namespace Chiron.Application.Common;

/// <summary>
/// Contrato genérico de persistencia para entidades del dominio.
/// La capa de Aplicación depende de esta abstracción, NO de una base de datos concreta
/// (Inversión de Dependencias — la "D" de SOLID).
/// La implementación real (memoria, PostgreSQL, etc.) vive en Infrastructure.
///
/// Los métodos son asíncronos porque el acceso a datos es una operación de E/S:
/// esto evita bloquear hilos y prepara el terreno para una base de datos real.
/// </summary>
/// <typeparam name="T">Tipo de entidad, debe heredar de EntidadBase.</typeparam>
public interface IRepository<T> where T : EntidadBase
{
    /// <summary>Agrega una nueva entidad.</summary>
    Task AgregarAsync(T entidad, CancellationToken cancellationToken = default);

    /// <summary>Obtiene una entidad por su identificador, o null si no existe.</summary>
    Task<T?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Obtiene todas las entidades.</summary>
    Task<IReadOnlyList<T>> ObtenerTodosAsync(CancellationToken cancellationToken = default);

    /// <summary>Actualiza una entidad existente.</summary>
    Task ActualizarAsync(T entidad, CancellationToken cancellationToken = default);

    /// <summary>Elimina una entidad por su identificador.</summary>
    Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);
}
