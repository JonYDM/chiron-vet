namespace Chiron.Domain.Citas;

/// <summary>
/// Respuesta del dueño a una cita programada. Es independiente del <see cref="EstadoCita"/>
/// (que lo maneja el staff): el dueño avisa si va o no, y la clínica decide qué hacer.
/// </summary>
public enum ConfirmacionCita
{
    /// <summary>El dueño aún no responde.</summary>
    Pendiente = 1,

    /// <summary>El dueño confirmó que asistirá.</summary>
    Confirmada = 2,

    /// <summary>El dueño avisó que no podrá asistir.</summary>
    NoAsistira = 3,
}
