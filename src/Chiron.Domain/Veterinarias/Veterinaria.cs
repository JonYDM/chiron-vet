using Chiron.Domain.Common;

namespace Chiron.Domain.Veterinarias;

/// <summary>
/// Veterinaria = Tenant (inquilino) del SaaS. Cada veterinaria que renta Chiron
/// es una instancia de esta entidad. Todos los datos operativos (clientes, mascotas,
/// citas, ventas) pertenecen a una Veterinaria y están aislados por ella.
///
/// Diseño rico: constructor privado + fábrica Crear que valida.
/// </summary>
public sealed class Veterinaria : EntidadBase
{
    /// <summary>Nombre comercial de la veterinaria.</summary>
    public string Nombre { get; private set; }

    /// <summary>Teléfono de contacto de la veterinaria.</summary>
    public string Telefono { get; private set; }

    /// <summary>Indica si la suscripción está activa (base para el modelo de renta).</summary>
    public bool Activa { get; private set; }

    /// <summary>
    /// Indica si el Administrador de esta veterinaria puede operar (registrar, vender,
    /// atender) además de ver métricas y gestionar el equipo. Si es false, el Admin es
    /// un supervisor puro (solo métricas + equipo). Lo configura el SuperAdmin.
    /// </summary>
    public bool AdminOperativo { get; private set; }

    /// <summary>Fecha de alta de la veterinaria (UTC).</summary>
    public DateTime FechaAlta { get; private set; }

    private Veterinaria(string nombre, string telefono)
    {
        Nombre = nombre;
        Telefono = telefono;
        Activa = true;
        AdminOperativo = true;
        FechaAlta = DateTime.UtcNow;
    }

    /// <summary>
    /// Crea una Veterinaria validando las reglas de negocio.
    /// </summary>
    public static Result<Veterinaria> Crear(string nombre, string telefono)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<Veterinaria>.Falla("El nombre de la veterinaria es obligatorio.");

        if (string.IsNullOrWhiteSpace(telefono))
            return Result<Veterinaria>.Falla("El teléfono de la veterinaria es obligatorio.");

        var veterinaria = new Veterinaria(nombre.Trim(), telefono.Trim());
        return Result<Veterinaria>.Exito(veterinaria);
    }

    /// <summary>Desactiva la veterinaria (ej: suscripción vencida).</summary>
    public void Desactivar() => Activa = false;

    /// <summary>Reactiva la veterinaria (ej: pago de suscripción).</summary>
    public void Activar() => Activa = true;

    /// <summary>Define si el Administrador puede operar (true) o es supervisor puro (false).</summary>
    public void EstablecerAdminOperativo(bool operativo) => AdminOperativo = operativo;
}
