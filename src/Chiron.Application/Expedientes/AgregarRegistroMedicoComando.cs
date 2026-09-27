using Chiron.Domain.Expedientes;

namespace Chiron.Application.Expedientes;

/// <summary>
/// Datos de entrada para agregar una entrada al expediente médico de una mascota.
/// Cubre consultas (H3.1) y vacunas/desparasitaciones con próxima aplicación (H3.2).
/// </summary>
public sealed record AgregarRegistroMedicoComando(
    Guid VeterinariaId,
    Guid MascotaId,
    TipoRegistroMedico Tipo,
    DateOnly Fecha,
    string Descripcion,
    DateOnly? FechaProximaAplicacion = null);
