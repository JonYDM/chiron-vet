using Chiron.Domain.Expedientes;

namespace Chiron.Application.Expedientes;

/// <summary>
/// Caso de uso: ver el expediente médico completo de una mascota (H3.3),
/// ordenado del registro más reciente al más antiguo.
/// </summary>
public sealed class VerExpedienteMascota
{
    private readonly IRegistroMedicoRepository _registros;

    public VerExpedienteMascota(IRegistroMedicoRepository registros) => _registros = registros;

    public Task<IReadOnlyList<RegistroMedico>> EjecutarAsync(
        Guid mascotaId, CancellationToken cancellationToken = default)
        => _registros.ObtenerPorMascotaAsync(mascotaId, cancellationToken);
}
