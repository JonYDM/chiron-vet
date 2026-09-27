using Chiron.Application.Common;
using Chiron.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Chiron.Infrastructure.Persistencia.Ef;

/// <summary>
/// Implementación genérica de IRepository&lt;T&gt; sobre EF Core / PostgreSQL.
/// Es la alternativa de producción al RepositorioEnMemoria; ambas cumplen el mismo
/// contrato, por lo que la capa de Aplicación no cambia (inversión de dependencias).
/// </summary>
public class RepositorioEf<T> : IRepository<T> where T : EntidadBase
{
    protected readonly ChironDbContext Contexto;
    protected readonly DbSet<T> Conjunto;

    public RepositorioEf(ChironDbContext contexto)
    {
        Contexto = contexto;
        Conjunto = contexto.Set<T>();
    }

    public async Task AgregarAsync(T entidad, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entidad);
        await Conjunto.AddAsync(entidad, cancellationToken);
        await Contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task<T?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await Conjunto.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<T>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
        => await Conjunto.ToListAsync(cancellationToken);

    public async Task ActualizarAsync(T entidad, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entidad);
        Conjunto.Update(entidad);
        await Contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        T? entidad = await Conjunto.FindAsync([id], cancellationToken);
        if (entidad is not null)
        {
            Conjunto.Remove(entidad);
            await Contexto.SaveChangesAsync(cancellationToken);
        }
    }
}
