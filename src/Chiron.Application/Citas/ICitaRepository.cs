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

    /// <summary>
    /// Lista las citas de una veterinaria, opcionalmente filtradas por estado, ordenadas
    /// por fecha descendente (para ver el historial completo: atendidas, canceladas, etc.).
    /// </summary>
    Task<IReadOnlyList<Cita>> ListarPorVeterinariaAsync(
        Guid veterinariaId, EstadoCita? estado, CancellationToken cancellationToken = default);
}
