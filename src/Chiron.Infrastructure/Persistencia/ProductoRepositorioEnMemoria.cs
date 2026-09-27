using Chiron.Application.PuntoVenta;
using Chiron.Domain.PuntoVenta;

namespace Chiron.Infrastructure.Persistencia;

/// <summary>Implementación en memoria de IProductoRepository.</summary>
public sealed class ProductoRepositorioEnMemoria : RepositorioEnMemoria<Producto>, IProductoRepository
{
    public async Task<IReadOnlyList<Producto>> ListarPorVeterinariaAsync(
        Guid veterinariaId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Producto> todos = await ObtenerTodosAsync(cancellationToken);
        return todos.Where(p => p.VeterinariaId == veterinariaId).ToList();
    }
}
