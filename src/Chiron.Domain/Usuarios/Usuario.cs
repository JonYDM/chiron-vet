using Chiron.Domain.Common;

namespace Chiron.Domain.Usuarios;

/// <summary>
/// Usuario que opera el sistema dentro de una veterinaria (tenant).
/// Pertenece a una Veterinaria (VeterinariaId) y tiene un Rol que define sus permisos.
///
/// La contraseña se almacena SIEMPRE como hash (nunca en texto plano). El algoritmo
/// de hash vive en Infrastructure (IHasheadorContrasena); aquí solo se guarda el resultado.
/// </summary>
public sealed class Usuario : EntidadBase
{
    /// <summary>Veterinaria (tenant) a la que pertenece el usuario.</summary>
    public Guid VeterinariaId { get; private set; }

    /// <summary>Nombre completo del usuario.</summary>
    public string Nombre { get; private set; }

    /// <summary>Correo del usuario (identificador de acceso / login).</summary>
    public string Correo { get; private set; }

    /// <summary>Hash de la contraseña (nunca la contraseña en claro).</summary>
    public string HashContrasena { get; private set; }

    /// <summary>Rol del usuario dentro de la veterinaria.</summary>
    public RolUsuario Rol { get; private set; }

    /// <summary>Indica si el usuario está activo.</summary>
    public bool Activo { get; private set; }

    private Usuario(Guid veterinariaId, string nombre, string correo, string hashContrasena, RolUsuario rol)
    {
        VeterinariaId = veterinariaId;
        Nombre = nombre;
        Correo = correo;
        HashContrasena = hashContrasena;
        Rol = rol;
        Activo = true;
    }

    // Constructor privado sin parámetros para EF Core.
    private Usuario()
    {
        Nombre = string.Empty;
        Correo = string.Empty;
        HashContrasena = string.Empty;
    }

    /// <summary>
    /// Crea un Usuario validando las reglas de negocio.
    /// El hash de contraseña se calcula fuera (Infrastructure) y se pasa ya hasheado.
    /// </summary>
    public static Result<Usuario> Crear(
        Guid veterinariaId, string nombre, string correo, string hashContrasena, RolUsuario rol)
    {
        if (veterinariaId == Guid.Empty)
            return Result<Usuario>.Falla("El usuario debe pertenecer a una veterinaria válida.");

        if (string.IsNullOrWhiteSpace(nombre))
            return Result<Usuario>.Falla("El nombre del usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(correo) || !EsCorreoValido(correo))
            return Result<Usuario>.Falla("El correo del usuario no es válido.");

        if (string.IsNullOrWhiteSpace(hashContrasena))
            return Result<Usuario>.Falla("La contraseña es obligatoria.");

        var usuario = new Usuario(
            veterinariaId, nombre.Trim(), correo.Trim().ToLowerInvariant(), hashContrasena, rol);
        return Result<Usuario>.Exito(usuario);
    }

    /// <summary>Desactiva el usuario (ej: baja de un empleado).</summary>
    public void Desactivar() => Activo = false;

    /// <summary>Reactiva el usuario.</summary>
    public void Activar() => Activo = true;

    /// <summary>Actualiza el hash de la contraseña (cambio de contraseña).</summary>
    public void CambiarHashContrasena(string nuevoHash)
    {
        if (!string.IsNullOrWhiteSpace(nuevoHash))
            HashContrasena = nuevoHash;
    }

    /// <summary>
    /// Validación mínima de correo: '@' con texto antes y '.' en el dominio.
    /// La validación estricta se hace en la capa de entrada (API).
    /// </summary>
    private static bool EsCorreoValido(string correo)
    {
        int posArroba = correo.IndexOf('@');
        if (posArroba <= 0)
            return false;

        int posPunto = correo.IndexOf('.', posArroba);
        return posPunto > posArroba + 1 && posPunto < correo.Length - 1;
    }
}
