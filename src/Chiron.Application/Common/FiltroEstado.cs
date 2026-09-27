namespace Chiron.Application.Common;

/// <summary>
/// Filtro de estado para los listados: permite ver activos, inactivos o todos.
/// Se procesa en el servidor (la capa de aplicación decide qué devolver).
/// </summary>
public enum FiltroEstado
{
    /// <summary>Solo registros activos (valor por defecto).</summary>
    Activos = 0,

    /// <summary>Solo registros inactivos (dados de baja).</summary>
    Inactivos = 1,

    /// <summary>Todos, sin importar el estado.</summary>
    Todos = 2,
}

/// <summary>Utilidad para aplicar el filtro de estado a una colección en memoria.</summary>
public static class FiltroEstadoExtensiones
{
    /// <summary>Filtra por estado usando un selector del campo Activo de cada elemento.</summary>
    public static IEnumerable<T> AplicarFiltro<T>(
        this IEnumerable<T> fuente, FiltroEstado filtro, Func<T, bool> esActivo)
        => filtro switch
        {
            FiltroEstado.Activos => fuente.Where(esActivo),
            FiltroEstado.Inactivos => fuente.Where(e => !esActivo(e)),
            _ => fuente,
        };
}
