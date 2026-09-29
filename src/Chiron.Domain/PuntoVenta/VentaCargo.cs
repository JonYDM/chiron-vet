namespace Chiron.Domain.PuntoVenta;

/// <summary>
/// Cargo (consulta/servicio) cobrado dentro de una Venta. Es distinto de LineaVenta
/// (que es para productos con stock). Guarda el concepto y monto al momento del cobro
/// (histórico fiel) y la referencia al Cargo original, para métricas y trazabilidad.
///
/// Owned type de Venta (no tiene tabla propia con Id de negocio).
/// </summary>
public sealed class VentaCargo
{
    /// <summary>Cargo (cuenta por cobrar) que se cobró.</summary>
    public Guid CargoId { get; private set; }

    /// <summary>Concepto al momento del cobro (ej. "Consulta general").</summary>
    public string Concepto { get; private set; }

    /// <summary>Monto cobrado.</summary>
    public decimal Monto { get; private set; }

    public VentaCargo(Guid cargoId, string concepto, decimal monto)
    {
        CargoId = cargoId;
        Concepto = concepto;
        Monto = monto;
    }

    // Constructor privado sin parámetros para EF Core.
    private VentaCargo()
    {
        Concepto = string.Empty;
    }
}
