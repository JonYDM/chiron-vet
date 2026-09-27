namespace Chiron.Domain.Expedientes;

/// <summary>
/// Tipo de entrada en el expediente médico. Lista cerrada para consistencia y métricas.
/// </summary>
public enum TipoRegistroMedico
{
    /// <summary>Consulta general / revisión.</summary>
    Consulta = 1,

    /// <summary>Aplicación de vacuna.</summary>
    Vacuna = 2,

    /// <summary>Desparasitación.</summary>
    Desparasitacion = 3,

    /// <summary>Cirugía o procedimiento quirúrgico.</summary>
    Cirugia = 4,

    /// <summary>Otro tipo de atención.</summary>
    Otro = 5
}
