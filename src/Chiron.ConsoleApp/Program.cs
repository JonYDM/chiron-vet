using Chiron.Application;
using Chiron.Application.Citas;
using Chiron.Application.Common;
using Chiron.Application.Expedientes;
using Chiron.Application.Recordatorios;
using Chiron.Domain.Citas;
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
// Chiron.ConsoleApp — Demostración: recordatorios (WhatsApp simulado).
// ─────────────────────────────────────────────────────────────────────────────

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddApplication();
        services.AddInfrastructure();
    })
    .Build();

ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🐾 Chiron — Recordatorios");

// Escenario base.
IRepository<Veterinaria> veterinarias = host.Services.GetRequiredService<IRepository<Veterinaria>>();
Veterinaria vet = Veterinaria.Crear("Veterinaria San Francisco", "7771112233").Valor!;
await veterinarias.AgregarAsync(vet);

IRepository<Cliente> clientes = host.Services.GetRequiredService<IRepository<Cliente>>();
IRepository<Mascota> mascotas = host.Services.GetRequiredService<IRepository<Mascota>>();

// Cliente A: CON consentimiento de WhatsApp.
Cliente ana = Cliente.Crear(vet.Id, "Ana Torres", "7771111111", OrigenCliente.Recomendacion, aceptaWhatsApp: true).Valor!;
await clientes.AgregarAsync(ana);
Mascota rocky = Mascota.Crear(vet.Id, ana.Id, "Rocky", EspecieMascota.Perro).Valor!;
await mascotas.AgregarAsync(rocky);

// Cliente B: SIN consentimiento (no debe recibir recordatorios).
Cliente luis = Cliente.Crear(vet.Id, "Luis Díaz", "7772222222", OrigenCliente.Google, aceptaWhatsApp: false).Valor!;
await clientes.AgregarAsync(luis);
Mascota michi = Mascota.Crear(vet.Id, luis.Id, "Michi", EspecieMascota.Gato).Valor!;
await mascotas.AgregarAsync(michi);

// Datos que disparan recordatorios (dentro de los próximos 7 días).
var agregarRegistro = host.Services.GetRequiredService<AgregarRegistroMedico>();
DateOnly hoy = DateOnly.FromDateTime(DateTime.UtcNow);

// Vacuna próxima de Rocky (cliente CON opt-in) → debe generar recordatorio.
await agregarRegistro.EjecutarAsync(new AgregarRegistroMedicoComando(
    vet.Id, rocky.Id, TipoRegistroMedico.Vacuna, hoy.AddDays(-365),
    "Vacuna antirrábica", FechaProximaAplicacion: hoy.AddDays(3)));

// Vacuna próxima de Michi (cliente SIN opt-in) → NO debe generar recordatorio.
await agregarRegistro.EjecutarAsync(new AgregarRegistroMedicoComando(
    vet.Id, michi.Id, TipoRegistroMedico.Desparasitacion, hoy.AddDays(-90),
    "Desparasitación", FechaProximaAplicacion: hoy.AddDays(2)));

// Cita próxima de Rocky (cliente CON opt-in) → debe generar recordatorio.
var agendarCita = host.Services.GetRequiredService<AgendarCita>();
await agendarCita.EjecutarAsync(new AgendarCitaComando(
    vet.Id, rocky.Id, DateTime.UtcNow.AddDays(2).Date.AddHours(11), "Revisión de rutina"));

// ── H5.1 + H5.2: detectar y enviar recordatorios ──
var enviarRecordatorios = host.Services.GetRequiredService<EnviarRecordatorios>();
EnvioRecordatoriosResultado resultado = await enviarRecordatorios.EjecutarAsync(vet.Id, diasAnticipacion: 7);

logger.LogInformation("🔔 Recordatorios detectados: {D} | enviados: {E}",
    resultado.Detectados, resultado.Enviados);
logger.LogInformation("(Michi no recibe porque su dueño no dio consentimiento de WhatsApp)");

await host.StopAsync();
