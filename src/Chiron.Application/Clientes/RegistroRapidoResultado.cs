namespace Chiron.Application.Clientes;

/// <summary>
/// Resultado del registro rápido: identificadores del cliente y la mascota creados.
/// </summary>
/// <param name="ClienteId">Id del cliente creado.</param>
/// <param name="MascotaId">Id de la mascota creada.</param>
public sealed record RegistroRapidoResultado(Guid ClienteId, Guid MascotaId);
