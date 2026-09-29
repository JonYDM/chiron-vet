using Chiron.Application.Cobros;
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
    decimal? MontoRecibido = null,
    IReadOnlyList<Guid>? CargoIds = null);

/// <summary>Resultado de una venta registrada.</summary>
public sealed record VentaResultado(Guid VentaId, decimal Total, decimal? Cambio);

/// <summary>
/// Caso de uso: registrar una venta (H6.2). Cobra productos (con stock) y/o cargos
/// pendientes (cuentas por cobrar de consultas). Al cobrar un cargo, lo marca como
/// Cobrado y lo liga a la venta. La venta debe tener al menos un item (producto o cargo).
/// </summary>
public sealed class RegistrarVenta
{
    private readonly IProductoRepository _productos;
    private readonly IVentaRepository _ventas;
    private readonly ICargoRepository _cargos;

    public RegistrarVenta(IProductoRepository productos, IVentaRepository ventas, ICargoRepository cargos)
    {
        _productos = productos;
        _ventas = ventas;
        _cargos = cargos;
    }

    public async Task<Result<VentaResultado>> EjecutarAsync(
        RegistrarVentaComando comando, CancellationToken cancellationToken = default)
    {
        var items = comando.Items ?? new List<ItemVentaComando>();
        var cargoIds = comando.CargoIds ?? new List<Guid>();

        if (items.Count == 0 && cargoIds.Count == 0)
            return Result<VentaResultado>.Falla("La venta debe incluir al menos un producto o cargo.");

        var lineas = new List<LineaVenta>(items.Count + cargoIds.Count);
        var descuentos = new List<(Producto producto, int cantidad)>(items.Count);

        // ── Productos ──
        foreach (ItemVentaComando item in items)
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

        // ── Cargos (cuentas por cobrar) ──
        var cargosACobrar = new List<Chiron.Domain.Cobros.Cargo>(cargoIds.Count);
        foreach (Guid cargoId in cargoIds)
        {
            Chiron.Domain.Cobros.Cargo? cargo = await _cargos.ObtenerPorIdAsync(cargoId, cancellationToken);
            if (cargo is null)
                return Result<VentaResultado>.Falla($"El cargo {cargoId} no existe.");
            if (cargo.VeterinariaId != comando.VeterinariaId)
                return Result<VentaResultado>.Falla("Un cargo no pertenece a la veterinaria indicada.");
            if (cargo.Estado != Chiron.Domain.Cobros.EstadoCargo.Pendiente)
                return Result<VentaResultado>.Falla("Un cargo ya no está pendiente de cobro.");

            // El cargo es una línea de venta sin producto (ProductoId vacío), cantidad 1.
            lineas.Add(new LineaVenta(Guid.Empty, cargo.Concepto, 1, cargo.Monto));
            cargosACobrar.Add(cargo);
        }

        // Construir la venta (valida que el monto recibido cubra el total).
        Result<Venta> ventaResult = Venta.Crear(
            comando.VeterinariaId, comando.ClienteId, lineas,
            comando.MetodoPago, comando.MontoRecibido);
        if (!ventaResult.EsExito)
            return Result<VentaResultado>.Falla(ventaResult.Error!);

        Venta venta = ventaResult.Valor!;

        // Descontar stock de los productos.
        foreach ((Producto producto, int cantidad) in descuentos)
        {
            producto.DescontarStock(cantidad);
            await _productos.ActualizarAsync(producto, cancellationToken);
        }

        await _ventas.AgregarAsync(venta, cancellationToken);

        // Marcar los cargos como cobrados, ligados a la venta.
        foreach (Chiron.Domain.Cobros.Cargo cargo in cargosACobrar)
        {
            cargo.MarcarCobrado(venta.Id);
            await _cargos.ActualizarAsync(cargo, cancellationToken);
        }

        return Result<VentaResultado>.Exito(new VentaResultado(venta.Id, venta.Total, venta.Cambio));
    }
}
