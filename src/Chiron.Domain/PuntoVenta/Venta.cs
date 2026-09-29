using Chiron.Domain.Common;

namespace Chiron.Domain.PuntoVenta;

/// <summary>
/// Venta realizada en la veterinaria. Agrupa una o más líneas (productos) y calcula el total.
/// Multi-tenant. Opcionalmente asociada a un cliente (si se identifica al comprador).
/// Diseño rico: se construye con la fábrica Crear a partir de líneas ya validadas.
/// </summary>
public sealed class Venta : EntidadBase
{
    private readonly List<LineaVenta> _lineas;
    private readonly List<VentaCargo> _cargos;

    /// <summary>Veterinaria (tenant) dueña de la venta.</summary>
    public Guid VeterinariaId { get; private set; }

    /// <summary>Cliente asociado a la venta (opcional; puede ser venta de mostrador).</summary>
    public Guid? ClienteId { get; private set; }

    /// <summary>Fecha y hora de la venta (UTC).</summary>
    public DateTime FechaHora { get; private set; }

    /// <summary>Líneas de PRODUCTOS de la venta (solo lectura desde fuera).</summary>
    public IReadOnlyList<LineaVenta> Lineas => _lineas;

    /// <summary>Cargos (consultas/servicios) cobrados en esta venta.</summary>
    public IReadOnlyList<VentaCargo> Cargos => _cargos;

    /// <summary>Total de la venta (productos + cargos).</summary>
    public decimal Total { get; private set; }

    /// <summary>Método de pago usado en la venta.</summary>
    public MetodoPago MetodoPago { get; private set; }

    /// <summary>Monto recibido del cliente (para calcular el vuelto en efectivo). Null si no aplica.</summary>
    public decimal? MontoRecibido { get; private set; }

    /// <summary>Cambio/vuelto entregado (MontoRecibido - Total), si aplica.</summary>
    public decimal? Cambio { get; private set; }

    /// <summary>Total cobrado por CONSULTAS/servicios (suma de cargos). Para métricas.</summary>
    public decimal TotalConsultas => _cargos.Sum(c => c.Monto);

    /// <summary>Total cobrado por PRODUCTOS (suma de líneas). Para métricas.</summary>
    public decimal TotalProductos => _lineas.Sum(l => l.Subtotal);

    private Venta(Guid veterinariaId, Guid? clienteId, List<LineaVenta> lineas,
        List<VentaCargo> cargos, MetodoPago metodoPago, decimal? montoRecibido)
    {
        VeterinariaId = veterinariaId;
        ClienteId = clienteId;
        _lineas = lineas;
        _cargos = cargos;
        FechaHora = DateTime.UtcNow;
        Total = lineas.Sum(l => l.Subtotal) + cargos.Sum(c => c.Monto);
        MetodoPago = metodoPago;
        MontoRecibido = montoRecibido;
        Cambio = montoRecibido is { } recibido ? recibido - Total : null;
    }

    // Constructor privado sin parámetros para EF Core (materialización desde la BD).
    private Venta()
    {
        _lineas = new List<LineaVenta>();
        _cargos = new List<VentaCargo>();
    }

    /// <summary>
    /// Crea una Venta validando que tenga al menos un producto o cargo.
    /// El descuento de stock y el cobro de cargos se coordinan en el caso de uso.
    /// </summary>
    public static Result<Venta> Crear(Guid veterinariaId, Guid? clienteId, IEnumerable<LineaVenta> lineas,
        MetodoPago metodoPago = MetodoPago.Efectivo, decimal? montoRecibido = null,
        IEnumerable<VentaCargo>? cargos = null)
    {
        if (veterinariaId == Guid.Empty)
            return Result<Venta>.Falla("La venta debe pertenecer a una veterinaria válida.");

        var listaLineas = lineas?.ToList() ?? new List<LineaVenta>();
        var listaCargos = cargos?.ToList() ?? new List<VentaCargo>();
        if (listaLineas.Count == 0 && listaCargos.Count == 0)
            return Result<Venta>.Falla("La venta debe tener al menos un producto o cargo.");

        decimal total = listaLineas.Sum(l => l.Subtotal) + listaCargos.Sum(c => c.Monto);

        // Si se indica monto recibido (típico en efectivo), debe cubrir el total.
        if (montoRecibido is { } recibido && recibido < total)
            return Result<Venta>.Falla("El monto recibido no cubre el total de la venta.");

        return Result<Venta>.Exito(
            new Venta(veterinariaId, clienteId, listaLineas, listaCargos, metodoPago, montoRecibido));
    }
}
