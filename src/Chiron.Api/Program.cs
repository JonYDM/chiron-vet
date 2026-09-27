using Chiron.Application;
using Chiron.Application.Citas;
using Chiron.Application.Clientes;
using Chiron.Application.Common;
using Chiron.Application.Expedientes;
using Chiron.Application.Mascotas;
using Chiron.Application.PuntoVenta;
using Chiron.Application.Recordatorios;
using Chiron.Domain.Common;
using Chiron.Domain.Veterinarias;
using Chiron.Infrastructure;
using Microsoft.EntityFrameworkCore;

// ─────────────────────────────────────────────────────────────────────────────
// Chiron.Api — API REST que expone los casos de uso (Épica 8, H8.1).
// Reutiliza las capas Application e Infrastructure vía inyección de dependencias.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// Railway (y otros PaaS) inyectan el puerto por la variable de entorno PORT.
// Si existe, la API escucha en ese puerto; si no, usa el valor por defecto.
string? puerto = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(puerto))
    builder.WebHost.UseUrls($"http://+:{puerto}");

// Registro de las capas de Chiron (mismos métodos que usa la consola).
builder.Services.AddApplication();

// Persistencia: si hay cadena de conexión "Chiron", usa PostgreSQL (producción);
// si no, usa repositorios en memoria (útil para demo/local sin base de datos).
string? cadenaPostgres = builder.Configuration.GetConnectionString("Chiron");
if (!string.IsNullOrWhiteSpace(cadenaPostgres))
    builder.Services.AddInfrastructurePostgres(cadenaPostgres);
else
    builder.Services.AddInfrastructure();

// Swagger para explorar y probar la API desde el navegador.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Si estamos en modo PostgreSQL, aplicar migraciones pendientes al arrancar
// (crea las tablas automáticamente en el primer despliegue en Railway).
// Con reintentos, porque la base de datos puede tardar en estar lista.
if (!string.IsNullOrWhiteSpace(cadenaPostgres))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<Chiron.Infrastructure.Persistencia.Ef.ChironDbContext>();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    const int maxIntentos = 10;
    for (int intento = 1; intento <= maxIntentos; intento++)
    {
        try
        {
            db.Database.Migrate();
            logger.LogInformation("Migraciones aplicadas correctamente.");
            break;
        }
        catch (Exception ex) when (intento < maxIntentos)
        {
            logger.LogWarning("Base de datos no lista (intento {Intento}/{Max}): {Error}. Reintentando en 3s...",
                intento, maxIntentos, ex.Message);
            Thread.Sleep(3000);
        }
    }
}

// Swagger disponible siempre (útil para el demo).
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // Ruta RELATIVA al documento OpenAPI. Detrás de un proxy (Railway), una ruta
    // absoluta ("/swagger/v1/swagger.json") puede resolverse mal y hacer que la UI
    // no encuentre la definición ("Unable to render this definition"). La ruta
    // relativa "v1/swagger.json" (respecto a /swagger/) funciona en local y producción.
    c.SwaggerEndpoint("v1/swagger.json", "Chiron.Api v1");
});

// Redirige la raíz a Swagger para que al abrir el navegador se vea la API.
app.MapGet("/", () => Results.Redirect("/swagger"));

// Helper: convierte un Result<T> en respuesta HTTP.
static IResult ToHttp<T>(Result<T> r) =>
    r.EsExito ? Results.Ok(r.Valor) : Results.BadRequest(new { error = r.Error });

// ── Veterinarias (tenant) ──
app.MapPost("/api/veterinarias", async (CrearVeterinariaDto dto, IRepository<Veterinaria> repo) =>
{
    Result<Veterinaria> r = Veterinaria.Crear(dto.Nombre, dto.Telefono);
    if (!r.EsExito) return Results.BadRequest(new { error = r.Error });
    await repo.AgregarAsync(r.Valor!);
    return Results.Ok(new { r.Valor!.Id, r.Valor.Nombre });
})
.WithName("CrearVeterinaria").WithTags("Veterinarias");

// ── Registro rápido: cliente + mascota ──
app.MapPost("/api/registro-rapido", async (RegistrarClienteConMascotaComando cmd, RegistrarClienteConMascota uc) =>
    ToHttp(await uc.EjecutarAsync(cmd)))
.WithName("RegistroRapido").WithTags("Clientes");

// ── Buscar clientes ──
app.MapGet("/api/veterinarias/{veterinariaId:guid}/clientes", async (Guid veterinariaId, string? texto, BuscarClientes uc) =>
    Results.Ok(await uc.EjecutarAsync(veterinariaId, texto)))
.WithName("BuscarClientes").WithTags("Clientes");

// ── Mascotas de un cliente ──
app.MapGet("/api/clientes/{clienteId:guid}/mascotas", async (Guid clienteId, ListarMascotasDeCliente uc) =>
    Results.Ok(await uc.EjecutarAsync(clienteId)))
.WithName("MascotasDeCliente").WithTags("Mascotas");

// ── Expediente médico ──
app.MapPost("/api/expediente", async (AgregarRegistroMedicoComando cmd, AgregarRegistroMedico uc) =>
    ToHttp(await uc.EjecutarAsync(cmd)))
.WithName("AgregarRegistroMedico").WithTags("Expediente");

app.MapGet("/api/mascotas/{mascotaId:guid}/expediente", async (Guid mascotaId, VerExpedienteMascota uc) =>
    Results.Ok(await uc.EjecutarAsync(mascotaId)))
.WithName("VerExpediente").WithTags("Expediente");

// ── Citas ──
app.MapPost("/api/citas", async (AgendarCitaComando cmd, AgendarCita uc) =>
    ToHttp(await uc.EjecutarAsync(cmd)))
.WithName("AgendarCita").WithTags("Citas");

app.MapGet("/api/veterinarias/{veterinariaId:guid}/citas/proximas", async (Guid veterinariaId, VerAgenda uc) =>
    Results.Ok(await uc.ProximasAsync(veterinariaId)))
.WithName("ProximasCitas").WithTags("Citas");

// ── Punto de venta ──
app.MapPost("/api/productos", async (AgregarProductoComando cmd, AgregarProducto uc) =>
    ToHttp(await uc.EjecutarAsync(cmd)))
.WithName("AgregarProducto").WithTags("PuntoVenta");

app.MapGet("/api/veterinarias/{veterinariaId:guid}/catalogo", async (Guid veterinariaId, ListarCatalogo uc) =>
    Results.Ok(await uc.EjecutarAsync(veterinariaId)))
.WithName("ListarCatalogo").WithTags("PuntoVenta");

app.MapPost("/api/ventas", async (RegistrarVentaComando cmd, RegistrarVenta uc) =>
    ToHttp(await uc.EjecutarAsync(cmd)))
.WithName("RegistrarVenta").WithTags("PuntoVenta");

// ── Recordatorios ──
app.MapPost("/api/veterinarias/{veterinariaId:guid}/recordatorios/enviar", async (Guid veterinariaId, int? dias, EnviarRecordatorios uc) =>
    Results.Ok(await uc.EjecutarAsync(veterinariaId, dias ?? 7)))
.WithName("EnviarRecordatorios").WithTags("Recordatorios");

app.Run();

// DTO de entrada para crear veterinaria (registro simple).
record CrearVeterinariaDto(string Nombre, string Telefono);
