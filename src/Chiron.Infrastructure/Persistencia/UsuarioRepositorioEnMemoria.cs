using Chiron.Application.Seguridad;
using Chiron.Domain.Usuarios;

namespace Chiron.Infrastructure.Persistencia;

/// <summary>Implementación en memoria de IUsuarioRepository.</summary>
public sealed class UsuarioRepositorioEnMemoria : RepositorioEnMemoria<Usuario>, IUsuarioRepository
{
    public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Usuario> todos = await ObtenerTodosAsync(cancellationToken);
        return todos.FirstOrDefault(u => u.NombreUsuario == nombreUsuario);
    }

    public async Task<IReadOnlyList<Usuario>> ListarPorVeterinariaAsync(
        Guid veterinariaId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Usuario> todos = await ObtenerTodosAsync(cancellationToken);
        return todos.Where(u => u.VeterinariaId == veterinariaId).ToList();
    }

    public async Task<IReadOnlyList<Usuario>> ListarPorRolAsync(
        RolUsuario rol, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Usuario> todos = await ObtenerTodosAsync(cancellationToken);
        return todos.Where(u => u.Rol == rol).ToList();
    }

    public async Task<Usuario?> ObtenerPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Usuario> todos = await ObtenerTodosAsync(cancellationToken);
        return todos.FirstOrDefault(u => u.ClienteId == clienteId);
    }
}
