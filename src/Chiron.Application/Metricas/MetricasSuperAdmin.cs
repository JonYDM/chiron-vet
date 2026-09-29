using Chiron.Application.Common;
using Chiron.Application.Seguridad;
using Chiron.Domain.Usuarios;
using Chiron.Domain.Veterinarias;

namespace Chiron.Application.Metricas;

/// <summary>Veterinaria con renovación próxima o vencida (para "a quién cobrar").</summary>
public sealed record RenovacionProximaDto(
    Guid Id,
    string Nombre,
    PlanSuscripcion Plan,
    DateOnly FechaRenovacion,
    int DiasRestantes,
    bool Activa);

/// <summary>Panorama general de la plataforma para el SuperAdmin (calculado en servidor).</summary>
public sealed record MetricasSuperAdminDto(
    int TotalVeterinarias,
    int VeterinariasActivas,
    int VeterinariasInactivas,
    int PorVencer,
    int Vencidas,
    int PlanMensual,
    int PlanAnual,
    int AltasMes,
    int AdministradoresActivos,
    int VeterinariasSinAdmin,
    IReadOnlyList<RenovacionProximaDto> ProximasRenovaciones);

/// <summary>
/// Caso de uso: métricas globales del SaaS para el SuperAdmin — estado de las
/// suscripciones (activas, por vencer, vencidas), mezcla de planes, altas del mes,
/// administradores y veterinarias activas que aún no tienen administrador.
/// </summary>
public sealed class MetricasSuperAdmin
{
    /// <summary>Días antes del vencimiento en que una suscripción cuenta como "por vencer".</summary>
    public const int DiasAviso = 7;

    private readonly IRepository<Veterinaria> _veterinarias;
    private readonly IUsuarioRepository _usuarios;

    public MetricasSuperAdmin(IRepository<Veterinaria> veterinarias, IUsuarioRepository usuarios)
    {
        _veterinarias = veterinarias;
        _usuarios = usuarios;
    }

    public async Task<MetricasSuperAdminDto> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        DateOnly hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        DateTime ahora = DateTime.UtcNow;

        IReadOnlyList<Veterinaria> vets = await _veterinarias.ObtenerTodosAsync(cancellationToken);
        IReadOnlyList<Usuario> admins = await _usuarios.ListarPorRolAsync(RolUsuario.Administrador, cancellationToken);

        int Dias(Veterinaria v) => v.FechaRenovacion.DayNumber - hoy.DayNumber;

        var vetsConAdmin = admins.Where(a => a.Activo).Select(a => a.VeterinariaId).ToHashSet();

        var proximas = vets
            .Where(v => Dias(v) <= DiasAviso)
            .OrderBy(Dias)
            .Take(5)
            .Select(v => new RenovacionProximaDto(v.Id, v.Nombre, v.Plan, v.FechaRenovacion, Dias(v), v.Activa))
            .ToList();

        return new MetricasSuperAdminDto(
            TotalVeterinarias: vets.Count,
            VeterinariasActivas: vets.Count(v => v.Activa),
            VeterinariasInactivas: vets.Count(v => !v.Activa),
            PorVencer: vets.Count(v => Dias(v) is >= 0 and <= DiasAviso),
            Vencidas: vets.Count(v => Dias(v) < 0),
            PlanMensual: vets.Count(v => v.Plan == PlanSuscripcion.Mensual),
            PlanAnual: vets.Count(v => v.Plan == PlanSuscripcion.Anual),
            AltasMes: vets.Count(v => v.FechaAlta.Year == ahora.Year && v.FechaAlta.Month == ahora.Month),
            AdministradoresActivos: admins.Count(a => a.Activo),
            VeterinariasSinAdmin: vets.Count(v => v.Activa && !vetsConAdmin.Contains(v.Id)),
            ProximasRenovaciones: proximas);
    }
}
