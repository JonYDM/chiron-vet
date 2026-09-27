using Chiron.Domain.Common;
using Chiron.Domain.PuntoVenta;

namespace Chiron.Application.PuntoVenta;

/// <summary>Datos de entrada para agregar un producto al catálogo.</summary>
public sealed record AgregarProductoComando(
    Guid VeterinariaId,
    string Nombre,
    CategoriaProducto Categoria,
    decimal Precio,
    int Stock);

/// <summary>
/// Caso de uso: agregar un producto al catálogo de la veterinaria (H6.1).
/// </summary>
public sealed class AgregarProducto
{
    private readonly IProductoRepository _productos;

    public AgregarProducto(IProductoRepository productos) => _productos = productos;

    public async Task<Result<Guid>> EjecutarAsync(
        AgregarProductoComando comando, CancellationToken cancellationToken = default)
    {
        Result<Producto> resultado = Producto.Crear(
            comando.VeterinariaId, comando.Nombre, comando.Categoria, comando.Precio, comando.Stock);
        if (!resultado.EsExito)
            return Result<Guid>.Falla(resultado.Error!);

        Producto producto = resultado.Valor!;
        await _productos.AgregarAsync(producto, cancellationToken);
        return Result<Guid>.Exito(producto.Id);
    }
}

/// <summary>
/// Caso de uso: listar el catálogo de productos de una veterinaria.
/// </summary>
public sealed class ListarCatalogo
{
    private readonly IProductoRepository _productos;

    public ListarCatalogo(IProductoRepository productos) => _productos = productos;

    public Task<IReadOnlyList<Producto>> EjecutarAsync(
        Guid veterinariaId, CancellationToken cancellationToken = default)
        => FiltrarActivosAsync(veterinariaId, cancellationToken);

    private async Task<IReadOnlyList<Producto>> FiltrarActivosAsync(
        Guid veterinariaId, CancellationToken cancellationToken)
    {
        IReadOnlyList<Producto> todos = await _productos.ListarPorVeterinariaAsync(veterinariaId, cancellationToken);
        return todos.Where(p => p.Activo).ToList();
    }
}
