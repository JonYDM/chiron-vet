using Chiron.Application.Common;
using Chiron.Domain.Common;
using Chiron.Domain.Usuarios;
using Chiron.Domain.Veterinarias;

namespace Chiron.Application.Seguridad;

/// <summary>Datos de entrada del login.</summary>
public sealed record LoginComando(string Correo, string Contrasena);

/// <summary>Resultado del login: el token y su expiración, más datos básicos del usuario.</summary>
public sealed record LoginResultado(string Token, DateTime ExpiraEn, string Nombre, RolUsuario Rol);

/// <summary>
/// Caso de uso: autenticar un usuario y emitir un JWT (H9.1).
/// Valida credenciales, que el usuario esté activo y que su veterinaria esté activa
/// (control de suscripción, H9.4). No revela si falló el correo o la contraseña
/// (mensaje genérico) para no dar pistas a atacantes.
/// </summary>
public sealed class Login
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IHasheadorContrasena _hasheador;
    private readonly IGeneradorToken _generadorToken;
    private readonly IRepository<Veterinaria> _veterinarias;

    public Login(
        IUsuarioRepository usuarios,
        IHasheadorContrasena hasheador,
        IGeneradorToken generadorToken,
        IRepository<Veterinaria> veterinarias)
    {
        _usuarios = usuarios;
        _hasheador = hasheador;
        _generadorToken = generadorToken;
        _veterinarias = veterinarias;
    }

    public async Task<Result<LoginResultado>> EjecutarAsync(
        LoginComando comando, CancellationToken cancellationToken = default)
    {
        const string errorGenerico = "Correo o contraseña incorrectos.";

        if (string.IsNullOrWhiteSpace(comando.Correo) || string.IsNullOrWhiteSpace(comando.Contrasena))
            return Result<LoginResultado>.Falla(errorGenerico);

        Usuario? usuario = await _usuarios.ObtenerPorCorreoAsync(
            comando.Correo.Trim().ToLowerInvariant(), cancellationToken);

        // Mismo mensaje si no existe o si la contraseña no coincide (no dar pistas).
        if (usuario is null || !_hasheador.Verificar(comando.Contrasena, usuario.HashContrasena))
            return Result<LoginResultado>.Falla(errorGenerico);

        if (!usuario.Activo)
            return Result<LoginResultado>.Falla("El usuario está desactivado.");

        // Control de suscripción: el SuperAdmin no pertenece a una veterinaria; el resto sí.
        if (usuario.Rol != RolUsuario.SuperAdmin)
        {
            Veterinaria? vet = await _veterinarias.ObtenerPorIdAsync(usuario.VeterinariaId, cancellationToken);
            if (vet is null || !vet.Activa)
                return Result<LoginResultado>.Falla("La veterinaria está inactiva. Contacte al proveedor.");
        }

        var datos = new DatosToken(usuario.Id, usuario.VeterinariaId, usuario.Correo, usuario.Rol);
        (string token, DateTime expiraEn) = _generadorToken.Generar(datos);

        return Result<LoginResultado>.Exito(
            new LoginResultado(token, expiraEn, usuario.Nombre, usuario.Rol));
    }
}
