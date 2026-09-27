using Chiron.Application.Common;
using Chiron.Domain.Mascotas;

namespace Chiron.Application.Mascotas;

/// <summary>
/// Repositorio específico de Mascota. Extiende el genérico con consultas
/// propias del negocio, acotadas por veterinaria/cliente.
/// </summary>
public interface IMascotaRepository : IRepository<Mascota>
{
    /// <summary>Obtiene todas las mascotas de un cliente (dueño).</summary>
    Task<IReadOnlyList<Mascota>> ObtenerPorClienteAsync(
        Guid clienteId, CancellationToken cancellationToken = default);

    /// <summary>Lista todas las mascotas de una veterinaria.</summary>
    Task<IReadOnlyList<Mascota>> ListarPorVeterinariaAsync(
        Guid veterinariaId, CancellationToken cancellationToken = default);
}
