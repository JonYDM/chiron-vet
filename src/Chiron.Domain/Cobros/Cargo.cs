using Chiron.Domain.Common;

namespace Chiron.Domain.Cobros;

/// <summary>Estado de un cargo (cuenta por cobrar).</summary>
public enum EstadoCargo
{
    /// <summary>Generado por el veterinario, aún no cobrado en caja.</summary>
    Pendiente = 1,

    /// <summary>Ya cobrado (ligado a una venta).</summary>
    Cobrado = 2,

    /// <summary>Cancelado sin cobrar.</summary>
    Cancelado = 3,
}

/// <summary>
/// Cargo = cuenta por cobrar generada por el staff clínico (típicamente el veterinario
/// al registrar una consulta/servicio). Queda PENDIENTE hasta que la caja lo cobra en el
/// POS, momento en que se liga a la venta y pasa a COBRADO. Separa "quién genera el cargo"
/// de "quién lo cobra" (el veterinario no toca caja).
///
/// Monto libre (el staff escribe el importe). Multi-tenant por VeterinariaId.
/// Diseño rico: constructor privado + fábrica Crear que valida.
/// </summary>
public sealed class Cargo : EntidadBase
{
    /// <summary>Veterinaria (tenant) dueña del cargo.</summary>
    public Guid VeterinariaId { get; private set; }

    /// <summary>Mascota (paciente) del servicio cobrado.</summary>
    public Guid MascotaId { get; private set; }

    /// <summary>Cliente (dueño) a quien se le cobra.</summary>
    public Guid ClienteId { get; private set; }

    /// <summary>Concepto del cargo (ej. "Consulta general", "Curación").</summary>
    public string Concepto { get; private set; }

    /// <summary>Importe a cobrar (monto libre, en la moneda de la veterinaria).</summary>
    public decimal Monto { get; private set; }

    /// <summary>Estado actual del cargo.</summary>
    public EstadoCargo Estado { get; private set; }

    /// <summary>Registro médico (consulta) que originó el cargo, si aplica.</summary>
    public Guid? RegistroMedicoId { get; private set; }

    /// <summary>Venta con la que se cobró el cargo (cuando pasa a Cobrado).</summary>
    public Guid? VentaId { get; private set; }

    /// <summary>Fecha de creación del cargo (UTC).</summary>
    public DateTime FechaCreacion { get; private set; }

    private Cargo(Guid veterinariaId, Guid mascotaId, Guid clienteId, string concepto,
        decimal monto, Guid? registroMedicoId)
    {
        VeterinariaId = veterinariaId;
        MascotaId = mascotaId;
        ClienteId = clienteId;
        Concepto = concepto;
        Monto = monto;
        Estado = EstadoCargo.Pendiente;
        RegistroMedicoId = registroMedicoId;
        FechaCreacion = DateTime.UtcNow;
    }

    // Constructor privado sin parámetros para EF Core.
    private Cargo()
    {
        Concepto = string.Empty;
    }

    /// <summary>Crea un cargo pendiente validando las reglas de negocio.</summary>
    public static Result<Cargo> Crear(Guid veterinariaId, Guid mascotaId, Guid clienteId,
        string concepto, decimal monto, Guid? registroMedicoId = null)
    {
        if (veterinariaId == Guid.Empty)
            return Result<Cargo>.Falla("El cargo debe pertenecer a una veterinaria válida.");
        if (mascotaId == Guid.Empty)
            return Result<Cargo>.Falla("El cargo debe estar asociado a una mascota.");
        if (clienteId == Guid.Empty)
            return Result<Cargo>.Falla("El cargo debe estar asociado a un cliente.");
        if (string.IsNullOrWhiteSpace(concepto))
            return Result<Cargo>.Falla("El concepto del cargo es obligatorio.");
        if (monto <= 0)
            return Result<Cargo>.Falla("El monto del cargo debe ser mayor a cero.");

        return Result<Cargo>.Exito(
            new Cargo(veterinariaId, mascotaId, clienteId, concepto.Trim(), monto, registroMedicoId));
    }

    /// <summary>Marca el cargo como cobrado, ligándolo a la venta. Solo si estaba pendiente.</summary>
    public Result<bool> MarcarCobrado(Guid ventaId)
    {
        if (Estado != EstadoCargo.Pendiente)
            return Result<bool>.Falla($"No se puede cobrar un cargo en estado {Estado}.");
        Estado = EstadoCargo.Cobrado;
        VentaId = ventaId;
        return Result<bool>.Exito(true);
    }

    /// <summary>Cancela el cargo. Solo si estaba pendiente.</summary>
    public Result<bool> Cancelar()
    {
        if (Estado != EstadoCargo.Pendiente)
            return Result<bool>.Falla($"No se puede cancelar un cargo en estado {Estado}.");
        Estado = EstadoCargo.Cancelado;
        return Result<bool>.Exito(true);
    }
}
