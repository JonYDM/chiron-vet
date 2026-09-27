using Chiron.Application;
using Chiron.Application.Common;
using Chiron.Domain.Clientes;
using Chiron.Domain.Common;
using Chiron.Domain.Mascotas;
using Chiron.Domain.Usuarios;
using Chiron.Domain.Veterinarias;
using Chiron.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ─────────────────────────────────────────────────────────────────────────────
// Chiron.ConsoleApp — Punto de entrada y composición de la aplicación.
// Demuestra el modelo multi-tenant: Veterinaria → Usuario / Cliente → Mascota.
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

// Repositorios resueltos por DI (uno por tipo de entidad).
IRepository<Veterinaria> veterinarias = host.Services.GetRequiredService<IRepository<Veterinaria>>();
IRepository<Usuario> usuarios = host.Services.GetRequiredService<IRepository<Usuario>>();
IRepository<Cliente> clientes = host.Services.GetRequiredService<IRepository<Cliente>>();
IRepository<Mascota> mascotas = host.Services.GetRequiredService<IRepository<Mascota>>();

// ── 1. Alta de la veterinaria (tenant) ──
Result<Veterinaria> rVet = Veterinaria.Crear("Veterinaria San Francisco", "7771112233");
if (!rVet.EsExito) { logger.LogError(rVet.Error); return; }
Veterinaria vet = rVet.Valor!;
await veterinarias.AgregarAsync(vet);
logger.LogInformation("🏥 Veterinaria dada de alta: {Nombre} (Id {Id})", vet.Nombre, vet.Id);

// ── 2. Usuario administrador de esa veterinaria ──
Result<Usuario> rUser = Usuario.Crear(vet.Id, "Dra. Ana López", "ana@sanfrancisco.mx", RolUsuario.Administrador);
if (rUser.EsExito)
{
    await usuarios.AgregarAsync(rUser.Valor!);
    logger.LogInformation("👤 Usuario creado: {Nombre} | Rol: {Rol}", rUser.Valor!.Nombre, rUser.Valor.Rol);
}

// ── 3. Cliente (dueño) de esa veterinaria ──
Result<Cliente> rCli = Cliente.Crear(vet.Id, "María González", "777-123-4567", OrigenCliente.Recomendacion);
if (!rCli.EsExito) { logger.LogError(rCli.Error); return; }
Cliente cliente = rCli.Valor!;
await clientes.AgregarAsync(cliente);
logger.LogInformation("🧑 Cliente registrado: {Nombre} | Tel: {Tel}", cliente.Nombre, cliente.Telefono);

// ── 4. Mascota (paciente) asociada al cliente ──
Result<Mascota> rMas = Mascota.Crear(
    vet.Id, cliente.Id, "Firulais", EspecieMascota.Perro, SexoMascota.Macho,
    raza: "Labrador", fechaNacimiento: new DateOnly(2021, 5, 10));
if (rMas.EsExito)
{
    await mascotas.AgregarAsync(rMas.Valor!);
    Mascota m = rMas.Valor!;
    logger.LogInformation("🐕 Mascota registrada: {Nombre} | {Especie} {Raza} | Edad: {Edad} años | Dueño: {ClienteId}",
        m.Nombre, m.Especie, m.Raza, m.EdadEnAnios(), m.ClienteId);
}

// ── 5. Casos inválidos (validaciones) ──
Result<Cliente> sinVet = Cliente.Crear(Guid.Empty, "Sin Tenant", "7771234567");
if (!sinVet.EsExito) logger.LogWarning("⛔ Rechazado (esperado): {Error}", sinVet.Error);

Result<Mascota> masSinDueno = Mascota.Crear(vet.Id, Guid.Empty, "Michi", EspecieMascota.Gato);
if (!masSinDueno.EsExito) logger.LogWarning("⛔ Rechazado (esperado): {Error}", masSinDueno.Error);

// ── Resumen ──
logger.LogInformation("Resumen → Veterinarias: {V} | Usuarios: {U} | Clientes: {C} | Mascotas: {M}",
    (await veterinarias.ObtenerTodosAsync()).Count,
    (await usuarios.ObtenerTodosAsync()).Count,
    (await clientes.ObtenerTodosAsync()).Count,
    (await mascotas.ObtenerTodosAsync()).Count);

await host.StopAsync();
