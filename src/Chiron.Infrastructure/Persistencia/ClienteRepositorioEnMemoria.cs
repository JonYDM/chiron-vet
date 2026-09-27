using Chiron.Application.Clientes;
using Chiron.Domain.Clientes;

namespace Chiron.Infrastructure.Persistencia;

/// <summary>
/// Implementación en memoria de IClienteRepository.
/// Reutiliza la base genérica RepositorioEnMemoria&lt;Cliente&gt; y añade las consultas
/// específicas del negocio, siempre filtrando por veterinaria (aislamiento multi-tenant).
/// </summary>
public sealed class ClienteRepositorioEnMemoria : RepositorioEnMemoria<Cliente>, IClienteRepository
{
    public async Task<IReadOnlyList<Cliente>> BuscarPorNombreAsync(
        Guid veterinariaId, string texto, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Cliente> todos = await ObtenerTodosAsync(cancellationToken);
        return todos
            .Where(c => c.VeterinariaId == veterinariaId
                        && c.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<Cliente?> ObtenerPorTelefonoAsync(
        Guid veterinariaId, string telefono, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Cliente> todos = await ObtenerTodosAsync(cancellationToken);
        return todos.FirstOrDefault(
            c => c.VeterinariaId == veterinariaId && c.Telefono == telefono);
    }

    public async Task<IReadOnlyList<Cliente>> ListarPorVeterinariaAsync(
        Guid veterinariaId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Cliente> todos = await ObtenerTodosAsync(cancellationToken);
        return todos.Where(c => c.VeterinariaId == veterinariaId).ToList();
    }
}
