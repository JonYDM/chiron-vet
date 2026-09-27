using Chiron.Domain.Usuarios;

namespace Chiron.Application.Seguridad;

/// <summary>
/// Caso de uso: obtener el usuario (acceso al portal) ligado a un cliente.
/// Útil para saber si un cliente ya tiene acceso y para resetear su PIN.
/// Devuelve el DTO seguro, o null si el cliente no tiene acceso creado.
/// </summary>
public sealed class ObtenerUsuarioDeCliente
{
    private readonly IUsuarioRepository _usuarios;

    public ObtenerUsuarioDeCliente(IUsuarioRepository usuarios) => _usuarios = usuarios;

    public async Task<UsuarioDto?> EjecutarAsync(
        Guid clienteId, CancellationToken cancellationToken = default)
    {
        Usuario? usuario = await _usuarios.ObtenerPorClienteAsync(clienteId, cancellationToken);
        return usuario is null ? null : UsuarioDto.Desde(usuario);
    }
}
