using Chiron.Domain.PuntoVenta;

namespace Chiron.Application.PuntoVenta;

/// <summary>Línea de una venta para exponer por la API.</summary>
public sealed record LineaVentaDto(
    Guid ProductoId,
    string NombreProducto,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal);

/// <summary>Cargo (consulta/servicio) cobrado en una venta, para exponer por la API.</summary>
public sealed record VentaCargoDto(Guid CargoId, string Concepto, decimal Monto);

/// <summary>Venta para exponer por la API (con sus líneas de producto y cargos).</summary>
public sealed record VentaDto(
    Guid Id,
    Guid? ClienteId,
    DateTime FechaHora,
    decimal Total,
    MetodoPago MetodoPago,
    decimal? MontoRecibido,
    decimal? Cambio,
    IReadOnlyList<LineaVentaDto> Lineas,
    IReadOnlyList<VentaCargoDto> Cargos)
{
    public static VentaDto Desde(Venta v) => new(
        v.Id,
        v.ClienteId,
        v.FechaHora,
        v.Total,
        v.MetodoPago,
        v.MontoRecibido,
        v.Cambio,
        v.Lineas.Select(l => new LineaVentaDto(
            l.ProductoId, l.NombreProducto, l.Cantidad, l.PrecioUnitario, l.Subtotal)).ToList(),
        v.Cargos.Select(c => new VentaCargoDto(c.CargoId, c.Concepto, c.Monto)).ToList());
}

/// <summary>
/// Caso de uso: historial de ventas de una veterinaria en un rango de fechas.
/// Si no se indican fechas, usa un rango amplio por defecto (últimos 90 días).
/// </summary>
public sealed class ListarVentas
{
    private readonly IVentaRepository _ventas;

    public ListarVentas(IVentaRepository ventas) => _ventas = ventas;

    public async Task<IReadOnlyList<VentaDto>> EjecutarAsync(
        Guid veterinariaId, DateTime? desde, DateTime? hasta, CancellationToken cancellationToken = default)
    {
        DateTime hastaReal = hasta ?? DateTime.UtcNow;
        DateTime desdeReal = desde ?? hastaReal.AddDays(-90);
        IReadOnlyList<Venta> ventas = await _ventas.ListarPorVeterinariaAsync(
            veterinariaId, desdeReal, hastaReal, cancellationToken);
        return ventas.Select(VentaDto.Desde).ToList();
    }
}

/// <summary>Caso de uso: historial de compras de un cliente.</summary>
public sealed class ListarVentasDeCliente
{
    private readonly IVentaRepository _ventas;

    public ListarVentasDeCliente(IVentaRepository ventas) => _ventas = ventas;

    public async Task<IReadOnlyList<VentaDto>> EjecutarAsync(
        Guid clienteId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Venta> ventas = await _ventas.ListarPorClienteAsync(clienteId, cancellationToken);
        return ventas.Select(VentaDto.Desde).ToList();
    }
}

/// <summary>Resumen de ventas de un período: total, conteo, desglose por método y por tipo.</summary>
public sealed record ResumenVentasDto(
    decimal Total,
    int NumeroVentas,
    decimal Efectivo,
    decimal Tarjeta,
    decimal Transferencia,
    decimal TotalProductos,
    decimal TotalConsultas);

/// <summary>
/// Caso de uso: resumen de ventas de una veterinaria en un rango de fechas. El cálculo
/// (totales y desglose) se hace en el servidor, no en el cliente.
/// </summary>
public sealed class ResumenVentas
{
    private readonly IVentaRepository _ventas;

    public ResumenVentas(IVentaRepository ventas) => _ventas = ventas;

    public async Task<ResumenVentasDto> EjecutarAsync(
        Guid veterinariaId, DateTime? desde, DateTime? hasta, CancellationToken cancellationToken = default)
    {
        DateTime hastaReal = hasta ?? DateTime.UtcNow;
        DateTime desdeReal = desde ?? hastaReal.AddDays(-30);
        IReadOnlyList<Venta> ventas = await _ventas.ListarPorVeterinariaAsync(
            veterinariaId, desdeReal, hastaReal, cancellationToken);

        decimal PorMetodo(MetodoPago m) => ventas.Where(v => v.MetodoPago == m).Sum(v => v.Total);

        return new ResumenVentasDto(
            Total: ventas.Sum(v => v.Total),
            NumeroVentas: ventas.Count,
            Efectivo: PorMetodo(MetodoPago.Efectivo),
            Tarjeta: PorMetodo(MetodoPago.Tarjeta),
            Transferencia: PorMetodo(MetodoPago.Transferencia),
            TotalProductos: ventas.Sum(v => v.TotalProductos),
            TotalConsultas: ventas.Sum(v => v.TotalConsultas));
    }
}
