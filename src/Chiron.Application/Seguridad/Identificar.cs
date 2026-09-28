using Chiron.Domain.Common;
using Chiron.Domain.Usuarios;

namespace Chiron.Application.Seguridad;

/// <summary>Entrada: el identificador de acceso (usuario o teléfono).</summary>
public sealed record IdentificarComando(string Identificador);

/// <summary>
/// Resultado: si el usuario existe y su primer nombre (para saludarlo antes del PIN).
/// No expone datos sensibles: solo el primer nombre.
/// </summary>
public sealed record IdentificarResultado(bool Existe, string? Nombre);

/// <summary>
/// Caso de uso del PASO 1 del login (estilo Nubank): valida el identificador ANTES de
/// pedir el PIN y devuelve el primer nombre del usuario para saludarlo.
///
/// Seguridad: habilita enumeración de usuarios. Se devuelve SOLO el primer nombre (no
/// apellidos, rol ni veterinaria) y conviene aplicar rate-limiting en el endpoint.
/// No revela si el usuario está desactivado o su veterinaria inactiva (eso lo maneja
/// el login real); aquí solo confirma existencia para mejorar la UX.
/// </summary>
public sealed class Identificar
{
    private readonly IUsuarioRepository _usuarios;

    public Identificar(IUsuarioRepository usuarios) => _usuarios = usuarios;

    public async Task<Result<IdentificarResultado>> EjecutarAsync(
        IdentificarComando comando, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(comando.Identificador))
            return Result<IdentificarResultado>.Exito(new IdentificarResultado(false, null));

        string id = Usuario.NormalizarIdentificador(comando.Identificador);
        Usuario? usuario = await _usuarios.ObtenerPorNombreUsuarioAsync(id, cancellationToken);

        if (usuario is null || !usuario.Activo)
            return Result<IdentificarResultado>.Exito(new IdentificarResultado(false, null));

        // Solo el primer nombre (no exponer más de lo necesario).
        string primerNombre = usuario.Nombre.Trim().Split(' ')[0];
        return Result<IdentificarResultado>.Exito(new IdentificarResultado(true, primerNombre));
    }
}
