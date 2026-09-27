using Chiron.Application.Mascotas;
using Chiron.Domain.Common;
using Chiron.Domain.Expedientes;
using Chiron.Domain.Mascotas;

namespace Chiron.Application.Expedientes;

/// <summary>
/// Caso de uso: agregar una entrada al expediente médico de una mascota.
/// Sirve tanto para consultas (H3.1) como para vacunas/desparasitaciones (H3.2).
/// </summary>
public sealed class AgregarRegistroMedico
{
    private readonly IRegistroMedicoRepository _registros;
    private readonly IMascotaRepository _mascotas;

    public AgregarRegistroMedico(IRegistroMedicoRepository registros, IMascotaRepository mascotas)
    {
        _registros = registros;
        _mascotas = mascotas;
    }

    public async Task<Result<Guid>> EjecutarAsync(
        AgregarRegistroMedicoComando comando, CancellationToken cancellationToken = default)
    {
        // La mascota debe existir y pertenecer a la misma veterinaria (aislamiento multi-tenant).
        Mascota? mascota = await _mascotas.ObtenerPorIdAsync(comando.MascotaId, cancellationToken);
        if (mascota is null)
            return Result<Guid>.Falla("La mascota indicada no existe.");
        if (mascota.VeterinariaId != comando.VeterinariaId)
            return Result<Guid>.Falla("La mascota no pertenece a la veterinaria indicada.");

        Result<RegistroMedico> registroResult = RegistroMedico.Crear(
            comando.VeterinariaId, comando.MascotaId, comando.Tipo,
            comando.Fecha, comando.Descripcion, comando.FechaProximaAplicacion,
            comando.Diagnostico, comando.Tratamiento, comando.PesoKg,
            comando.TemperaturaC, comando.Notas, comando.AtendidoPorId);
        if (!registroResult.EsExito)
            return Result<Guid>.Falla(registroResult.Error!);

        RegistroMedico registro = registroResult.Valor!;
        await _registros.AgregarAsync(registro, cancellationToken);

        return Result<Guid>.Exito(registro.Id);
    }
}
