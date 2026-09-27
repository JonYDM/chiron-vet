using Chiron.Domain.Mascotas;

namespace Chiron.Application.Mascotas;

/// <summary>
/// Caso de uso: listar las mascotas (pacientes) de un cliente (dueño).
/// </summary>
public sealed class ListarMascotasDeCliente
{
    private readonly IMascotaRepository _mascotas;

    public ListarMascotasDeCliente(IMascotaRepository mascotas) => _mascotas = mascotas;

    public Task<IReadOnlyList<Mascota>> EjecutarAsync(
        Guid clienteId, CancellationToken cancellationToken = default)
        => _mascotas.ObtenerPorClienteAsync(clienteId, cancellationToken);
}
