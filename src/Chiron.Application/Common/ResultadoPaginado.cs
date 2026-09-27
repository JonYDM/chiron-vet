namespace Chiron.Application.Common;

/// <summary>
/// Resultado paginado genérico para listados. El servidor hace la paginación
/// (no el cliente): devuelve solo la página pedida más el total para calcular
/// el número de páginas.
/// </summary>
/// <typeparam name="T">Tipo de los elementos de la página.</typeparam>
public sealed record ResultadoPaginado<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Pagina,
    int TamanoPagina)
{
    /// <summary>Número total de páginas.</summary>
    public int TotalPaginas => TamanoPagina > 0
        ? (int)Math.Ceiling(Total / (double)TamanoPagina)
        : 0;

    /// <summary>Crea una página a partir de la lista completa ya filtrada (paginación en memoria).</summary>
    public static ResultadoPaginado<T> Crear(
        IReadOnlyList<T> todos, int pagina, int tamanoPagina)
    {
        int p = pagina < 1 ? 1 : pagina;
        int size = tamanoPagina is < 1 or > 100 ? 20 : tamanoPagina;
        var items = todos.Skip((p - 1) * size).Take(size).ToList();
        return new ResultadoPaginado<T>(items, todos.Count, p, size);
    }
}
