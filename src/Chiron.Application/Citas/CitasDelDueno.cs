using Chiron.Application.Mascotas;
using Chiron.Domain.Citas;
using Chiron.Domain.Common;
using Chiron.Domain.Mascotas;

namespace Chiron.Application.Citas;

/// <summary>Cita vista por el dueño en su portal (solo las de SUS mascotas).</summary>
public sealed record MiCitaDto(
    Guid Id,
    Guid MascotaId,
    string MascotaNombre,
    DateTime FechaHora,
    string Motivo,
    EstadoCita Estado,
    ConfirmacionCita Confirmacion);

/// <summary>
/// Casos de uso del portal sobre citas: listar las citas de mis mascotas y responder si voy
/// o no. Siempre valida que la cita sea de una mascota del cliente del token.
/// </summary>
public sealed class CitasDelDueno
{
    private readonly ICitaRepository _citas;
    private readonly IMascotaRepository _mascotas;

    public CitasDelDueno(ICitaRepository citas, IMascotaRepository mascotas)
    {
        _citas = citas;
        _mascotas = mascotas;
    }

    /// <summary>Todas las citas de mis mascotas (más recientes primero; el front separa próximas e historial).</summary>
    public async Task<IReadOnlyList<MiCitaDto>> ListarAsync(
        Guid clienteId, Guid veterinariaId, CancellationToken ct = default)
    {
        IReadOnlyList<Mascota> mias = await _mascotas.ObtenerPorClienteAsync(clienteId, ct);
        var nombres = mias.ToDictionary(m => m.Id, m => m.Nombre);
        if (nombres.Count == 0)
            return [];

        IReadOnlyList<Cita> citas = await _citas.ListarPorVeterinariaAsync(veterinariaId, null, ct);
        return citas
            .Where(c => nombres.ContainsKey(c.MascotaId))
            .OrderByDescending(c => c.FechaHora)
            .Select(c => new MiCitaDto(c.Id, c.MascotaId, nombres[c.MascotaId], c.FechaHora, c.Motivo, c.Estado, c.Confirmacion))
            .ToList();
    }

    /// <summary>El dueño responde si asistirá. Falla si la cita no es de una de sus mascotas.</summary>
    public async Task<Result<MiCitaDto>> ResponderAsync(
        Guid clienteId, Guid citaId, bool asistira, CancellationToken ct = default)
    {
        Cita? cita = await _citas.ObtenerPorIdAsync(citaId, ct);
        if (cita is null)
            return Result<MiCitaDto>.Falla("La cita no existe.");

        Mascota? mascota = await _mascotas.ObtenerPorIdAsync(cita.MascotaId, ct);
        if (mascota is null || mascota.ClienteId != clienteId)
            return Result<MiCitaDto>.Falla("La cita no existe.");  // mismo mensaje: no revelar citas ajenas

        Result<bool> r = cita.ResponderAsistencia(asistira);
        if (!r.EsExito)
            return Result<MiCitaDto>.Falla(r.Error!);

        await _citas.ActualizarAsync(cita, ct);
        return Result<MiCitaDto>.Exito(new MiCitaDto(
            cita.Id, cita.MascotaId, mascota.Nombre, cita.FechaHora, cita.Motivo, cita.Estado, cita.Confirmacion));
    }
}
