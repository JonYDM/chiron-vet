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
}
