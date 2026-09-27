using Chiron.Application.Citas;
using Chiron.Application.Clientes;
using Chiron.Application.Expedientes;
using Chiron.Application.Mascotas;
using Chiron.Domain.Citas;
using Chiron.Domain.Clientes;
using Chiron.Domain.Expedientes;
using Chiron.Domain.Mascotas;

namespace Chiron.Application.Recordatorios;

/// <summary>
/// Caso de uso: detectar recordatorios pendientes de una veterinaria dentro de una
/// ventana de días (H5.1) y generar los mensajes correspondientes (H5.2).
///
/// Combina dos orígenes:
///  - Próximas aplicaciones (vacunas/desparasitaciones) del expediente médico.
///  - Citas programadas próximas.
///
/// Respeta el consentimiento (opt-in) de WhatsApp del cliente: si no aceptó, se omite.
/// </summary>
public sealed class GenerarRecordatorios
{
    private readonly IRegistroMedicoRepository _registros;
    private readonly ICitaRepository _citas;
    private readonly IMascotaRepository _mascotas;
    private readonly IClienteRepository _clientes;

    public GenerarRecordatorios(
        IRegistroMedicoRepository registros,
        ICitaRepository citas,
        IMascotaRepository mascotas,
        IClienteRepository clientes)
    {
        _registros = registros;
        _citas = citas;
        _mascotas = mascotas;
        _clientes = clientes;
    }

    /// <summary>
    /// Detecta los recordatorios de la veterinaria para los próximos <paramref name="diasAnticipacion"/> días.
    /// Solo incluye clientes que dieron consentimiento de WhatsApp.
    /// </summary>
    public async Task<IReadOnlyList<RecordatorioDetectado>> DetectarAsync(
        Guid veterinariaId, int diasAnticipacion = 7, CancellationToken cancellationToken = default)
    {
        DateOnly hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly hasta = hoy.AddDays(diasAnticipacion);

        var recordatorios = new List<RecordatorioDetectado>();

        // Caché de clientes y mascotas para no consultar repetidamente (eficiencia).
        var cacheClientes = new Dictionary<Guid, Cliente?>();
        var cacheMascotas = new Dictionary<Guid, Mascota?>();

        // ── 1. Próximas aplicaciones (vacunas / desparasitaciones) ──
        IReadOnlyList<RegistroMedico> proximas =
            await _registros.ObtenerProximasAplicacionesAsync(veterinariaId, hoy, hasta, cancellationToken);

        foreach (RegistroMedico r in proximas)
        {
            Mascota? mascota = await ObtenerMascotaAsync(cacheMascotas, r.MascotaId, cancellationToken);
            if (mascota is null) continue;

            Cliente? cliente = await ObtenerClienteAsync(cacheClientes, mascota.ClienteId, cancellationToken);
            if (cliente is null || !cliente.AceptaWhatsApp) continue;

            recordatorios.Add(new RecordatorioDetectado(
                TipoRecordatorio.ProximaAplicacion, cliente.Id, cliente.Nombre, cliente.Telefono,
                mascota.Nombre, r.Descripcion, r.FechaProximaAplicacion!.Value));
        }

        // ── 2. Citas próximas ──
        IReadOnlyList<Cita> citas = await _citas.ObtenerProximasAsync(veterinariaId, DateTime.UtcNow, cancellationToken);
        foreach (Cita c in citas)
        {
            DateOnly diaCita = DateOnly.FromDateTime(c.FechaHora);
            if (diaCita > hasta) continue;  // fuera de la ventana de anticipación

            Mascota? mascota = await ObtenerMascotaAsync(cacheMascotas, c.MascotaId, cancellationToken);
            if (mascota is null) continue;

            Cliente? cliente = await ObtenerClienteAsync(cacheClientes, mascota.ClienteId, cancellationToken);
            if (cliente is null || !cliente.AceptaWhatsApp) continue;

            recordatorios.Add(new RecordatorioDetectado(
                TipoRecordatorio.Cita, cliente.Id, cliente.Nombre, cliente.Telefono,
                mascota.Nombre, c.Motivo, diaCita));
        }

        return recordatorios;
    }

    private async Task<Mascota?> ObtenerMascotaAsync(
        Dictionary<Guid, Mascota?> cache, Guid id, CancellationToken ct)
    {
        if (!cache.TryGetValue(id, out Mascota? mascota))
        {
            mascota = await _mascotas.ObtenerPorIdAsync(id, ct);
            cache[id] = mascota;
        }
        return mascota;
    }

    private async Task<Cliente?> ObtenerClienteAsync(
        Dictionary<Guid, Cliente?> cache, Guid id, CancellationToken ct)
    {
        if (!cache.TryGetValue(id, out Cliente? cliente))
        {
            cliente = await _clientes.ObtenerPorIdAsync(id, ct);
            cache[id] = cliente;
        }
        return cliente;
    }
}
