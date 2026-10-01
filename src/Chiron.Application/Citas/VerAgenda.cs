using Chiron.Application.Clientes;
using Chiron.Application.Mascotas;
using Chiron.Domain.Citas;
using Chiron.Domain.Clientes;
using Chiron.Domain.Mascotas;

namespace Chiron.Application.Citas;

/// <summary>
/// Caso de uso: consultar la agenda de una veterinaria (H4.2).
/// </summary>
public sealed class VerAgenda
{
    private readonly ICitaRepository _citas;
    private readonly IMascotaRepository _mascotas;
    private readonly IClienteRepository _clientes;

    public VerAgenda(ICitaRepository citas, IMascotaRepository mascotas, IClienteRepository clientes)
    {
        _citas = citas;
        _mascotas = mascotas;
        _clientes = clientes;
    }

    /// <summary>Agenda de un día concreto.</summary>
    public Task<IReadOnlyList<Cita>> DelDiaAsync(
        Guid veterinariaId, DateOnly dia, CancellationToken cancellationToken = default)
        => _citas.ObtenerAgendaDelDiaAsync(veterinariaId, dia, cancellationToken);

    /// <summary>Próximas citas programadas a partir de ahora.</summary>
    public Task<IReadOnlyList<Cita>> ProximasAsync(
        Guid veterinariaId, CancellationToken cancellationToken = default)
        => _citas.ObtenerProximasAsync(veterinariaId, DateTime.UtcNow, cancellationToken);

    /// <summary>
    /// Lista las citas de la veterinaria (opcionalmente por estado), enriquecidas con el
    /// nombre de la mascota y del dueño. Ordenadas por fecha descendente (historial).
    /// </summary>
    public async Task<IReadOnlyList<CitaDto>> ListarAsync(
        Guid veterinariaId, EstadoCita? estado, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Cita> citas =
            await _citas.ListarPorVeterinariaAsync(veterinariaId, estado, cancellationToken);

        // Cachés para no consultar la misma mascota/cliente repetidamente.
        var cacheMascotas = new Dictionary<Guid, Mascota?>();
        var cacheClientes = new Dictionary<Guid, Cliente?>();
        var resultado = new List<CitaDto>(citas.Count);

        foreach (Cita c in citas)
        {
            if (!cacheMascotas.TryGetValue(c.MascotaId, out Mascota? mascota))
            {
                mascota = await _mascotas.ObtenerPorIdAsync(c.MascotaId, cancellationToken);
                cacheMascotas[c.MascotaId] = mascota;
            }

            string mascotaNombre = mascota?.Nombre ?? "—";
            string clienteNombre = "—";
            if (mascota is not null)
            {
                if (!cacheClientes.TryGetValue(mascota.ClienteId, out Cliente? cliente))
                {
                    cliente = await _clientes.ObtenerPorIdAsync(mascota.ClienteId, cancellationToken);
                    cacheClientes[mascota.ClienteId] = cliente;
                }
                clienteNombre = cliente?.Nombre ?? "—";
            }

            resultado.Add(new CitaDto(
                c.Id, c.MascotaId, mascotaNombre, clienteNombre,
                c.FechaHora, c.Motivo, c.Estado, c.VeterinarioId, c.Confirmacion));
        }

        return resultado;
    }
}
