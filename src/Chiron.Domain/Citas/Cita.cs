using Chiron.Domain.Common;

namespace Chiron.Domain.Citas;

/// <summary>
/// Cita agendada para una mascota en una veterinaria.
/// Diseño rico: constructor privado + fábrica Crear; el estado solo cambia
/// mediante métodos que validan la transición.
/// </summary>
public sealed class Cita : EntidadBase
{
    /// <summary>Veterinaria (tenant) dueña de la cita.</summary>
    public Guid VeterinariaId { get; private set; }

    /// <summary>Mascota (paciente) de la cita.</summary>
    public Guid MascotaId { get; private set; }

    /// <summary>Fecha y hora programada (UTC).</summary>
    public DateTime FechaHora { get; private set; }

    /// <summary>Motivo de la cita.</summary>
    public string Motivo { get; private set; }

    /// <summary>Estado actual de la cita.</summary>
    public EstadoCita Estado { get; private set; }

    /// <summary>Veterinario asignado a la cita (opcional). Referencia a un Usuario.</summary>
    public Guid? VeterinarioId { get; private set; }

    private Cita(Guid veterinariaId, Guid mascotaId, DateTime fechaHora, string motivo, Guid? veterinarioId)
    {
        VeterinariaId = veterinariaId;
        MascotaId = mascotaId;
        FechaHora = fechaHora;
        Motivo = motivo;
        Estado = EstadoCita.Programada;
        VeterinarioId = veterinarioId;
    }

    /// <summary>
    /// Agenda una nueva cita validando las reglas de negocio.
    /// </summary>
    public static Result<Cita> Crear(Guid veterinariaId, Guid mascotaId, DateTime fechaHora, string motivo, Guid? veterinarioId = null)
    {
        if (veterinariaId == Guid.Empty)
            return Result<Cita>.Falla("La cita debe pertenecer a una veterinaria válida.");

        if (mascotaId == Guid.Empty)
            return Result<Cita>.Falla("La cita debe estar asociada a una mascota válida.");

        if (fechaHora <= DateTime.UtcNow)
            return Result<Cita>.Falla("La fecha y hora de la cita debe ser futura.");

        if (string.IsNullOrWhiteSpace(motivo))
            return Result<Cita>.Falla("El motivo de la cita es obligatorio.");

        return Result<Cita>.Exito(new Cita(veterinariaId, mascotaId, fechaHora, motivo.Trim(), veterinarioId));
    }

    /// <summary>Marca la cita como atendida. Solo válido si estaba programada.</summary>
    public Result<bool> MarcarAtendida() => CambiarEstado(EstadoCita.Atendida);

    /// <summary>Marca la cita como cancelada. Solo válido si estaba programada.</summary>
    public Result<bool> Cancelar() => CambiarEstado(EstadoCita.Cancelada);

    /// <summary>Marca que el cliente no asistió. Solo válido si estaba programada.</summary>
    public Result<bool> MarcarNoAsistio() => CambiarEstado(EstadoCita.NoAsistio);

    /// <summary>
    /// Cambia el estado validando que solo se pueda transicionar desde 'Programada'.
    /// Evita cambios inválidos (ej: atender una cita ya cancelada).
    /// </summary>
    private Result<bool> CambiarEstado(EstadoCita nuevoEstado)
    {
        if (Estado != EstadoCita.Programada)
            return Result<bool>.Falla($"No se puede cambiar una cita en estado {Estado}.");

        Estado = nuevoEstado;
        return Result<bool>.Exito(true);
    }
}
