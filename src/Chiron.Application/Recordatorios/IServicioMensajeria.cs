namespace Chiron.Application.Recordatorios;

/// <summary>
/// Un mensaje de recordatorio listo para enviar.
/// </summary>
/// <param name="TelefonoDestino">Teléfono del destinatario (solo dígitos).</param>
/// <param name="Texto">Contenido del recordatorio.</param>
public sealed record MensajeRecordatorio(string TelefonoDestino, string Texto);

/// <summary>
/// Abstracción del canal de mensajería (WhatsApp, SMS, etc.).
/// La lógica de negocio depende de esta interfaz, NO de Meta ni de ningún proveedor
/// concreto (Inversión de Dependencias). Así, cambiar entre una implementación de
/// prueba (consola), la Cloud API oficial de Meta, o la capa de un proveedor externo,
/// no afecta el dominio ni los casos de uso.
/// </summary>
public interface IServicioMensajeria
{
    /// <summary>Envía un mensaje de recordatorio. Devuelve true si se aceptó el envío.</summary>
    Task<bool> EnviarAsync(MensajeRecordatorio mensaje, CancellationToken cancellationToken = default);
}
