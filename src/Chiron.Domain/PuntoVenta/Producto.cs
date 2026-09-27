using Chiron.Domain.Common;

namespace Chiron.Domain.PuntoVenta;

/// <summary>
/// Producto del catálogo de la veterinaria (alimento, medicina, accesorio, etc.).
/// Multi-tenant (pertenece a una Veterinaria). Diseño rico con fábrica Crear.
/// </summary>
public sealed class Producto : EntidadBase
{
    /// <summary>Veterinaria (tenant) dueña del producto.</summary>
    public Guid VeterinariaId { get; private set; }

    /// <summary>Nombre del producto.</summary>
    public string Nombre { get; private set; }

    /// <summary>Categoría del producto.</summary>
    public CategoriaProducto Categoria { get; private set; }

    /// <summary>Precio de venta unitario. Se almacena como decimal (correcto para dinero).</summary>
    public decimal Precio { get; private set; }

    /// <summary>Existencias disponibles en inventario.</summary>
    public int Stock { get; private set; }

    private Producto(Guid veterinariaId, string nombre, CategoriaProducto categoria, decimal precio, int stock)
    {
        VeterinariaId = veterinariaId;
        Nombre = nombre;
        Categoria = categoria;
        Precio = precio;
        Stock = stock;
    }

    /// <summary>
    /// Crea un Producto validando las reglas de negocio.
    /// </summary>
    public static Result<Producto> Crear(
        Guid veterinariaId, string nombre, CategoriaProducto categoria, decimal precio, int stock)
    {
        if (veterinariaId == Guid.Empty)
            return Result<Producto>.Falla("El producto debe pertenecer a una veterinaria válida.");

        if (string.IsNullOrWhiteSpace(nombre))
            return Result<Producto>.Falla("El nombre del producto es obligatorio.");

        // El precio debe ser positivo. (> 0, no >= 0: un producto no se vende en 0.)
        if (precio <= 0)
            return Result<Producto>.Falla("El precio debe ser mayor que cero.");

        // El stock no puede ser negativo. (< 0 inválido; 0 es válido: producto agotado.)
        if (stock < 0)
            return Result<Producto>.Falla("El stock no puede ser negativo.");

        return Result<Producto>.Exito(new Producto(veterinariaId, nombre.Trim(), categoria, precio, stock));
    }

    /// <summary>
    /// Descuenta unidades del stock (al vender). Valida que haya existencias suficientes.
    /// </summary>
    public Result<bool> DescontarStock(int cantidad)
    {
        if (cantidad <= 0)
            return Result<bool>.Falla("La cantidad a descontar debe ser mayor que cero.");
        if (cantidad > Stock)
            return Result<bool>.Falla($"Stock insuficiente de '{Nombre}' (disponible: {Stock}).");

        Stock -= cantidad;
        return Result<bool>.Exito(true);
    }

    /// <summary>Aumenta el stock (al reabastecer).</summary>
    public void ReabastecerStock(int cantidad)
    {
        if (cantidad > 0)
            Stock += cantidad;
    }

    /// <summary>Actualiza el precio del producto.</summary>
    public Result<bool> CambiarPrecio(decimal nuevoPrecio)
    {
        if (nuevoPrecio <= 0)
            return Result<bool>.Falla("El precio debe ser mayor que cero.");
        Precio = nuevoPrecio;
        return Result<bool>.Exito(true);
    }
}
