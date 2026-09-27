using Chiron.Application.Common;
using Chiron.Domain.Clientes;

namespace Chiron.Application.Clientes;

/// <summary>
/// Caso de uso: buscar/listar clientes de una veterinaria con búsqueda por texto,
/// filtro de estado (activos/inactivos/todos) y paginación. Todo el procesamiento
/// (filtrado, búsqueda, corte de página) se hace en el servidor.
/// </summary>
public sealed class BuscarClientes
{
    private readonly IClienteRepository _clientes;

    public BuscarClientes(IClienteRepository clientes) => _clientes = clientes;

    /// <summary>Versión paginada con filtro de estado.</summary>
    public async Task<ResultadoPaginado<Cliente>> EjecutarAsync(
        Guid veterinariaId,
        string? texto,
        FiltroEstado estado,
        int pagina,
        int tamanoPagina,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Cliente> baseLista = string.IsNullOrWhiteSpace(texto)
            ? await _clientes.ListarPorVeterinariaAsync(veterinariaId, cancellationToken)
            : await _clientes.BuscarPorNombreAsync(veterinariaId, texto.Trim(), cancellationToken);

        var filtrados = baseLista
            .AplicarFiltro(estado, c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToList();

        return ResultadoPaginado<Cliente>.Crear(filtrados, pagina, tamanoPagina);
    }
}
