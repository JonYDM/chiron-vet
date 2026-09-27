using Chiron.Application;
using Chiron.Application.Clientes;
using Chiron.Application.Common;
using Chiron.Application.Mascotas;
using Chiron.Domain.Clientes;
using Chiron.Domain.Common;
using Chiron.Domain.Mascotas;
using Chiron.Domain.Veterinarias;
using Chiron.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ─────────────────────────────────────────────────────────────────────────────
// Chiron.ConsoleApp — Demostración de los casos de uso de la capa de Aplicación.
// ─────────────────────────────────────────────────────────────────────────────

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddApplication();
        services.AddInfrastructure();
    })
    .Build();

ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🐾 Chiron — Demostración de casos de uso");

// Alta de una veterinaria (tenant) para el escenario.
IRepository<Veterinaria> veterinarias = host.Services.GetRequiredService<IRepository<Veterinaria>>();
Veterinaria vet = Veterinaria.Crear("Veterinaria San Francisco", "7771112233").Valor!;
await veterinarias.AgregarAsync(vet);
logger.LogInformation("🏥 Veterinaria: {Nombre}", vet.Nombre);

// ── Caso de uso 1: REGISTRO RÁPIDO (llega María con Firulais) ──
var registroRapido = host.Services.GetRequiredService<RegistrarClienteConMascota>();
var comando = new RegistrarClienteConMascotaComando(
    VeterinariaId: vet.Id,
    NombreCliente: "María González",
    TelefonoCliente: "777-123-4567",
    OrigenCliente: OrigenCliente.Recomendacion,
    NombreMascota: "Firulais",
    Especie: EspecieMascota.Perro,
    Sexo: SexoMascota.Macho,
    Raza: "Labrador",
    FechaNacimiento: new DateOnly(2021, 5, 10));

Result<RegistroRapidoResultado> resultado = await registroRapido.EjecutarAsync(comando);
if (resultado.EsExito)
    logger.LogInformation("✅ Registro rápido OK → ClienteId={C} | MascotaId={M}",
        resultado.Valor!.ClienteId, resultado.Valor.MascotaId);
else
    logger.LogWarning("⛔ Registro rápido falló: {Error}", resultado.Error);

// Registro rápido de un segundo cliente para probar la búsqueda.
await registroRapido.EjecutarAsync(new RegistrarClienteConMascotaComando(
    vet.Id, "María Fernanda Ruiz", "7779998877", OrigenCliente.Google,
    "Michi", EspecieMascota.Gato));

// ── Caso de uso 2: BUSCAR CLIENTES por nombre ──
var buscarClientes = host.Services.GetRequiredService<BuscarClientes>();
IReadOnlyList<Cliente> encontrados = await buscarClientes.EjecutarAsync(vet.Id, "maría");
logger.LogInformation("🔎 Clientes que contienen 'maría': {Total}", encontrados.Count);
foreach (Cliente c in encontrados)
    logger.LogInformation("   • {Nombre} ({Tel})", c.Nombre, c.Telefono);

// ── Caso de uso 3: LISTAR MASCOTAS de un cliente ──
var listarMascotas = host.Services.GetRequiredService<ListarMascotasDeCliente>();
Guid clienteId = resultado.Valor!.ClienteId;
IReadOnlyList<Mascota> mascotasCliente = await listarMascotas.EjecutarAsync(clienteId);
logger.LogInformation("🐕 Mascotas de María González: {Total}", mascotasCliente.Count);
foreach (Mascota m in mascotasCliente)
    logger.LogInformation("   • {Nombre} — {Especie} {Raza} (edad {Edad})",
        m.Nombre, m.Especie, m.Raza, m.EdadEnAnios());

// ── Validación: registro rápido en veterinaria inexistente ──
Result<RegistroRapidoResultado> invalido = await registroRapido.EjecutarAsync(
    new RegistrarClienteConMascotaComando(
        Guid.NewGuid(), "Cliente X", "7770000000", OrigenCliente.NoEspecificado,
        "Rex", EspecieMascota.Perro));
if (!invalido.EsExito)
    logger.LogWarning("⛔ Rechazado (esperado): {Error}", invalido.Error);

await host.StopAsync();
