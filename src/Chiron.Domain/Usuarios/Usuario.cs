using Chiron.Domain.Common;

namespace Chiron.Domain.Usuarios;

/// <summary>
/// Usuario que opera el sistema dentro de una veterinaria (tenant).
/// Pertenece a una Veterinaria (VeterinariaId) y tiene un Rol que define sus permisos.
///
/// NOTA: aquí NO se maneja la contraseña ni la autenticación. Eso se implementará
/// de forma segura junto con la API/login (Épica 8). Esta entidad modela la identidad
/// y el rol del usuario, no el mecanismo de acceso.
/// </summary>
public sealed class Usuario : EntidadBase
{
    /// <summary>Veterinaria (tenant) a la que pertenece el usuario.</summary>
    public Guid VeterinariaId { get; private set; }

    /// <summary>Nombre completo del usuario.</summary>
    public string Nombre { get; private set; }

    /// <summary>Correo del usuario (identificador de acceso futuro).</summary>
    public string Correo { get; private set; }

    /// <summary>Rol del usuario dentro de la veterinaria.</summary>
    public RolUsuario Rol { get; private set; }

    /// <summary>Indica si el usuario está activo.</summary>
    public bool Activo { get; private set; }

    private Usuario(Guid veterinariaId, string nombre, string correo, RolUsuario rol)
    {
        VeterinariaId = veterinariaId;
        Nombre = nombre;
        Correo = correo;
        Rol = rol;
        Activo = true;
    }

    /// <summary>
    /// Crea un Usuario validando las reglas de negocio.
    /// </summary>
    public static Result<Usuario> Crear(Guid veterinariaId, string nombre, string correo, RolUsuario rol)
    {
        if (veterinariaId == Guid.Empty)
            return Result<Usuario>.Falla("El usuario debe pertenecer a una veterinaria válida.");

        if (string.IsNullOrWhiteSpace(nombre))
            return Result<Usuario>.Falla("El nombre del usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(correo) || !EsCorreoValido(correo))
            return Result<Usuario>.Falla("El correo del usuario no es válido.");

        var usuario = new Usuario(veterinariaId, nombre.Trim(), correo.Trim().ToLowerInvariant(), rol);
        return Result<Usuario>.Exito(usuario);
    }

    /// <summary>Desactiva el usuario (ej: baja de un empleado).</summary>
    public void Desactivar() => Activo = false;

    /// <summary>
    /// Validación mínima de correo: contiene una '@' con texto antes y después,
    /// y un '.' en el dominio. Suficiente y eficiente para una regla de dominio;
    /// la validación estricta de formato se hará en la capa de entrada (API).
    /// </summary>
    private static bool EsCorreoValido(string correo)
    {
        int posArroba = correo.IndexOf('@');
        // Debe haber al menos un carácter antes de '@'.
        if (posArroba <= 0)
            return false;

        // Debe haber un '.' después de la '@' (dominio), sin quedar al final.
        int posPunto = correo.IndexOf('.', posArroba);
        return posPunto > posArroba + 1 && posPunto < correo.Length - 1;
    }
}
