using Chiron.Domain.Citas;

namespace Chiron.Application.Citas;

/// <summary>
/// Cita enriquecida para la agenda: incluye el nombre de la mascota (paciente) y del
/// dueño, resueltos en el servidor, para que el frontend no tenga que cruzarlos.
/// </summary>
/// <param name="Id">Id de la cita.</param>
/// <param name="MascotaId">Id de la mascota (paciente).</param>
/// <param name="MascotaNombre">Nombre de la mascota.</param>
/// <param name="ClienteNombre">Nombre del dueño.</param>
/// <param name="FechaHora">Fecha y hora (UTC).</param>
/// <param name="Motivo">Motivo de la cita.</param>
/// <param name="Estado">Estado actual.</param>
/// <param name="VeterinarioId">Veterinario asignado (opcional).</param>
public sealed record CitaDto(
    Guid Id,
    Guid MascotaId,
    string MascotaNombre,
    string ClienteNombre,
    DateTime FechaHora,
    string Motivo,
    EstadoCita Estado,
    Guid? VeterinarioId);
