using Chiron.Application.PuntoVenta;
using Chiron.Domain.PuntoVenta;

namespace Chiron.Infrastructure.Persistencia;

/// <summary>Implementación en memoria de IVentaRepository.</summary>
public sealed class VentaRepositorioEnMemoria : RepositorioEnMemoria<Venta>, IVentaRepository
{
    public async Task<IReadOnlyList<Venta>> ListarPorVeterinariaAsync(
        Guid veterinariaId, DateTime desde, DateTime hasta, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Venta> todas = await ObtenerTodosAsync(cancellationToken);
        return todas
            .Where(v => v.VeterinariaId == veterinariaId && v.FechaHora >= desde && v.FechaHora <= hasta)
            .OrderByDescending(v => v.FechaHora)
            .ToList();
    }

    public async Task<IReadOnlyList<Venta>> ListarPorClienteAsync(
        Guid clienteId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Venta> todas = await ObtenerTodosAsync(cancellationToken);
        return todas
            .Where(v => v.ClienteId == clienteId)
            .OrderByDescending(v => v.FechaHora)
            .ToList();
    }
}
