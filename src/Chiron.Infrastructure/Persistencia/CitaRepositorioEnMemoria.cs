using Chiron.Application.Citas;
using Chiron.Domain.Citas;

namespace Chiron.Infrastructure.Persistencia;

/// <summary>
/// Implementación en memoria de ICitaRepository.
/// </summary>
public sealed class CitaRepositorioEnMemoria : RepositorioEnMemoria<Cita>, ICitaRepository
{
    public async Task<IReadOnlyList<Cita>> ObtenerAgendaDelDiaAsync(
        Guid veterinariaId, DateOnly dia, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Cita> todas = await ObtenerTodosAsync(cancellationToken);
        return todas
            .Where(c => c.VeterinariaId == veterinariaId
                        && DateOnly.FromDateTime(c.FechaHora) == dia)
            .OrderBy(c => c.FechaHora)
            .ToList();
    }

    public async Task<IReadOnlyList<Cita>> ObtenerProximasAsync(
        Guid veterinariaId, DateTime desde, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Cita> todas = await ObtenerTodosAsync(cancellationToken);
        return todas
            .Where(c => c.VeterinariaId == veterinariaId
                        && c.Estado == EstadoCita.Programada
                        && c.FechaHora >= desde)
            .OrderBy(c => c.FechaHora)
            .ToList();
    }
}
