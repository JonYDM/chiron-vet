using Chiron.Domain.Common;

namespace Chiron.Domain.Mascotas;

/// <summary>
/// Mascota (paciente) de la veterinaria. Pertenece a un Cliente (dueño) y a una
/// Veterinaria (tenant). Relación: 1 Cliente → N Mascotas.
///
/// Diseño rico: constructor privado + fábrica Crear que valida.
/// </summary>
public sealed class Mascota : EntidadBase
{
    /// <summary>Veterinaria (tenant) a la que pertenece el registro.</summary>
    public Guid VeterinariaId { get; private set; }

    /// <summary>Cliente (dueño) al que pertenece la mascota. Esta es la relación 1→N.</summary>
    public Guid ClienteId { get; private set; }

    /// <summary>Nombre de la mascota.</summary>
    public string Nombre { get; private set; }

    /// <summary>Especie (perro, gato, etc.).</summary>
    public EspecieMascota Especie { get; private set; }

    /// <summary>Raza (opcional).</summary>
    public string? Raza { get; private set; }

    /// <summary>Sexo de la mascota.</summary>
    public SexoMascota Sexo { get; private set; }

    /// <summary>Fecha de nacimiento (opcional). Permite calcular la edad y recordatorios.</summary>
    public DateOnly? FechaNacimiento { get; private set; }

    /// <summary>Peso actual en kg (opcional).</summary>
    public decimal? PesoKg { get; private set; }

    /// <summary>Padecimientos / condiciones previas (alergias, crónicos…) (opcional).</summary>
    public string? Padecimientos { get; private set; }

    /// <summary>Indica si la mascota está esterilizada (opcional/desconocido = null).</summary>
    public bool? Esterilizado { get; private set; }

    /// <summary>Indica si la mascota está activa (baja lógica; preserva su expediente).</summary>
    public bool Activo { get; private set; }

    /// <summary>URL de la foto de perfil (avatar) de la mascota. Null si no tiene.</summary>
    public string? FotoPerfilUrl { get; private set; }

    /// <summary>Establece (o reemplaza) la URL de la foto de perfil.</summary>
    public void EstablecerFotoPerfil(string url) => FotoPerfilUrl = url;

    private Mascota(
        Guid veterinariaId,
        Guid clienteId,
        string nombre,
        EspecieMascota especie,
        SexoMascota sexo,
        string? raza,
        DateOnly? fechaNacimiento,
        decimal? pesoKg,
        string? padecimientos,
        bool? esterilizado)
    {
        VeterinariaId = veterinariaId;
        ClienteId = clienteId;
        Nombre = nombre;
        Especie = especie;
        Sexo = sexo;
        Raza = raza;
        FechaNacimiento = fechaNacimiento;
        PesoKg = pesoKg;
        Padecimientos = padecimientos;
        Esterilizado = esterilizado;
        Activo = true;
    }

    /// <summary>
    /// Crea una Mascota validando las reglas de negocio.
    /// </summary>
    public static Result<Mascota> Crear(
        Guid veterinariaId,
        Guid clienteId,
        string nombre,
        EspecieMascota especie,
        SexoMascota sexo = SexoMascota.NoEspecificado,
        string? raza = null,
        DateOnly? fechaNacimiento = null,
        decimal? pesoKg = null,
        string? padecimientos = null,
        bool? esterilizado = null)
    {
        if (veterinariaId == Guid.Empty)
            return Result<Mascota>.Falla("La mascota debe pertenecer a una veterinaria válida.");

        if (clienteId == Guid.Empty)
            return Result<Mascota>.Falla("La mascota debe estar asociada a un cliente válido.");

        if (string.IsNullOrWhiteSpace(nombre))
            return Result<Mascota>.Falla("El nombre de la mascota es obligatorio.");

        // La fecha de nacimiento, si se indica, no puede ser futura.
        if (fechaNacimiento is { } fn && fn > DateOnly.FromDateTime(DateTime.UtcNow))
            return Result<Mascota>.Falla("La fecha de nacimiento no puede ser futura.");

        if (pesoKg is { } peso && peso <= 0)
            return Result<Mascota>.Falla("El peso debe ser mayor que cero.");

        string? razaNormalizada = string.IsNullOrWhiteSpace(raza) ? null : raza.Trim();
        string? padecimientosNorm = string.IsNullOrWhiteSpace(padecimientos) ? null : padecimientos.Trim();

        var mascota = new Mascota(
            veterinariaId, clienteId, nombre.Trim(), especie, sexo, razaNormalizada,
            fechaNacimiento, pesoKg, padecimientosNorm, esterilizado);
        return Result<Mascota>.Exito(mascota);
    }

    /// <summary>
    /// Actualiza los datos editables de la mascota. Valida las mismas reglas que Crear.
    /// </summary>
    public Result<bool> ActualizarDatos(
        string nombre,
        EspecieMascota especie,
        SexoMascota sexo,
        string? raza,
        DateOnly? fechaNacimiento,
        decimal? pesoKg,
        string? padecimientos,
        bool? esterilizado)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<bool>.Falla("El nombre de la mascota es obligatorio.");
        if (fechaNacimiento is { } fn && fn > DateOnly.FromDateTime(DateTime.UtcNow))
            return Result<bool>.Falla("La fecha de nacimiento no puede ser futura.");
        if (pesoKg is { } peso && peso <= 0)
            return Result<bool>.Falla("El peso debe ser mayor que cero.");

        Nombre = nombre.Trim();
        Especie = especie;
        Sexo = sexo;
        Raza = string.IsNullOrWhiteSpace(raza) ? null : raza.Trim();
        FechaNacimiento = fechaNacimiento;
        PesoKg = pesoKg;
        Padecimientos = string.IsNullOrWhiteSpace(padecimientos) ? null : padecimientos.Trim();
        Esterilizado = esterilizado;
        return Result<bool>.Exito(true);
    }

    /// <summary>
    /// Calcula la edad en años a partir de la fecha de nacimiento.
    /// Devuelve null si no se conoce la fecha de nacimiento.
    /// </summary>
    public int? EdadEnAnios()
    {
        if (FechaNacimiento is not { } nacimiento)
            return null;

        DateOnly hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        int edad = hoy.Year - nacimiento.Year;

        // Ajusta si aún no ha cumplido años este año.
        if (nacimiento > hoy.AddYears(-edad))
            edad--;

        return edad;
    }

    /// <summary>Da de baja lógica a la mascota (preserva su expediente médico).</summary>
    public void Desactivar() => Activo = false;

    /// <summary>Reactiva una mascota dada de baja.</summary>
    public void Activar() => Activo = true;
}
