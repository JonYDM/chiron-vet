using Chiron.Domain.Clientes;

namespace Chiron.Application.Clientes;

/// <summary>
/// Caso de uso: buscar clientes de una veterinaria por texto en el nombre.
/// Si el texto viene vacío, devuelve todos los clientes de la veterinaria.
/// </summary>
public sealed class BuscarClientes
{
    private readonly IClienteRepository _clientes;

    public BuscarClientes(IClienteRepository clientes) => _clientes = clientes;

    public async Task<IReadOnlyList<Cliente>> EjecutarAsync(
        Guid veterinariaId, string? texto, CancellationToken cancellationToken = default)
    {
        // Sin filtro: listar todos los de la veterinaria.
        if (string.IsNullOrWhiteSpace(texto))
            return await _clientes.ListarPorVeterinariaAsync(veterinariaId, cancellationToken);

        return await _clientes.BuscarPorNombreAsync(veterinariaId, texto.Trim(), cancellationToken);
    }
}
