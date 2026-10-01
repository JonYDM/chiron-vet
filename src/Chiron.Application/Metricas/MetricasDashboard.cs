using Chiron.Application.Citas;
using Chiron.Application.Clientes;
using Chiron.Application.Common;
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
/// Alcance de las métricas de dinero que puede ver quien consulta el dashboard.
/// - Ninguno: no ve ventas (Veterinario).
/// - SoloHoy: ve solo la venta del día, su caja (Recepcionista).
/// - Completo: ve ventas del día y del mes, lo que genera el negocio (Admin).
/// </summary>
public enum AlcanceMetricas
{
    Ninguno = 0,
    SoloHoy = 1,
    Completo = 2,
}

/// <summary>
/// Caso de uso: reunir las métricas del dashboard de una veterinaria. Todo el cálculo
/// (sumas, conteos, filtros por fecha) se hace en el servidor, no en el cliente. Las
/// métricas de dinero se ENTREGAN según el alcance del rol (el Veterinario no las recibe;
/// la Recepcionista solo la venta del día; el Admin todo).
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
        Guid veterinariaId, AlcanceMetricas alcance, CancellationToken cancellationToken = default)
    {
        DateTime ahora = DateTime.UtcNow;
        // Cortes de día y mes en hora de México (en UTC, las ventas después de las 6 pm
        // contaban para el día siguiente).
        DateTime inicioDia = HoraMexico.InicioDeHoyUtc();
        DateTime inicioMes = HoraMexico.InicioDeMesUtc();

        // Métricas operativas: las ven todos los roles del staff.
        var proximas = await _citas.ObtenerProximasAsync(veterinariaId, ahora, cancellationToken);
        int citasProximas = proximas.Count(c => c.Estado == EstadoCita.Programada);

        var clientes = await _clientes.ListarPorVeterinariaAsync(veterinariaId, cancellationToken);
        int clientesActivos = clientes.Count(c => c.Activo);

        // Métricas de dinero: solo si el alcance lo permite.
        decimal ventasHoy = 0m, ventasMes = 0m;
        int numeroVentasMes = 0;
        if (alcance != AlcanceMetricas.Ninguno)
        {
            var ventasMesLista = await _ventas.ListarPorVeterinariaAsync(veterinariaId, inicioMes, ahora, cancellationToken);
            ventasHoy = ventasMesLista.Where(v => v.FechaHora >= inicioDia).Sum(v => v.Total);
            // El mes/acumulado solo lo ve el Admin (alcance Completo).
            if (alcance == AlcanceMetricas.Completo)
            {
                ventasMes = ventasMesLista.Sum(v => v.Total);
                numeroVentasMes = ventasMesLista.Count;
            }
        }

        return new MetricasDashboardDto(
            VentasHoy: ventasHoy,
            VentasMes: ventasMes,
            NumeroVentasMes: numeroVentasMes,
            CitasProximas: citasProximas,
            ClientesActivos: clientesActivos);
    }
}
