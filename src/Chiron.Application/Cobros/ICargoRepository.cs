using Chiron.Application.Common;
using Chiron.Domain.Cobros;

namespace Chiron.Application.Cobros;

/// <summary>Repositorio de Cargo (cuentas por cobrar), con consultas por veterinaria.</summary>
public interface ICargoRepository : IRepository<Cargo>
{
    /// <summary>Cargos PENDIENTES de una veterinaria (para la caja), más recientes primero.</summary>
    Task<IReadOnlyList<Cargo>> ObtenerPendientesAsync(
        Guid veterinariaId, CancellationToken cancellationToken = default);
}
