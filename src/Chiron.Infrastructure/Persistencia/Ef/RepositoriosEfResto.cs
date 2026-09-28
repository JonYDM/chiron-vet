using Chiron.Application.Citas;
using Chiron.Application.Expedientes;
using Chiron.Application.PuntoVenta;
using Chiron.Domain.Citas;
using Chiron.Domain.Expedientes;
using Chiron.Domain.PuntoVenta;
using Microsoft.EntityFrameworkCore;

namespace Chiron.Infrastructure.Persistencia.Ef;

/// <summary>Implementación EF Core de IRegistroMedicoRepository.</summary>
public sealed class RegistroMedicoRepositorioEf : RepositorioEf<RegistroMedico>, IRegistroMedicoRepository
{
    public RegistroMedicoRepositorioEf(ChironDbContext contexto) : base(contexto) { }

    public async Task<IReadOnlyList<RegistroMedico>> ObtenerPorMascotaAsync(
        Guid mascotaId, CancellationToken cancellationToken = default)
        => await Conjunto.Where(r => r.MascotaId == mascotaId)
            .OrderByDescending(r => r.Fecha).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<RegistroMedico>> ObtenerProximasAplicacionesAsync(
        Guid veterinariaId, DateOnly desde, DateOnly hasta, CancellationToken cancellationToken = default)
        => await Conjunto.Where(r => r.VeterinariaId == veterinariaId
                && r.FechaProximaAplicacion != null
                && r.FechaProximaAplicacion >= desde && r.FechaProximaAplicacion <= hasta)
            .OrderBy(r => r.FechaProximaAplicacion).ToListAsync(cancellationToken);
}

/// <summary>Implementación EF Core de ICitaRepository.</summary>
public sealed class CitaRepositorioEf : RepositorioEf<Cita>, ICitaRepository
{
    public CitaRepositorioEf(ChironDbContext contexto) : base(contexto) { }

    public async Task<IReadOnlyList<Cita>> ObtenerAgendaDelDiaAsync(
        Guid veterinariaId, DateOnly dia, CancellationToken cancellationToken = default)
    {
        DateTime inicio = dia.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        DateTime fin = inicio.AddDays(1);
        return await Conjunto.Where(c => c.VeterinariaId == veterinariaId
                && c.FechaHora >= inicio && c.FechaHora < fin)
            .OrderBy(c => c.FechaHora).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Cita>> ObtenerProximasAsync(
        Guid veterinariaId, DateTime desde, CancellationToken cancellationToken = default)
        => await Conjunto.Where(c => c.VeterinariaId == veterinariaId
                && c.Estado == EstadoCita.Programada && c.FechaHora >= desde)
            .OrderBy(c => c.FechaHora).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Cita>> ListarPorVeterinariaAsync(
        Guid veterinariaId, EstadoCita? estado, CancellationToken cancellationToken = default)
        => await Conjunto.Where(c => c.VeterinariaId == veterinariaId
                && (estado == null || c.Estado == estado))
            .OrderByDescending(c => c.FechaHora).ToListAsync(cancellationToken);
}

/// <summary>Implementación EF Core de IProductoRepository.</summary>
public sealed class ProductoRepositorioEf : RepositorioEf<Producto>, IProductoRepository
{
    public ProductoRepositorioEf(ChironDbContext contexto) : base(contexto) { }

    public async Task<IReadOnlyList<Producto>> ListarPorVeterinariaAsync(
        Guid veterinariaId, CancellationToken cancellationToken = default)
        => await Conjunto.Where(p => p.VeterinariaId == veterinariaId).ToListAsync(cancellationToken);
}

/// <summary>Implementación EF Core de IVentaRepository.</summary>
public sealed class VentaRepositorioEf : RepositorioEf<Venta>, IVentaRepository
{
    public VentaRepositorioEf(ChironDbContext contexto) : base(contexto) { }

    public async Task<IReadOnlyList<Venta>> ListarPorVeterinariaAsync(
        Guid veterinariaId, DateTime desde, DateTime hasta, CancellationToken cancellationToken = default)
        => await Conjunto.Include(v => v.Lineas)
            .Where(v => v.VeterinariaId == veterinariaId && v.FechaHora >= desde && v.FechaHora <= hasta)
            .OrderByDescending(v => v.FechaHora).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Venta>> ListarPorClienteAsync(
        Guid clienteId, CancellationToken cancellationToken = default)
        => await Conjunto.Include(v => v.Lineas)
            .Where(v => v.ClienteId == clienteId)
            .OrderByDescending(v => v.FechaHora).ToListAsync(cancellationToken);
}
