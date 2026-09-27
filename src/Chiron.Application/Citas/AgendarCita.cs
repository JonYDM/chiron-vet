using Chiron.Application.Mascotas;
using Chiron.Domain.Citas;
using Chiron.Domain.Common;
using Chiron.Domain.Mascotas;

namespace Chiron.Application.Citas;

/// <summary>
/// Datos de entrada para agendar una cita.
/// </summary>
public sealed record AgendarCitaComando(
    Guid VeterinariaId,
    Guid MascotaId,
    DateTime FechaHora,
    string Motivo);

/// <summary>
/// Caso de uso: agendar una cita para una mascota (H4.1).
/// </summary>
public sealed class AgendarCita
{
    private readonly ICitaRepository _citas;
    private readonly IMascotaRepository _mascotas;

    public AgendarCita(ICitaRepository citas, IMascotaRepository mascotas)
    {
        _citas = citas;
        _mascotas = mascotas;
    }

    public async Task<Result<Guid>> EjecutarAsync(
        AgendarCitaComando comando, CancellationToken cancellationToken = default)
    {
        // La mascota debe existir y pertenecer a la veterinaria (aislamiento multi-tenant).
        Mascota? mascota = await _mascotas.ObtenerPorIdAsync(comando.MascotaId, cancellationToken);
        if (mascota is null)
            return Result<Guid>.Falla("La mascota indicada no existe.");
        if (mascota.VeterinariaId != comando.VeterinariaId)
            return Result<Guid>.Falla("La mascota no pertenece a la veterinaria indicada.");

        Result<Cita> citaResult = Cita.Crear(
            comando.VeterinariaId, comando.MascotaId, comando.FechaHora, comando.Motivo);
        if (!citaResult.EsExito)
            return Result<Guid>.Falla(citaResult.Error!);

        Cita cita = citaResult.Valor!;
        await _citas.AgregarAsync(cita, cancellationToken);
        return Result<Guid>.Exito(cita.Id);
    }
}
