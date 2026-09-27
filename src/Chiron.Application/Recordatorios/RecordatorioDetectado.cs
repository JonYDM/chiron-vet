namespace Chiron.Application.Recordatorios;

/// <summary>Tipo de recordatorio según su origen.</summary>
public enum TipoRecordatorio
{
    /// <summary>Próxima aplicación de vacuna o desparasitación (del expediente).</summary>
    ProximaAplicacion = 1,

    /// <summary>Cita agendada próxima.</summary>
    Cita = 2
}

/// <summary>
/// Un recordatorio detectado por el sistema, listo para convertirse en mensaje.
/// Es un resultado de lectura (no una entidad persistida).
/// </summary>
/// <param name="Tipo">Origen del recordatorio.</param>
/// <param name="ClienteId">Cliente (dueño) a notificar.</param>
/// <param name="NombreCliente">Nombre del dueño.</param>
/// <param name="TelefonoCliente">Teléfono del dueño (destino del mensaje).</param>
/// <param name="NombreMascota">Nombre de la mascota.</param>
/// <param name="Detalle">Descripción del motivo (ej: "Vacuna antirrábica").</param>
/// <param name="Fecha">Fecha del evento a recordar.</param>
public sealed record RecordatorioDetectado(
    TipoRecordatorio Tipo,
    Guid ClienteId,
    string NombreCliente,
    string TelefonoCliente,
    string NombreMascota,
    string Detalle,
    DateOnly Fecha);
