using Chiron.Domain.Common;

namespace Chiron.Domain.Usuarios;

/// <summary>
/// Usuario que accede al sistema. Autenticación con identificador + PIN (sin correo):
///  - Staff (Administrador/Veterinario/Recepcionista): identificador = nombre de usuario.
///  - Dueño de mascota: identificador = su teléfono.
///  - SuperAdmin: nombre de usuario.
///
/// El PIN se almacena SIEMPRE como hash (BCrypt); nunca en claro. Se incluye bloqueo
/// temporal tras varios intentos fallidos (los PIN son cortos, hay que protegerlos).
/// </summary>
public sealed class Usuario : EntidadBase
{
    /// <summary>Máximo de intentos fallidos antes de bloquear.</summary>
    public const int MaxIntentosFallidos = 5;

    /// <summary>Minutos de bloqueo tras exceder los intentos.</summary>
    public const int MinutosBloqueo = 5;

    /// <summary>Veterinaria (tenant). Guid.Empty para SuperAdmin.</summary>
    public Guid VeterinariaId { get; private set; }

    /// <summary>
    /// Identificador de acceso: nombre de usuario (staff) o teléfono (dueño de mascota).
    /// Único a nivel global. Se normaliza a minúsculas/sin espacios.
    /// </summary>
    public string NombreUsuario { get; private set; }

    /// <summary>Nombre para mostrar del usuario.</summary>
    public string Nombre { get; private set; }

    /// <summary>Hash del PIN (nunca el PIN en claro).</summary>
    public string HashPin { get; private set; }

    /// <summary>Rol del usuario.</summary>
    public RolUsuario Rol { get; private set; }

    /// <summary>
    /// Cliente (dueño) al que corresponde este usuario, cuando el rol es DuenoMascota.
    /// Null para el staff. Permite que el dueño vea solo SUS mascotas.
    /// </summary>
    public Guid? ClienteId { get; private set; }

    /// <summary>Indica si el usuario está activo.</summary>
    public bool Activo { get; private set; }

    /// <summary>Contador de intentos fallidos consecutivos.</summary>
    public int IntentosFallidos { get; private set; }

    /// <summary>Momento (UTC) hasta el cual el usuario está bloqueado, si aplica.</summary>
    public DateTime? BloqueadoHasta { get; private set; }

    private Usuario(Guid veterinariaId, string nombreUsuario, string nombre, string hashPin, RolUsuario rol, Guid? clienteId)
    {
        VeterinariaId = veterinariaId;
        NombreUsuario = nombreUsuario;
        Nombre = nombre;
        HashPin = hashPin;
        Rol = rol;
        ClienteId = clienteId;
        Activo = true;
    }

    // Constructor privado sin parámetros para EF Core.
    private Usuario()
    {
        NombreUsuario = string.Empty;
        Nombre = string.Empty;
        HashPin = string.Empty;
    }

    /// <summary>
    /// Crea un usuario de staff (o SuperAdmin) con nombre de usuario y PIN (ya hasheado).
    /// </summary>
    public static Result<Usuario> CrearStaff(
        Guid veterinariaId, string nombreUsuario, string nombre, string hashPin, RolUsuario rol)
    {
        if (rol == RolUsuario.DuenoMascota)
            return Result<Usuario>.Falla("Use CrearDueno para usuarios dueños de mascota.");
        if (rol != RolUsuario.SuperAdmin && veterinariaId == Guid.Empty)
            return Result<Usuario>.Falla("El usuario debe pertenecer a una veterinaria válida.");

        return CrearInterno(veterinariaId, nombreUsuario, nombre, hashPin, rol, clienteId: null);
    }

    /// <summary>
    /// Crea un usuario dueño de mascota: identificador = teléfono, ligado a su Cliente.
    /// </summary>
    public static Result<Usuario> CrearDueno(
        Guid veterinariaId, Guid clienteId, string telefono, string nombre, string hashPin)
    {
        if (veterinariaId == Guid.Empty)
            return Result<Usuario>.Falla("El usuario debe pertenecer a una veterinaria válida.");
        if (clienteId == Guid.Empty)
            return Result<Usuario>.Falla("El dueño debe estar ligado a un cliente válido.");

        return CrearInterno(veterinariaId, telefono, nombre, hashPin, RolUsuario.DuenoMascota, clienteId);
    }

    private static Result<Usuario> CrearInterno(
        Guid veterinariaId, string identificador, string nombre, string hashPin, RolUsuario rol, Guid? clienteId)
    {
        if (string.IsNullOrWhiteSpace(identificador))
            return Result<Usuario>.Falla("El identificador de usuario es obligatorio.");
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<Usuario>.Falla("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(hashPin))
            return Result<Usuario>.Falla("El PIN es obligatorio.");

        string idNormalizado = NormalizarIdentificador(identificador);
        var usuario = new Usuario(veterinariaId, idNormalizado, nombre.Trim(), hashPin, rol, clienteId);
        return Result<Usuario>.Exito(usuario);
    }

    /// <summary>Indica si el usuario está bloqueado en este momento.</summary>
    public bool EstaBloqueado() => BloqueadoHasta is { } hasta && hasta > DateTime.UtcNow;

    /// <summary>
    /// Registra un intento fallido de login. Bloquea temporalmente al alcanzar el máximo.
    /// </summary>
    public void RegistrarIntentoFallido()
    {
        IntentosFallidos++;
        if (IntentosFallidos >= MaxIntentosFallidos)
        {
            BloqueadoHasta = DateTime.UtcNow.AddMinutes(MinutosBloqueo);
            IntentosFallidos = 0;
        }
    }

    /// <summary>Reinicia los contadores tras un login exitoso.</summary>
    public void RegistrarLoginExitoso()
    {
        IntentosFallidos = 0;
        BloqueadoHasta = null;
    }

    /// <summary>Desactiva el usuario.</summary>
    public void Desactivar() => Activo = false;

    /// <summary>Reactiva el usuario.</summary>
    public void Activar() => Activo = true;

    /// <summary>Actualiza el hash del PIN (cambio de PIN).</summary>
    public void CambiarHashPin(string nuevoHash)
    {
        if (!string.IsNullOrWhiteSpace(nuevoHash))
            HashPin = nuevoHash;
    }

    /// <summary>Edita el nombre para mostrar del usuario.</summary>
    public Result<bool> EditarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<bool>.Falla("El nombre es obligatorio.");
        Nombre = nombre.Trim();
        return Result<bool>.Exito(true);
    }

    /// <summary>Normaliza el identificador: minúsculas y sin espacios alrededor.</summary>
    public static string NormalizarIdentificador(string identificador)
        => identificador.Trim().ToLowerInvariant();
}
