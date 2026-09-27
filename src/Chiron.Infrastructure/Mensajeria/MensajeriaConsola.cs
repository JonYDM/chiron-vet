using Chiron.Application.Recordatorios;
using Microsoft.Extensions.Logging;

namespace Chiron.Infrastructure.Mensajeria;

/// <summary>
/// Implementación de PRUEBA de IServicioMensajeria: en lugar de enviar por WhatsApp,
/// registra el mensaje en el log. Permite desarrollar y probar toda la lógica de
/// recordatorios sin depender de la cuenta de Meta ni de trámites.
///
/// La implementación real (Cloud API de Meta o capa de un proveedor) se agregará
/// después implementando esta misma interfaz, sin tocar la lógica de negocio.
/// </summary>
public sealed class MensajeriaConsola : IServicioMensajeria
{
    private readonly ILogger<MensajeriaConsola> _logger;

    public MensajeriaConsola(ILogger<MensajeriaConsola> logger) => _logger = logger;

    public Task<bool> EnviarAsync(MensajeRecordatorio mensaje, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📲 [SIMULADO] WhatsApp a {Telefono}: {Texto}",
            mensaje.TelefonoDestino, mensaje.Texto);
        return Task.FromResult(true);
    }
}
