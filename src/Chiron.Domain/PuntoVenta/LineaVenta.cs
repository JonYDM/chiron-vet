namespace Chiron.Domain.PuntoVenta;

/// <summary>
/// Línea de una venta: un producto, su cantidad y el precio unitario al momento de vender.
/// Se guarda el precio en el momento de la venta (no se referencia el precio actual del
/// producto), porque el precio puede cambiar después y el histórico debe ser fiel.
/// </summary>
public sealed class LineaVenta
{
    /// <summary>Producto vendido.</summary>
    public Guid ProductoId { get; }

    /// <summary>Nombre del producto al momento de la venta (para el ticket/histórico).</summary>
    public string NombreProducto { get; }

    /// <summary>Cantidad vendida.</summary>
    public int Cantidad { get; }

    /// <summary>Precio unitario al momento de la venta.</summary>
    public decimal PrecioUnitario { get; }

    /// <summary>Importe de la línea (cantidad × precio unitario).</summary>
    public decimal Subtotal => Cantidad * PrecioUnitario;

    public LineaVenta(Guid productoId, string nombreProducto, int cantidad, decimal precioUnitario)
    {
        ProductoId = productoId;
        NombreProducto = nombreProducto;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
    }
}
