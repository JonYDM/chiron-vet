using Chiron.Application.Expedientes;
using Chiron.Domain.Expedientes;

namespace Chiron.Infrastructure.Persistencia;

/// <summary>
/// Implementación en memoria de IRegistroMedicoRepository.
/// </summary>
public sealed class RegistroMedicoRepositorioEnMemoria
    : RepositorioEnMemoria<RegistroMedico>, IRegistroMedicoRepository
{
    public async Task<IReadOnlyList<RegistroMedico>> ObtenerPorMascotaAsync(
        Guid mascotaId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RegistroMedico> todos = await ObtenerTodosAsync(cancellationToken);
        return todos
            .Where(r => r.MascotaId == mascotaId)
            .OrderByDescending(r => r.Fecha)  // más reciente primero
            .ToList();
    }

    public async Task<IReadOnlyList<RegistroMedico>> ObtenerProximasAplicacionesAsync(
        Guid veterinariaId, DateOnly desde, DateOnly hasta, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RegistroMedico> todos = await ObtenerTodosAsync(cancellationToken);
        return todos
            .Where(r => r.VeterinariaId == veterinariaId
                        && r.FechaProximaAplicacion is { } p
                        && p >= desde && p <= hasta)
            .OrderBy(r => r.FechaProximaAplicacion)  // más próximas primero
            .ToList();
    }
}
