using Chiron.Application.Common;
using Chiron.Domain.Usuarios;

namespace Chiron.Application.Seguridad;

/// <summary>
/// Repositorio específico de Usuario, con búsqueda por nombre de usuario (identificador
/// de login: nombre de usuario para staff, teléfono para dueños de mascota).
/// </summary>
public interface IUsuarioRepository : IRepository<Usuario>
{
    /// <summary>Obtiene un usuario por su identificador de acceso (ya normalizado), o null.</summary>
    Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default);
}
