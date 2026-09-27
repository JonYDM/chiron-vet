using Chiron.Application.Common;
using Chiron.Domain.Usuarios;

namespace Chiron.Application.Seguridad;

/// <summary>
/// Repositorio específico de Usuario, con búsqueda por correo para autenticación.
/// </summary>
public interface IUsuarioRepository : IRepository<Usuario>
{
    /// <summary>Obtiene un usuario por su correo (único a nivel global), o null si no existe.</summary>
    Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken = default);
}
