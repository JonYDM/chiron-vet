using Microsoft.Extensions.DependencyInjection;

namespace Chiron.Infrastructure;

/// <summary>
/// Punto único de registro de dependencias de la capa de Infraestructura.
/// Aquí se registran las implementaciones concretas (repositorios, acceso a datos,
/// servicios externos como WhatsApp) que satisfacen los contratos definidos en Application.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de la capa de Infraestructura.
    /// Por ahora no hay implementaciones; el repositorio en memoria llegará en H1.3.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Aquí se registrarán repositorios y servicios externos en próximas historias.
        return services;
    }
}
