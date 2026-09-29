using Chiron.Application.Cobros;
using Chiron.Domain.Cobros;

namespace Chiron.Infrastructure.Persistencia;

/// <summary>Implementación en memoria de ICargoRepository.</summary>
public sealed class CargoRepositorioEnMemoria : RepositorioEnMemoria<Cargo>, ICargoRepository
{
    public async Task<IReadOnlyList<Cargo>> ObtenerPendientesAsync(
        Guid veterinariaId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Cargo> todos = await ObtenerTodosAsync(cancellationToken);
        return todos
            .Where(c => c.VeterinariaId == veterinariaId && c.Estado == EstadoCargo.Pendiente)
            .OrderByDescending(c => c.FechaCreacion)
            .ToList();
    }
}
