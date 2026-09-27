using Chiron.Application;
using Chiron.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ─────────────────────────────────────────────────────────────────────────────
// Chiron.ConsoleApp — Punto de entrada y composición de la aplicación (H1.2).
//
// Aquí se configura el "Composition Root": el único lugar donde se arma el
// contenedor de inyección de dependencias. Cada capa registra lo suyo mediante
// sus métodos de extensión (AddApplication / AddInfrastructure), manteniendo
// la separación de responsabilidades (SOLID).
// ─────────────────────────────────────────────────────────────────────────────

// Se usa el Generic Host de .NET: trae DI, configuración y logging integrados.
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Cada capa se registra a sí misma. ConsoleApp no conoce los detalles internos.
        services.AddApplication();
        services.AddInfrastructure();
    })
    .Build();

// Se resuelve un servicio del contenedor para demostrar que la DI funciona.
ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("🐾 Chiron — Sistema de gestión para veterinarias");
logger.LogInformation("Inyección de dependencias configurada correctamente (H1.2).");
logger.LogInformation("Capas registradas: Application, Infrastructure.");

await host.StopAsync();
