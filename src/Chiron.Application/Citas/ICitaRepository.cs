using Chiron.Application.Common;
using Chiron.Domain.Citas;

namespace Chiron.Application.Citas;

/// <summary>
/// Repositorio específico de Cita, con consultas de agenda por veterinaria.
/// </summary>
public interface ICitaRepository : IRepository<Cita>
{
    /// <summary>Obtiene las citas de una veterinaria en un día concreto, ordenadas por hora.</summary>
    Task<IReadOnlyList<Cita>> ObtenerAgendaDelDiaAsync(
        Guid veterinariaId, DateOnly dia, CancellationToken cancellationToken = default);

    /// <summary>Obtiene las próximas citas programadas de una veterinaria a partir de un momento dado.</summary>
    Task<IReadOnlyList<Cita>> ObtenerProximasAsync(
        Guid veterinariaId, DateTime desde, CancellationToken cancellationToken = default);
}
