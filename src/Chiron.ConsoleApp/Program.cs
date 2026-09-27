using Chiron.Application;
using Chiron.Application.Common;
using Chiron.Domain.Clientes;
using Chiron.Domain.Common;
using Chiron.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ─────────────────────────────────────────────────────────────────────────────
// Chiron.ConsoleApp — Punto de entrada y composición de la aplicación.
// Composition Root: único lugar donde se arma el contenedor de DI.
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

// Repositorio de clientes resuelto por DI (contrato, no implementación concreta).
IRepository<Cliente> clientes = host.Services.GetRequiredService<IRepository<Cliente>>();

// ── Caso 1: cliente válido ──
Result<Cliente> resultado = Cliente.Crear("María González", "777-123-4567", OrigenCliente.Recomendacion);
if (resultado.EsExito)
{
    await clientes.AgregarAsync(resultado.Valor!);
    logger.LogInformation("✅ Cliente registrado: {Nombre} | Tel: {Telefono} | Origen: {Origen}",
        resultado.Valor!.Nombre, resultado.Valor.Telefono, resultado.Valor.Origen);
}

// ── Caso 2: cliente inválido (sin nombre) ──
Result<Cliente> invalido = Cliente.Crear("", "7771234567");
if (!invalido.EsExito)
    logger.LogWarning("⛔ Registro rechazado (esperado): {Error}", invalido.Error);

// ── Caso 3: cliente inválido (teléfono corto) ──
Result<Cliente> telCorto = Cliente.Crear("Juan Pérez", "123");
if (!telCorto.EsExito)
    logger.LogWarning("⛔ Registro rechazado (esperado): {Error}", telCorto.Error);

// ── Verificación de persistencia ──
IReadOnlyList<Cliente> todos = await clientes.ObtenerTodosAsync();
logger.LogInformation("Total de clientes almacenados: {Total}", todos.Count);

await host.StopAsync();
