using Chiron.Application.Common;
using Chiron.Domain.Mascotas;

namespace Chiron.Application.Mascotas;

/// <summary>
/// Repositorio de fotos de mascota. Extiende el genérico con la consulta de
/// galería (todas las fotos de una mascota, más recientes primero).
/// </summary>
public interface IFotoMascotaRepository : IRepository<FotoMascota>
{
    /// <summary>Lista las fotos de una mascota, de la más reciente a la más antigua.</summary>
    Task<IReadOnlyList<FotoMascota>> ListarPorMascotaAsync(
        Guid mascotaId, CancellationToken cancellationToken = default);
}
