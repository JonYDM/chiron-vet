namespace Chiron.Domain.Citas;

/// <summary>
/// Estado de una cita. 'NoAsistio' permite medir el ausentismo, problema clave
/// que los recordatorios por WhatsApp (Épica 5) buscan reducir.
/// </summary>
public enum EstadoCita
{
    /// <summary>Cita agendada, aún no ocurre.</summary>
    Programada = 1,

    /// <summary>La mascota fue atendida.</summary>
    Atendida = 2,

    /// <summary>La cita fue cancelada.</summary>
    Cancelada = 3,

    /// <summary>El cliente no se presentó (ausentismo).</summary>
    NoAsistio = 4
}
