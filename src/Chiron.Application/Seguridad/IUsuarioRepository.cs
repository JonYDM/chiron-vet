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

    /// <summary>Lista los usuarios de una veterinaria (tenant).</summary>
    Task<IReadOnlyList<Usuario>> ListarPorVeterinariaAsync(
        Guid veterinariaId, CancellationToken cancellationToken = default);

    /// <summary>Lista todos los usuarios con un rol dado (ej: Administradores, para el SuperAdmin).</summary>
    Task<IReadOnlyList<Usuario>> ListarPorRolAsync(
        RolUsuario rol, CancellationToken cancellationToken = default);

    /// <summary>Obtiene el usuario dueño ligado a un cliente, o null.</summary>
    Task<Usuario?> ObtenerPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);
}
