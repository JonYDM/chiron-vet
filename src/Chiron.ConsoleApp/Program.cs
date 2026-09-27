using Chiron.Application;
using Chiron.Application.Clientes;
using Chiron.Application.Common;
using Chiron.Application.Expedientes;
using Chiron.Domain.Clientes;
using Chiron.Domain.Common;
using Chiron.Domain.Expedientes;
using Chiron.Domain.Mascotas;
using Chiron.Domain.Veterinarias;
using Chiron.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ─────────────────────────────────────────────────────────────────────────────
// Chiron.ConsoleApp — Demostración: registro rápido + expediente médico.
// ─────────────────────────────────────────────────────────────────────────────

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddApplication();
        services.AddInfrastructure();
    })
    .Build();

ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🐾 Chiron — Expediente médico");

// Preparar escenario: veterinaria + registro rápido de María con Firulais.
IRepository<Veterinaria> veterinarias = host.Services.GetRequiredService<IRepository<Veterinaria>>();
Veterinaria vet = Veterinaria.Crear("Veterinaria San Francisco", "7771112233").Valor!;
await veterinarias.AgregarAsync(vet);

var registroRapido = host.Services.GetRequiredService<RegistrarClienteConMascota>();
Result<RegistroRapidoResultado> alta = await registroRapido.EjecutarAsync(new RegistrarClienteConMascotaComando(
    vet.Id, "María González", "7771234567", OrigenCliente.Recomendacion,
    "Firulais", EspecieMascota.Perro, SexoMascota.Macho, "Labrador", new DateOnly(2021, 5, 10)));
Guid mascotaId = alta.Valor!.MascotaId;
logger.LogInformation("🐕 Mascota Firulais lista (Id {Id})", mascotaId);

var agregarRegistro = host.Services.GetRequiredService<AgregarRegistroMedico>();

// ── H3.1: registrar una consulta ──
Result<Guid> consulta = await agregarRegistro.EjecutarAsync(new AgregarRegistroMedicoComando(
    vet.Id, mascotaId, TipoRegistroMedico.Consulta, new DateOnly(2026, 9, 1),
    "Revisión general. Peso 28 kg, saludable."));
logger.LogInformation(consulta.EsExito ? "✅ Consulta registrada" : $"⛔ {consulta.Error}");

// ── H3.2: registrar una vacuna CON próxima aplicación (base de recordatorio) ──
Result<Guid> vacuna = await agregarRegistro.EjecutarAsync(new AgregarRegistroMedicoComando(
    vet.Id, mascotaId, TipoRegistroMedico.Vacuna, new DateOnly(2026, 9, 1),
    "Vacuna antirrábica anual.", FechaProximaAplicacion: new DateOnly(2027, 9, 1)));
logger.LogInformation(vacuna.EsExito ? "✅ Vacuna registrada (próxima: 2027-09-01)" : $"⛔ {vacuna.Error}");

// ── H3.2: desparasitación con próxima aplicación ──
await agregarRegistro.EjecutarAsync(new AgregarRegistroMedicoComando(
    vet.Id, mascotaId, TipoRegistroMedico.Desparasitacion, new DateOnly(2026, 9, 1),
    "Desparasitación interna.", FechaProximaAplicacion: new DateOnly(2026, 12, 1)));

// ── Validación: próxima aplicación anterior a la fecha (debe fallar) ──
Result<Guid> invalido = await agregarRegistro.EjecutarAsync(new AgregarRegistroMedicoComando(
    vet.Id, mascotaId, TipoRegistroMedico.Vacuna, new DateOnly(2026, 9, 1),
    "Fecha inválida.", FechaProximaAplicacion: new DateOnly(2026, 8, 1)));
if (!invalido.EsExito)
    logger.LogWarning("⛔ Rechazado (esperado): {Error}", invalido.Error);

// ── H3.3: ver el expediente completo (más reciente primero) ──
var verExpediente = host.Services.GetRequiredService<VerExpedienteMascota>();
IReadOnlyList<RegistroMedico> expediente = await verExpediente.EjecutarAsync(mascotaId);
logger.LogInformation("📋 Expediente de Firulais ({Total} registros):", expediente.Count);
foreach (RegistroMedico r in expediente)
{
    string proxima = r.FechaProximaAplicacion is { } p ? $" | próxima: {p}" : "";
    logger.LogInformation("   • [{Fecha}] {Tipo}: {Desc}{Proxima}", r.Fecha, r.Tipo, r.Descripcion, proxima);
}

await host.StopAsync();
