using System.Collections.Concurrent;
using Chiron.Application.Common;
using Chiron.Domain.Common;

namespace Chiron.Infrastructure.Persistencia;

/// <summary>
/// Implementación en memoria del repositorio genérico.
/// Sirve para desarrollo y pruebas sin necesidad de una base de datos.
/// Es intercambiable: en H7.1 se sustituirá por una implementación con PostgreSQL
/// sin tocar la capa de Aplicación (gracias a que ambas cumplen IRepository&lt;T&gt;).
///
/// Se usa ConcurrentDictionary para:
///  - Acceso por Id en O(1).
///  - Seguridad ante accesos concurrentes sin bloqueos manuales.
/// </summary>
/// <typeparam name="T">Tipo de entidad, debe heredar de EntidadBase.</typeparam>
public class RepositorioEnMemoria<T> : IRepository<T> where T : EntidadBase
{
    private readonly ConcurrentDictionary<Guid, T> _almacen = new();

    public Task AgregarAsync(T entidad, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entidad);
        // TryAdd evita sobrescribir si ya existiera un Id igual.
        _almacen.TryAdd(entidad.Id, entidad);
        return Task.CompletedTask;
    }

    public Task<T?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _almacen.TryGetValue(id, out T? entidad);
        return Task.FromResult(entidad);
    }

    public Task<IReadOnlyList<T>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<T> lista = _almacen.Values.ToList();
        return Task.FromResult(lista);
    }

    public Task ActualizarAsync(T entidad, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entidad);
        // El índice es el Id; asignar reemplaza el valor existente.
        _almacen[entidad.Id] = entidad;
        return Task.CompletedTask;
    }

    public Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _almacen.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
