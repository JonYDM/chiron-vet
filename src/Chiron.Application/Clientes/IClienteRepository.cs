using Chiron.Application.Common;
using Chiron.Domain.Clientes;

namespace Chiron.Application.Clientes;

/// <summary>
/// Repositorio específico de Cliente. Extiende el genérico con consultas
/// propias del negocio, siempre acotadas por veterinaria (aislamiento multi-tenant).
/// </summary>
public interface IClienteRepository : IRepository<Cliente>
{
    /// <summary>Obtiene los clientes de una veterinaria cuyo nombre contiene el texto dado.</summary>
    Task<IReadOnlyList<Cliente>> BuscarPorNombreAsync(
        Guid veterinariaId, string texto, CancellationToken cancellationToken = default);

    /// <summary>Obtiene un cliente por su teléfono dentro de una veterinaria, o null si no existe.</summary>
    Task<Cliente?> ObtenerPorTelefonoAsync(
        Guid veterinariaId, string telefono, CancellationToken cancellationToken = default);

    /// <summary>Lista todos los clientes de una veterinaria.</summary>
    Task<IReadOnlyList<Cliente>> ListarPorVeterinariaAsync(
        Guid veterinariaId, CancellationToken cancellationToken = default);
}
