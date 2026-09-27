using Microsoft.Extensions.DependencyInjection;

namespace Chiron.Application;

/// <summary>
/// Punto único de registro de dependencias de la capa de Aplicación.
/// Cada capa es responsable de registrar sus propios servicios (SOLID: SRP).
/// La capa de arranque (ConsoleApp/API) solo llama a este método,
/// sin conocer los detalles internos de Application.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de la capa de Aplicación (casos de uso, etc.).
    /// Por ahora no hay servicios; se irán agregando en las siguientes historias.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Aquí se registrarán los casos de uso (ej: RegistrarCliente) en próximas historias.
        return services;
    }
}
