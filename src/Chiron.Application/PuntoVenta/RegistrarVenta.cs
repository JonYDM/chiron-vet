using Chiron.Domain.Common;
using Chiron.Domain.PuntoVenta;

namespace Chiron.Application.PuntoVenta;

/// <summary>Un renglón solicitado en la venta: qué producto y cuánta cantidad.</summary>
public sealed record ItemVentaComando(Guid ProductoId, int Cantidad);

/// <summary>Datos de entrada para registrar una venta.</summary>
public sealed record RegistrarVentaComando(
    Guid VeterinariaId,
    Guid? ClienteId,
    IReadOnlyList<ItemVentaComando> Items,
    MetodoPago MetodoPago = MetodoPago.Efectivo,
    decimal? MontoRecibido = null);

/// <summary>Resultado de una venta registrada.</summary>
public sealed record VentaResultado(Guid VentaId, decimal Total, decimal? Cambio);

/// <summary>
/// Caso de uso: registrar una venta (H6.2).
/// Valida existencia y stock de cada producto, construye las líneas con el precio
/// del momento, descuenta el stock y persiste la venta.
/// </summary>
public sealed class RegistrarVenta
{
    private readonly IProductoRepository _productos;
    private readonly IVentaRepository _ventas;

    public RegistrarVenta(IProductoRepository productos, IVentaRepository ventas)
    {
        _productos = productos;
        _ventas = ventas;
    }

    public async Task<Result<VentaResultado>> EjecutarAsync(
        RegistrarVentaComando comando, CancellationToken cancellationToken = default)
    {
        if (comando.Items is null || comando.Items.Count == 0)
            return Result<VentaResultado>.Falla("La venta debe incluir al menos un producto.");

        var lineas = new List<LineaVenta>(comando.Items.Count);
        // Guardamos (producto, cantidad) para descontar stock solo si todo es válido.
        var descuentos = new List<(Producto producto, int cantidad)>(comando.Items.Count);

        foreach (ItemVentaComando item in comando.Items)
        {
            Producto? producto = await _productos.ObtenerPorIdAsync(item.ProductoId, cancellationToken);
            if (producto is null)
                return Result<VentaResultado>.Falla($"El producto {item.ProductoId} no existe.");
            if (producto.VeterinariaId != comando.VeterinariaId)
                return Result<VentaResultado>.Falla("Un producto no pertenece a la veterinaria indicada.");
            if (item.Cantidad <= 0)
                return Result<VentaResultado>.Falla($"La cantidad de '{producto.Nombre}' debe ser mayor que cero.");
            if (item.Cantidad > producto.Stock)
                return Result<VentaResultado>.Falla(
                    $"Stock insuficiente de '{producto.Nombre}' (disponible: {producto.Stock}).");

            lineas.Add(new LineaVenta(producto.Id, producto.Nombre, item.Cantidad, producto.Precio));
            descuentos.Add((producto, item.Cantidad));
        }

        // Construir la venta (valida líneas y que el monto recibido cubra el total).
        Result<Venta> ventaResult = Venta.Crear(
            comando.VeterinariaId, comando.ClienteId, lineas,
            comando.MetodoPago, comando.MontoRecibido);
        if (!ventaResult.EsExito)
            return Result<VentaResultado>.Falla(ventaResult.Error!);

        // Todo validado: descontar stock y actualizar cada producto.
        foreach ((Producto producto, int cantidad) in descuentos)
        {
            producto.DescontarStock(cantidad);  // ya validado arriba
            await _productos.ActualizarAsync(producto, cancellationToken);
        }

        Venta venta = ventaResult.Valor!;
        await _ventas.AgregarAsync(venta, cancellationToken);

        return Result<VentaResultado>.Exito(new VentaResultado(venta.Id, venta.Total, venta.Cambio));
    }
}
