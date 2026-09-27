namespace Chiron.Domain.PuntoVenta;

/// <summary>
/// Método de pago de una venta. Lista cerrada para métricas de caja consistentes.
/// </summary>
public enum MetodoPago
{
    Efectivo = 1,
    Tarjeta = 2,
    Transferencia = 3,
}
