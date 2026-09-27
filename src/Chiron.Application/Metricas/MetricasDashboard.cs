using Chiron.Application.Citas;
using Chiron.Application.Clientes;
using Chiron.Application.PuntoVenta;
using Chiron.Domain.Citas;

namespace Chiron.Application.Metricas;

/// <summary>Métricas del dashboard, calculadas en el servidor.</summary>
public sealed record MetricasDashboardDto(
    decimal VentasHoy,
    decimal VentasMes,
    int NumeroVentasMes,
    int CitasProximas,
    int ClientesActivos);

/// <summary>
/// Caso de uso: reunir las métricas del dashboard de una veterinaria. Todo el cálculo
/// (sumas, conteos, filtros por fecha) se hace en el servidor, no en el cliente.
/// </summary>
public sealed class MetricasDashboard
{
    private readonly IVentaRepository _ventas;
    private readonly ICitaRepository _citas;
    private readonly IClienteRepository _clientes;

    public MetricasDashboard(
        IVentaRepository ventas, ICitaRepository citas, IClienteRepository clientes)
    {
        _ventas = ventas;
        _citas = citas;
        _clientes = clientes;
    }

    public async Task<MetricasDashboardDto> EjecutarAsync(
        Guid veterinariaId, CancellationToken cancellationToken = default)
    {
        DateTime ahora = DateTime.UtcNow;
        DateTime inicioDia = new(ahora.Year, ahora.Month, ahora.Day, 0, 0, 0, DateTimeKind.Utc);
        DateTime inicioMes = new(ahora.Year, ahora.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var ventasMes = await _ventas.ListarPorVeterinariaAsync(veterinariaId, inicioMes, ahora, cancellationToken);
        decimal ventasHoy = ventasMes.Where(v => v.FechaHora >= inicioDia).Sum(v => v.Total);

        var proximas = await _citas.ObtenerProximasAsync(veterinariaId, ahora, cancellationToken);
        int citasProximas = proximas.Count(c => c.Estado == EstadoCita.Programada);

        var clientes = await _clientes.ListarPorVeterinariaAsync(veterinariaId, cancellationToken);
        int clientesActivos = clientes.Count(c => c.Activo);

        return new MetricasDashboardDto(
            VentasHoy: ventasHoy,
            VentasMes: ventasMes.Sum(v => v.Total),
            NumeroVentasMes: ventasMes.Count,
            CitasProximas: citasProximas,
            ClientesActivos: clientesActivos);
    }
}
