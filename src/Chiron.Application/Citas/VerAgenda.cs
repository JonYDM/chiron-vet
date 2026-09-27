using Chiron.Domain.Citas;

namespace Chiron.Application.Citas;

/// <summary>
/// Caso de uso: consultar la agenda de una veterinaria (H4.2).
/// </summary>
public sealed class VerAgenda
{
    private readonly ICitaRepository _citas;

    public VerAgenda(ICitaRepository citas) => _citas = citas;

    /// <summary>Agenda de un día concreto.</summary>
    public Task<IReadOnlyList<Cita>> DelDiaAsync(
        Guid veterinariaId, DateOnly dia, CancellationToken cancellationToken = default)
        => _citas.ObtenerAgendaDelDiaAsync(veterinariaId, dia, cancellationToken);

    /// <summary>Próximas citas programadas a partir de ahora.</summary>
    public Task<IReadOnlyList<Cita>> ProximasAsync(
        Guid veterinariaId, CancellationToken cancellationToken = default)
        => _citas.ObtenerProximasAsync(veterinariaId, DateTime.UtcNow, cancellationToken);
}
