using Chiron.Application.Common;
using Chiron.Infrastructure.Persistencia;
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
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Repositorio genérico en memoria como implementación por defecto de IRepository<T>.
        // Singleton: al ser almacenamiento en memoria, los datos deben vivir durante
        // toda la ejecución (una instancia compartida). Al migrar a PostgreSQL (H7.1),
        // aquí se cambiará por la implementación con base de datos, sin tocar Application.
        services.AddSingleton(typeof(IRepository<>), typeof(RepositorioEnMemoria<>));

        return services;
    }
}
