using Chiron.Domain.Usuarios;

namespace Chiron.Application.Seguridad;

/// <summary>
/// DTO seguro de un usuario para exponer por la API. NUNCA incluye el HashPin
/// ni datos sensibles. Solo lo necesario para gestionarlo desde el frontend.
/// </summary>
public sealed record UsuarioDto(
    Guid Id,
    string NombreUsuario,
    string Nombre,
    RolUsuario Rol,
    bool Activo,
    Guid? ClienteId,
    Guid VeterinariaId,
    string? Telefono = null)
{
    /// <summary>Mapea una entidad Usuario a su DTO seguro.</summary>
    public static UsuarioDto Desde(Usuario u) =>
        new(u.Id, u.NombreUsuario, u.Nombre, u.Rol, u.Activo, u.ClienteId, u.VeterinariaId, u.Telefono);
}

/// <summary>
/// Caso de uso: listar los usuarios de una veterinaria (para el Administrador).
/// Devuelve DTOs seguros (sin hash). Se puede excluir al propio SuperAdmin/otros
/// según el rol, pero por defecto lista el staff y dueños del tenant.
/// </summary>
public sealed class ListarUsuariosDeVeterinaria
{
    private readonly IUsuarioRepository _usuarios;

    public ListarUsuariosDeVeterinaria(IUsuarioRepository usuarios) => _usuarios = usuarios;

    public async Task<IReadOnlyList<UsuarioDto>> EjecutarAsync(
        Guid veterinariaId,
        Chiron.Application.Common.FiltroEstado estado = Chiron.Application.Common.FiltroEstado.Activos,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Usuario> usuarios = await _usuarios.ListarPorVeterinariaAsync(veterinariaId, cancellationToken);
        return Chiron.Application.Common.FiltroEstadoExtensiones
            .AplicarFiltro(usuarios, estado, u => u.Activo)
            .Select(UsuarioDto.Desde)
            .ToList();
    }
}

/// <summary>
/// Caso de uso: listar todos los Administradores (para el SuperAdmin).
/// Devuelve DTOs seguros.
/// </summary>
public sealed class ListarAdministradores
{
    private readonly IUsuarioRepository _usuarios;

    public ListarAdministradores(IUsuarioRepository usuarios) => _usuarios = usuarios;

    public async Task<IReadOnlyList<UsuarioDto>> EjecutarAsync(
        Chiron.Application.Common.FiltroEstado estado = Chiron.Application.Common.FiltroEstado.Activos,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Usuario> admins = await _usuarios.ListarPorRolAsync(RolUsuario.Administrador, cancellationToken);
        return Chiron.Application.Common.FiltroEstadoExtensiones
            .AplicarFiltro(admins, estado, u => u.Activo)
            .Select(UsuarioDto.Desde)
            .ToList();
    }
}
