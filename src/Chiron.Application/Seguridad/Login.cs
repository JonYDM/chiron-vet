using Chiron.Application.Common;
using Chiron.Domain.Common;
using Chiron.Domain.Usuarios;
using Chiron.Domain.Veterinarias;

namespace Chiron.Application.Seguridad;

/// <summary>Datos de entrada del login: identificador (usuario o teléfono) + PIN.</summary>
public sealed record LoginComando(string Identificador, string Pin);

/// <summary>Resultado del login: token, expiración y datos básicos del usuario.</summary>
public sealed record LoginResultado(
    string Token, DateTime ExpiraEn, string Nombre, RolUsuario Rol, bool AdminOperativo);

/// <summary>
/// Caso de uso: autenticar con identificador + PIN y emitir un JWT.
/// Incluye bloqueo temporal por intentos fallidos (los PIN son cortos) y validación de
/// suscripción (veterinaria activa). Mensajes genéricos para no dar pistas a atacantes.
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
        const string errorGenerico = "Usuario o PIN incorrectos.";

        if (string.IsNullOrWhiteSpace(comando.Identificador) || string.IsNullOrWhiteSpace(comando.Pin))
            return Result<LoginResultado>.Falla(errorGenerico);

        string id = Usuario.NormalizarIdentificador(comando.Identificador);
        Usuario? usuario = await _usuarios.ObtenerPorNombreUsuarioAsync(id, cancellationToken);

        if (usuario is null)
            return Result<LoginResultado>.Falla(errorGenerico);

        if (!usuario.Activo)
            return Result<LoginResultado>.Falla("El usuario está desactivado.");

        // Bloqueo por intentos fallidos.
        if (usuario.EstaBloqueado())
            return Result<LoginResultado>.Falla("Demasiados intentos. Intente de nuevo en unos minutos.");

        // Verificar el PIN.
        if (!_hasheador.Verificar(comando.Pin, usuario.HashPin))
        {
            usuario.RegistrarIntentoFallido();
            await _usuarios.ActualizarAsync(usuario, cancellationToken);
            return Result<LoginResultado>.Falla(errorGenerico);
        }

        // Control de suscripción (el SuperAdmin no depende de una veterinaria).
        bool adminOperativo = true;
        if (usuario.Rol != RolUsuario.SuperAdmin)
        {
            Veterinaria? vet = await _veterinarias.ObtenerPorIdAsync(usuario.VeterinariaId, cancellationToken);
            if (vet is null || !vet.Activa)
                return Result<LoginResultado>.Falla("La veterinaria está inactiva. Contacte al proveedor.");
            adminOperativo = vet.AdminOperativo;
        }

        // Login exitoso: reiniciar contadores y emitir token.
        usuario.RegistrarLoginExitoso();
        await _usuarios.ActualizarAsync(usuario, cancellationToken);

        var datos = new DatosToken(
            usuario.Id, usuario.VeterinariaId, usuario.NombreUsuario, usuario.Rol,
            usuario.ClienteId, adminOperativo);
        (string token, DateTime expiraEn) = _generadorToken.Generar(datos);

        return Result<LoginResultado>.Exito(
            new LoginResultado(token, expiraEn, usuario.Nombre, usuario.Rol, adminOperativo));
    }
}
