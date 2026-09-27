using Chiron.Application.Common;
using Chiron.Domain.PuntoVenta;

namespace Chiron.Application.PuntoVenta;

/// <summary>
/// Repositorio específico de Producto (catálogo), acotado por veterinaria.
/// </summary>
public interface IProductoRepository : IRepository<Producto>
{
    /// <summary>Lista el catálogo de productos de una veterinaria.</summary>
    Task<IReadOnlyList<Producto>> ListarPorVeterinariaAsync(
        Guid veterinariaId, CancellationToken cancellationToken = default);
}
