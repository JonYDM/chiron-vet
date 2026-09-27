using Chiron.Application.Mascotas;
using Chiron.Domain.Mascotas;

namespace Chiron.Infrastructure.Persistencia;

/// <summary>
/// Implementación en memoria de IMascotaRepository.
/// Reutiliza la base genérica y añade consultas por cliente y por veterinaria.
/// </summary>
public sealed class MascotaRepositorioEnMemoria : RepositorioEnMemoria<Mascota>, IMascotaRepository
{
    public async Task<IReadOnlyList<Mascota>> ObtenerPorClienteAsync(
        Guid clienteId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Mascota> todas = await ObtenerTodosAsync(cancellationToken);
        return todas.Where(m => m.ClienteId == clienteId).ToList();
    }

    public async Task<IReadOnlyList<Mascota>> ListarPorVeterinariaAsync(
        Guid veterinariaId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Mascota> todas = await ObtenerTodosAsync(cancellationToken);
        return todas.Where(m => m.VeterinariaId == veterinariaId).ToList();
    }
}
