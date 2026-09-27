using Chiron.Application.Common;
using Chiron.Domain.Mascotas;

namespace Chiron.Application.Mascotas;

/// <summary>
/// Caso de uso: listar las mascotas (pacientes) de un cliente, con filtro de estado
/// (activas/inactivas/todas) procesado en el servidor.
/// </summary>
public sealed class ListarMascotasDeCliente
{
    private readonly IMascotaRepository _mascotas;

    public ListarMascotasDeCliente(IMascotaRepository mascotas) => _mascotas = mascotas;

    public async Task<IReadOnlyList<Mascota>> EjecutarAsync(
        Guid clienteId, FiltroEstado estado = FiltroEstado.Activos,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Mascota> mascotas = await _mascotas.ObtenerPorClienteAsync(clienteId, cancellationToken);
        return mascotas.AplicarFiltro(estado, m => m.Activo).ToList();
    }
}
