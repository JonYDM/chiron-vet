using Chiron.Application.Clientes;
using Chiron.Domain.Clientes;
using Chiron.Domain.Mascotas;

namespace Chiron.Application.Mascotas;

/// <summary>Mascota con el nombre de su dueño (para el listado global de pacientes).</summary>
public sealed record MascotaConDuenoDto(
    Guid Id,
    string Nombre,
    EspecieMascota Especie,
    string? Raza,
    SexoMascota Sexo,
    DateOnly? FechaNacimiento,
    decimal? PesoKg,
    string? Padecimientos,
    bool? Esterilizado,
    bool Activo,
    string? FotoPerfilUrl,
    Guid ClienteId,
    string ClienteNombre);

/// <summary>
/// Caso de uso: listar TODAS las mascotas de la veterinaria (pacientes) con el nombre de
/// su dueño y búsqueda opcional por nombre de mascota. Para la vista Pacientes.
/// </summary>
public sealed class ListarMascotasDeVeterinaria
{
    private readonly IMascotaRepository _mascotas;
    private readonly IClienteRepository _clientes;

    public ListarMascotasDeVeterinaria(IMascotaRepository mascotas, IClienteRepository clientes)
    {
        _mascotas = mascotas;
        _clientes = clientes;
    }

    public async Task<IReadOnlyList<MascotaConDuenoDto>> EjecutarAsync(
        Guid veterinariaId, string? texto = null, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Mascota> mascotas =
            await _mascotas.ListarPorVeterinariaAsync(veterinariaId, cancellationToken);

        // Filtro de búsqueda por nombre de mascota (en memoria; el volumen por vet es bajo).
        if (!string.IsNullOrWhiteSpace(texto))
        {
            string t = texto.Trim();
            mascotas = mascotas
                .Where(m => m.Nombre.Contains(t, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Mapa de nombres de cliente (una consulta) para evitar N+1.
        IReadOnlyList<Cliente> clientes =
            await _clientes.ListarPorVeterinariaAsync(veterinariaId, cancellationToken);
        Dictionary<Guid, string> nombrePorCliente =
            clientes.ToDictionary(c => c.Id, c => c.Nombre);

        return mascotas
            .OrderBy(m => m.Nombre)
            .Select(m => new MascotaConDuenoDto(
                m.Id, m.Nombre, m.Especie, m.Raza, m.Sexo, m.FechaNacimiento, m.PesoKg,
                m.Padecimientos, m.Esterilizado, m.Activo, m.FotoPerfilUrl, m.ClienteId,
                nombrePorCliente.TryGetValue(m.ClienteId, out string? n) ? n : "—"))
            .ToList();
    }
}
