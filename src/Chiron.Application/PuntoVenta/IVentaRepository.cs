using Chiron.Application.Common;
using Chiron.Domain.PuntoVenta;

namespace Chiron.Application.PuntoVenta;

/// <summary>
/// Repositorio específico de Venta, acotado por veterinaria.
/// </summary>
public interface IVentaRepository : IRepository<Venta>
{
    /// <summary>Lista las ventas de una veterinaria en un rango de fechas.</summary>
    Task<IReadOnlyList<Venta>> ListarPorVeterinariaAsync(
        Guid veterinariaId, DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);
}
