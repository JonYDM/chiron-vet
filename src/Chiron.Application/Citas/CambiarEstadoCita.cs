using Chiron.Domain.Citas;
using Chiron.Domain.Common;

namespace Chiron.Application.Citas;

/// <summary>Acción de cambio de estado sobre una cita.</summary>
public enum AccionCita
{
    Atender = 1,
    Cancelar = 2,
    NoAsistio = 3,
}

/// <summary>Datos para cambiar el estado de una cita.</summary>
public sealed record CambiarEstadoCitaComando(
    Guid CitaId,
    AccionCita Accion,
    Guid VeterinariaId);

/// <summary>
/// Caso de uso: cambiar el estado de una cita (atender / cancelar / no asistió).
/// Valida que la cita exista, pertenezca a la veterinaria (multi-tenant) y que la
/// transición sea válida (la entidad Cita solo permite cambiar desde 'Programada').
/// </summary>
public sealed class CambiarEstadoCita
{
    private readonly ICitaRepository _citas;

    public CambiarEstadoCita(ICitaRepository citas) => _citas = citas;

    public async Task<Result<bool>> EjecutarAsync(
        CambiarEstadoCitaComando comando, CancellationToken cancellationToken = default)
    {
        Cita? cita = await _citas.ObtenerPorIdAsync(comando.CitaId, cancellationToken);
        if (cita is null)
            return Result<bool>.Falla("La cita indicada no existe.");
        if (cita.VeterinariaId != comando.VeterinariaId)
            return Result<bool>.Falla("La cita no pertenece a tu veterinaria.");

        Result<bool> resultado = comando.Accion switch
        {
            AccionCita.Atender => cita.MarcarAtendida(),
            AccionCita.Cancelar => cita.Cancelar(),
            AccionCita.NoAsistio => cita.MarcarNoAsistio(),
            _ => Result<bool>.Falla("Acción de cita no válida."),
        };

        if (!resultado.EsExito)
            return resultado;

        await _citas.ActualizarAsync(cita, cancellationToken);
        return Result<bool>.Exito(true);
    }
}
