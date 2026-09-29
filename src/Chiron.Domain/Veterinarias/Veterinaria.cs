using Chiron.Domain.Common;

namespace Chiron.Domain.Veterinarias;

/// <summary>Plan de suscripción de la veterinaria (define el periodo de renovación).</summary>
public enum PlanSuscripcion
{
    Mensual = 1,
    Anual = 2,
}

/// <summary>
/// Veterinaria = Tenant (inquilino) del SaaS. Cada veterinaria que renta Patwi
/// es una instancia de esta entidad. Todos los datos operativos (clientes, mascotas,
/// citas, ventas) pertenecen a una Veterinaria y están aislados por ella.
///
/// Suscripción: tiene un Plan (mensual/anual) y una FechaRenovacion calculada a partir
/// del alta o de la última renovación; el SuperAdmin puede ajustarla a mano.
///
/// Diseño rico: constructor privado + fábrica Crear que valida.
/// </summary>
public sealed class Veterinaria : EntidadBase
{
    /// <summary>Nombre comercial de la veterinaria.</summary>
    public string Nombre { get; private set; }

    /// <summary>Teléfono de contacto de la veterinaria.</summary>
    public string Telefono { get; private set; }

    /// <summary>Dirección de la veterinaria (opcional).</summary>
    public string? Direccion { get; private set; }

    /// <summary>Indica si la suscripción está activa (base para el modelo de renta).</summary>
    public bool Activa { get; private set; }

    /// <summary>
    /// Obsoleto para control de módulos (el Admin tiene acceso completo). Se conserva por
    /// compatibilidad de datos/endpoints existentes.
    /// </summary>
    public bool AdminOperativo { get; private set; }

    /// <summary>Fecha de alta de la veterinaria (UTC).</summary>
    public DateTime FechaAlta { get; private set; }

    /// <summary>Plan de suscripción (mensual/anual).</summary>
    public PlanSuscripcion Plan { get; private set; }

    /// <summary>Fecha en que vence/renueva la suscripción (UTC, solo fecha).</summary>
    public DateOnly FechaRenovacion { get; private set; }

    private Veterinaria(string nombre, string telefono, string? direccion, PlanSuscripcion plan)
    {
        Nombre = nombre;
        Telefono = telefono;
        Direccion = direccion;
        Activa = true;
        AdminOperativo = true;
        FechaAlta = DateTime.UtcNow;
        Plan = plan;
        FechaRenovacion = SiguientePeriodo(DateOnly.FromDateTime(FechaAlta), plan);
    }

    // Constructor privado sin parámetros para EF Core.
    private Veterinaria()
    {
        Nombre = string.Empty;
        Telefono = string.Empty;
    }

    /// <summary>Crea una Veterinaria validando las reglas de negocio.</summary>
    public static Result<Veterinaria> Crear(string nombre, string telefono,
        string? direccion = null, PlanSuscripcion plan = PlanSuscripcion.Mensual)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<Veterinaria>.Falla("El nombre de la veterinaria es obligatorio.");

        if (string.IsNullOrWhiteSpace(telefono))
            return Result<Veterinaria>.Falla("El teléfono de la veterinaria es obligatorio.");

        if (!Enum.IsDefined(plan))
            return Result<Veterinaria>.Falla("El plan de suscripción no es válido.");

        string? dir = string.IsNullOrWhiteSpace(direccion) ? null : direccion.Trim();
        return Result<Veterinaria>.Exito(new Veterinaria(nombre.Trim(), telefono.Trim(), dir, plan));
    }

    /// <summary>Edita los datos generales (nombre, teléfono, dirección, plan).</summary>
    public Result<bool> Editar(string nombre, string telefono, string? direccion, PlanSuscripcion plan)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<bool>.Falla("El nombre de la veterinaria es obligatorio.");
        if (string.IsNullOrWhiteSpace(telefono))
            return Result<bool>.Falla("El teléfono de la veterinaria es obligatorio.");
        if (!Enum.IsDefined(plan))
            return Result<bool>.Falla("El plan de suscripción no es válido.");

        Nombre = nombre.Trim();
        Telefono = telefono.Trim();
        Direccion = string.IsNullOrWhiteSpace(direccion) ? null : direccion.Trim();
        Plan = plan;
        return Result<bool>.Exito(true);
    }

    /// <summary>
    /// Renueva la suscripción: extiende un periodo según el Plan. Si ya venció, cuenta desde
    /// hoy; si aún está vigente, se suma al vencimiento actual (no se pierden días pagados).
    /// Reactiva la veterinaria si estaba inactiva.
    /// </summary>
    public void Renovar(DateOnly hoy)
    {
        DateOnly desde = FechaRenovacion > hoy ? FechaRenovacion : hoy;
        FechaRenovacion = SiguientePeriodo(desde, Plan);
        Activa = true;
    }

    /// <summary>Ajuste manual de la fecha de renovación (pagos irregulares, prórrogas).</summary>
    public void AjustarRenovacion(DateOnly nuevaFecha) => FechaRenovacion = nuevaFecha;

    /// <summary>Desactiva la veterinaria (ej: suscripción vencida).</summary>
    public void Desactivar() => Activa = false;

    /// <summary>Reactiva la veterinaria (ej: pago de suscripción).</summary>
    public void Activar() => Activa = true;

    /// <summary>Obsoleto: se conserva por compatibilidad.</summary>
    public void EstablecerAdminOperativo(bool operativo) => AdminOperativo = operativo;

    private static DateOnly SiguientePeriodo(DateOnly desde, PlanSuscripcion plan)
        => plan == PlanSuscripcion.Anual ? desde.AddYears(1) : desde.AddMonths(1);
}
