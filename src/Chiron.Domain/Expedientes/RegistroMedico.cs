using Chiron.Domain.Common;

namespace Chiron.Domain.Expedientes;

/// <summary>
/// Entrada del expediente médico de una mascota (una consulta, vacuna, desparasitación, etc.).
/// Pertenece a una Veterinaria (tenant) y a una Mascota.
///
/// Para vacunas y desparasitaciones puede registrarse una FechaProximaAplicacion,
/// que es la base de los recordatorios automáticos (Épica 5 — diferenciador WhatsApp).
///
/// Diseño rico: constructor privado + fábrica Crear que valida.
/// </summary>
public sealed class RegistroMedico : EntidadBase
{
    /// <summary>Veterinaria (tenant) dueña del registro.</summary>
    public Guid VeterinariaId { get; private set; }

    /// <summary>Mascota (paciente) a la que pertenece el registro.</summary>
    public Guid MascotaId { get; private set; }

    /// <summary>Tipo de registro (consulta, vacuna, etc.).</summary>
    public TipoRegistroMedico Tipo { get; private set; }

    /// <summary>Fecha en que se realizó la atención.</summary>
    public DateOnly Fecha { get; private set; }

    /// <summary>Descripción / notas de la atención.</summary>
    public string Descripcion { get; private set; }

    /// <summary>
    /// Fecha de la próxima aplicación (solo para vacunas/desparasitaciones).
    /// Null si no aplica. Base para recordatorios.
    /// </summary>
    public DateOnly? FechaProximaAplicacion { get; private set; }

    /// <summary>Diagnóstico de la consulta (opcional).</summary>
    public string? Diagnostico { get; private set; }

    /// <summary>Tratamiento administrado/recetado (medicamentos, dosis, indicaciones) (opcional).</summary>
    public string? Tratamiento { get; private set; }

    /// <summary>Peso de la mascota registrado en la consulta, en kg (opcional). Base del histórico de peso.</summary>
    public decimal? PesoKg { get; private set; }

    /// <summary>Temperatura de la mascota en °C (opcional).</summary>
    public decimal? TemperaturaC { get; private set; }

    /// <summary>Notas / observaciones adicionales del veterinario (opcional).</summary>
    public string? Notas { get; private set; }

    private RegistroMedico(
        Guid veterinariaId,
        Guid mascotaId,
        TipoRegistroMedico tipo,
        DateOnly fecha,
        string descripcion,
        DateOnly? fechaProximaAplicacion,
        string? diagnostico,
        string? tratamiento,
        decimal? pesoKg,
        decimal? temperaturaC,
        string? notas)
    {
        VeterinariaId = veterinariaId;
        MascotaId = mascotaId;
        Tipo = tipo;
        Fecha = fecha;
        Descripcion = descripcion;
        FechaProximaAplicacion = fechaProximaAplicacion;
        Diagnostico = diagnostico;
        Tratamiento = tratamiento;
        PesoKg = pesoKg;
        TemperaturaC = temperaturaC;
        Notas = notas;
    }

    /// <summary>
    /// Crea un RegistroMedico validando las reglas de negocio.
    /// </summary>
    public static Result<RegistroMedico> Crear(
        Guid veterinariaId,
        Guid mascotaId,
        TipoRegistroMedico tipo,
        DateOnly fecha,
        string descripcion,
        DateOnly? fechaProximaAplicacion = null,
        string? diagnostico = null,
        string? tratamiento = null,
        decimal? pesoKg = null,
        decimal? temperaturaC = null,
        string? notas = null)
    {
        if (veterinariaId == Guid.Empty)
            return Result<RegistroMedico>.Falla("El registro debe pertenecer a una veterinaria válida.");

        if (mascotaId == Guid.Empty)
            return Result<RegistroMedico>.Falla("El registro debe pertenecer a una mascota válida.");

        if (string.IsNullOrWhiteSpace(descripcion))
            return Result<RegistroMedico>.Falla("La descripción del registro es obligatoria.");

        // La próxima aplicación, si se indica, debe ser posterior a la fecha de atención.
        if (fechaProximaAplicacion is { } proxima && proxima <= fecha)
            return Result<RegistroMedico>.Falla(
                "La fecha de próxima aplicación debe ser posterior a la fecha de atención.");

        // El peso, si se indica, debe ser positivo.
        if (pesoKg is { } peso && peso <= 0)
            return Result<RegistroMedico>.Falla("El peso debe ser mayor que cero.");

        static string? Limpiar(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        var registro = new RegistroMedico(
            veterinariaId, mascotaId, tipo, fecha, descripcion.Trim(), fechaProximaAplicacion,
            Limpiar(diagnostico), Limpiar(tratamiento), pesoKg, temperaturaC, Limpiar(notas));
        return Result<RegistroMedico>.Exito(registro);
    }

    /// <summary>
    /// Indica si este registro genera un recordatorio pendiente a la fecha dada
    /// (tiene próxima aplicación y aún no ha pasado). Base para la Épica 5.
    /// </summary>
    public bool TieneRecordatorioPendiente(DateOnly aFecha)
        => FechaProximaAplicacion is { } proxima && proxima >= aFecha;
}
