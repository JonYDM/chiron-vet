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

    /// <summary>Veterinaria (tenant) dueña de la venta.</summary>
    public Guid VeterinariaId { get; private set; }

    /// <summary>Cliente asociado a la venta (opcional; puede ser venta de mostrador).</summary>
    public Guid? ClienteId { get; private set; }

    /// <summary>Fecha y hora de la venta (UTC).</summary>
    public DateTime FechaHora { get; private set; }

    /// <summary>Líneas de la venta (solo lectura desde fuera).</summary>
    public IReadOnlyList<LineaVenta> Lineas => _lineas;

    /// <summary>Total de la venta (suma de subtotales).</summary>
    public decimal Total { get; private set; }

    /// <summary>Método de pago usado en la venta.</summary>
    public MetodoPago MetodoPago { get; private set; }

    /// <summary>Monto recibido del cliente (para calcular el vuelto en efectivo). Null si no aplica.</summary>
    public decimal? MontoRecibido { get; private set; }

    /// <summary>Cambio/vuelto entregado (MontoRecibido - Total), si aplica.</summary>
    public decimal? Cambio { get; private set; }

    private Venta(Guid veterinariaId, Guid? clienteId, List<LineaVenta> lineas,
        MetodoPago metodoPago, decimal? montoRecibido)
    {
        VeterinariaId = veterinariaId;
        ClienteId = clienteId;
        _lineas = lineas;
        FechaHora = DateTime.UtcNow;
        Total = lineas.Sum(l => l.Subtotal);
        MetodoPago = metodoPago;
        MontoRecibido = montoRecibido;
        Cambio = montoRecibido is { } recibido ? recibido - Total : null;
    }

    // Constructor privado sin parámetros para EF Core (materialización desde la BD).
    private Venta()
    {
        _lineas = new List<LineaVenta>();
    }

    /// <summary>
    /// Crea una Venta validando que tenga al menos una línea.
    /// El descuento de stock se coordina en el caso de uso (capa de aplicación).
    /// </summary>
    public static Result<Venta> Crear(Guid veterinariaId, Guid? clienteId, IEnumerable<LineaVenta> lineas,
        MetodoPago metodoPago = MetodoPago.Efectivo, decimal? montoRecibido = null)
    {
        if (veterinariaId == Guid.Empty)
            return Result<Venta>.Falla("La venta debe pertenecer a una veterinaria válida.");

        var listaLineas = lineas?.ToList() ?? new List<LineaVenta>();
        if (listaLineas.Count == 0)
            return Result<Venta>.Falla("La venta debe tener al menos un producto.");

        decimal total = listaLineas.Sum(l => l.Subtotal);

        // Si se indica monto recibido (típico en efectivo), debe cubrir el total.
        if (montoRecibido is { } recibido && recibido < total)
            return Result<Venta>.Falla("El monto recibido no cubre el total de la venta.");

        return Result<Venta>.Exito(new Venta(veterinariaId, clienteId, listaLineas, metodoPago, montoRecibido));
    }
}
