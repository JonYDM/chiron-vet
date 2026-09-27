using Chiron.Application.Mascotas;
using Chiron.Domain.Mascotas;

namespace Chiron.Infrastructure.Persistencia;

/// <summary>
/// Implementación en memoria de IFotoMascotaRepository (consola/demo).
/// </summary>
public sealed class FotoMascotaRepositorioEnMemoria : RepositorioEnMemoria<FotoMascota>, IFotoMascotaRepository
{
    public async Task<IReadOnlyList<FotoMascota>> ListarPorMascotaAsync(
        Guid mascotaId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<FotoMascota> todas = await ObtenerTodosAsync(cancellationToken);
        return todas.Where(f => f.MascotaId == mascotaId)
                    .OrderByDescending(f => f.FechaSubida)
                    .ToList();
    }
}
