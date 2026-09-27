using Chiron.Application;
using Chiron.Application.Citas;
using Chiron.Application.Clientes;
using Chiron.Application.Common;
using Chiron.Domain.Citas;
using Chiron.Domain.Clientes;
using Chiron.Domain.Common;
using Chiron.Domain.Mascotas;
using Chiron.Domain.Veterinarias;
using Chiron.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ─────────────────────────────────────────────────────────────────────────────
// Chiron.ConsoleApp — Demostración: registro rápido + agenda de citas.
// ─────────────────────────────────────────────────────────────────────────────

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddApplication();
        services.AddInfrastructure();
    })
    .Build();

ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🐾 Chiron — Agenda de citas");

// Escenario: veterinaria + María con Firulais.
IRepository<Veterinaria> veterinarias = host.Services.GetRequiredService<IRepository<Veterinaria>>();
Veterinaria vet = Veterinaria.Crear("Veterinaria San Francisco", "7771112233").Valor!;
await veterinarias.AgregarAsync(vet);

var registroRapido = host.Services.GetRequiredService<RegistrarClienteConMascota>();
Result<RegistroRapidoResultado> alta = await registroRapido.EjecutarAsync(new RegistrarClienteConMascotaComando(
    vet.Id, "María González", "7771234567", OrigenCliente.Recomendacion,
    "Firulais", EspecieMascota.Perro));
Guid mascotaId = alta.Valor!.MascotaId;

var agendarCita = host.Services.GetRequiredService<AgendarCita>();

// ── H4.1: agendar citas ──
DateTime manana10 = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
Result<Guid> cita1 = await agendarCita.EjecutarAsync(
    new AgendarCitaComando(vet.Id, mascotaId, manana10, "Vacunación anual"));
logger.LogInformation(cita1.EsExito ? "✅ Cita agendada para mañana 10:00" : $"⛔ {cita1.Error}");

DateTime pasadoManana16 = DateTime.UtcNow.Date.AddDays(2).AddHours(16);
await agendarCita.EjecutarAsync(
    new AgendarCitaComando(vet.Id, mascotaId, pasadoManana16, "Revisión de rutina"));

// ── Validación: cita en el pasado (debe fallar) ──
Result<Guid> pasada = await agendarCita.EjecutarAsync(
    new AgendarCitaComando(vet.Id, mascotaId, DateTime.UtcNow.AddHours(-2), "Cita en el pasado"));
if (!pasada.EsExito)
    logger.LogWarning("⛔ Rechazado (esperado): {Error}", pasada.Error);

// ── H4.2: ver próximas citas ──
var verAgenda = host.Services.GetRequiredService<VerAgenda>();
IReadOnlyList<Cita> proximas = await verAgenda.ProximasAsync(vet.Id);
logger.LogInformation("📅 Próximas citas programadas: {Total}", proximas.Count);
foreach (Cita c in proximas)
    logger.LogInformation("   • {Fecha:yyyy-MM-dd HH:mm} — {Motivo} [{Estado}]", c.FechaHora, c.Motivo, c.Estado);

// ── H4.2: agenda de mañana ──
DateOnly manana = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1));
IReadOnlyList<Cita> agendaManana = await verAgenda.DelDiaAsync(vet.Id, manana);
logger.LogInformation("📅 Agenda de mañana ({Dia}): {Total} cita(s)", manana, agendaManana.Count);

await host.StopAsync();
