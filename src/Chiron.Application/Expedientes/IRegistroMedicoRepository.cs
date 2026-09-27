using Chiron.Application.Common;
using Chiron.Domain.Expedientes;

namespace Chiron.Application.Expedientes;

/// <summary>
/// Repositorio específico del expediente médico.
/// </summary>
public interface IRegistroMedicoRepository : IRepository<RegistroMedico>
{
    /// <summary>Obtiene el expediente (registros) de una mascota, ordenado del más reciente al más antiguo.</summary>
    Task<IReadOnlyList<RegistroMedico>> ObtenerPorMascotaAsync(
        Guid mascotaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los registros de una veterinaria con próxima aplicación pendiente
    /// dentro del rango [desde, hasta]. Base para los recordatorios (Épica 5).
    /// </summary>
    Task<IReadOnlyList<RegistroMedico>> ObtenerProximasAplicacionesAsync(
        Guid veterinariaId, DateOnly desde, DateOnly hasta, CancellationToken cancellationToken = default);
}
