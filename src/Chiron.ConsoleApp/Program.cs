using Chiron.Application;
using Chiron.Application.Common;
using Chiron.Application.PuntoVenta;
using Chiron.Domain.Common;
using Chiron.Domain.PuntoVenta;
using Chiron.Domain.Veterinarias;
using Chiron.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ─────────────────────────────────────────────────────────────────────────────
// Chiron.ConsoleApp — Demostración: punto de venta (catálogo + ventas).
// ─────────────────────────────────────────────────────────────────────────────

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddApplication();
        services.AddInfrastructure();
    })
    .Build();

ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🐾 Chiron — Punto de venta");

// Veterinaria de prueba.
IRepository<Veterinaria> veterinarias = host.Services.GetRequiredService<IRepository<Veterinaria>>();
Veterinaria vet = Veterinaria.Crear("Veterinaria San Francisco", "7771112233").Valor!;
await veterinarias.AgregarAsync(vet);

// ── H6.1: agregar productos al catálogo ──
var agregarProducto = host.Services.GetRequiredService<AgregarProducto>();
Result<Guid> croquetas = await agregarProducto.EjecutarAsync(
    new AgregarProductoComando(vet.Id, "Croquetas Adulto 10kg", CategoriaProducto.Alimento, 450m, 20));
Result<Guid> antipulgas = await agregarProducto.EjecutarAsync(
    new AgregarProductoComando(vet.Id, "Antipulgas pipeta", CategoriaProducto.Medicina, 180m, 5));
logger.LogInformation("🛒 Catálogo cargado (croquetas y antipulgas)");

// Validación: precio inválido (debe fallar).
Result<Guid> malo = await agregarProducto.EjecutarAsync(
    new AgregarProductoComando(vet.Id, "Producto gratis", CategoriaProducto.Otro, 0m, 10));
if (!malo.EsExito) logger.LogWarning("⛔ Rechazado (esperado): {Error}", malo.Error);

// Ver catálogo.
var listarCatalogo = host.Services.GetRequiredService<ListarCatalogo>();
foreach (Producto p in await listarCatalogo.EjecutarAsync(vet.Id))
    logger.LogInformation("   • {Nombre} | {Cat} | ${Precio} | stock {Stock}", p.Nombre, p.Categoria, p.Precio, p.Stock);

// ── H6.2: registrar una venta (2 croquetas + 1 antipulgas) ──
var registrarVenta = host.Services.GetRequiredService<RegistrarVenta>();
Result<VentaResultado> venta = await registrarVenta.EjecutarAsync(new RegistrarVentaComando(
    vet.Id, ClienteId: null, Items: new[]
    {
        new ItemVentaComando(croquetas.Valor, 2),
        new ItemVentaComando(antipulgas.Valor, 1)
    }));
if (venta.EsExito)
    logger.LogInformation("✅ Venta registrada. Total: ${Total}", venta.Valor!.Total);

// Ver stock actualizado tras la venta.
foreach (Producto p in await listarCatalogo.EjecutarAsync(vet.Id))
    logger.LogInformation("   • {Nombre} → stock ahora {Stock}", p.Nombre, p.Stock);

// ── Validación: venta con stock insuficiente (100 antipulgas, solo hay 4) ──
Result<VentaResultado> sinStock = await registrarVenta.EjecutarAsync(new RegistrarVentaComando(
    vet.Id, null, new[] { new ItemVentaComando(antipulgas.Valor, 100) }));
if (!sinStock.EsExito) logger.LogWarning("⛔ Rechazado (esperado): {Error}", sinStock.Error);

await host.StopAsync();
