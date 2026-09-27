using Chiron.Application;
using Chiron.Application.Common;
using Chiron.Domain.Common;
using Chiron.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ─────────────────────────────────────────────────────────────────────────────
// Chiron.ConsoleApp — Punto de entrada y composición de la aplicación.
//
// Composition Root: único lugar donde se arma el contenedor de DI.
// Cada capa registra lo suyo (AddApplication / AddInfrastructure).
// ─────────────────────────────────────────────────────────────────────────────

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddApplication();
        services.AddInfrastructure();
    })
    .Build();

ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("🐾 Chiron — Sistema de gestión para veterinarias");
logger.LogInformation("Fundación técnica lista (Épica 1: DI + repositorio base).");

// ── Prueba del repositorio genérico resuelto por DI (H1.3) ──
// Se resuelve IRepository<EntidadPrueba> desde el contenedor: la app NO crea
// la implementación concreta, solo pide el contrato. Eso es Inversión de Dependencias.
IRepository<EntidadPrueba> repositorio =
    host.Services.GetRequiredService<IRepository<EntidadPrueba>>();

var prueba = new EntidadPrueba("Firulais");
await repositorio.AgregarAsync(prueba);

EntidadPrueba? recuperada = await repositorio.ObtenerPorIdAsync(prueba.Id);
IReadOnlyList<EntidadPrueba> todos = await repositorio.ObtenerTodosAsync();

logger.LogInformation("Repositorio en memoria verificado:");
logger.LogInformation("  - Entidad recuperada por Id: {Nombre}", recuperada?.Nombre ?? "(no encontrada)");
logger.LogInformation("  - Total de entidades almacenadas: {Total}", todos.Count);

await host.StopAsync();
