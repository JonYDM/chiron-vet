namespace Chiron.Application.Recordatorios;

/// <summary>
/// Resultado del envío de recordatorios.
/// </summary>
/// <param name="Detectados">Cuántos recordatorios se detectaron.</param>
/// <param name="Enviados">Cuántos se enviaron correctamente.</param>
public sealed record EnvioRecordatoriosResultado(int Detectados, int Enviados);

/// <summary>
/// Caso de uso: detectar y enviar los recordatorios de una veterinaria (H5.2).
/// Usa GenerarRecordatorios para detectar y IServicioMensajeria para enviar,
/// manteniéndose independiente del canal concreto (WhatsApp, SMS, etc.).
/// </summary>
public sealed class EnviarRecordatorios
{
    private readonly GenerarRecordatorios _generar;
    private readonly IServicioMensajeria _mensajeria;

    public EnviarRecordatorios(GenerarRecordatorios generar, IServicioMensajeria mensajeria)
    {
        _generar = generar;
        _mensajeria = mensajeria;
    }

    public async Task<EnvioRecordatoriosResultado> EjecutarAsync(
        Guid veterinariaId, int diasAnticipacion = 7, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RecordatorioDetectado> detectados =
            await _generar.DetectarParaEnvioAsync(veterinariaId, diasAnticipacion, cancellationToken);

        int enviados = 0;
        foreach (RecordatorioDetectado r in detectados)
        {
            var mensaje = new MensajeRecordatorio(r.TelefonoCliente, FormatearTexto(r));
            if (await _mensajeria.EnviarAsync(mensaje, cancellationToken))
                enviados++;
        }

        return new EnvioRecordatoriosResultado(detectados.Count, enviados);
    }

    /// <summary>
    /// Construye el texto del recordatorio. En la implementación real de WhatsApp,
    /// estos textos corresponderán a plantillas de utilidad aprobadas por Meta.
    /// </summary>
    private static string FormatearTexto(RecordatorioDetectado r) => r.Tipo switch
    {
        TipoRecordatorio.ProximaAplicacion =>
            $"Hola {r.NombreCliente}, le recordamos que {r.NombreMascota} tiene pendiente: " +
            $"{r.Detalle} el {r.Fecha:dd/MM/yyyy}. ¡Le esperamos!",
        TipoRecordatorio.Cita =>
            $"Hola {r.NombreCliente}, le recordamos la cita de {r.NombreMascota} " +
            $"({r.Detalle}) el {r.Fecha:dd/MM/yyyy}. Responda para confirmar.",
        _ => $"Hola {r.NombreCliente}, tiene un recordatorio para {r.NombreMascota}."
    };
}
