using System.Security.Claims;
using System.Text;
using Chiron.Application;
using Chiron.Application.Citas;
using Chiron.Application.Clientes;
using Chiron.Application.Common;
using Chiron.Application.Expedientes;
using Chiron.Application.Mascotas;
using Chiron.Application.PuntoVenta;
using Chiron.Application.Recordatorios;
using Chiron.Application.Seguridad;
using Chiron.Domain.Common;
using Chiron.Domain.Usuarios;
using Chiron.Domain.Veterinarias;
using Chiron.Infrastructure;
using Chiron.Infrastructure.Seguridad;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// ─────────────────────────────────────────────────────────────────────────────
// Chiron.Api — API REST con autenticación JWT y autorización por roles (Épica 9).
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// Railway inyecta el puerto por la variable PORT.
string? puerto = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(puerto))
    builder.WebHost.UseUrls($"http://+:{puerto}");

// Opciones de JWT desde configuración/variables de entorno.
// La clave DEBE venir de una variable de entorno en producción (Jwt__Clave).
var jwtOpciones = new JwtOpciones
{
    Clave = builder.Configuration["Jwt:Clave"]
            ?? "clave-de-desarrollo-solo-local-cambiar-en-produccion-1234567890",
    Emisor = builder.Configuration["Jwt:Emisor"] ?? "Chiron",
    Audiencia = builder.Configuration["Jwt:Audiencia"] ?? "ChironApi"
};

builder.Services.AddApplication();

// Persistencia: PostgreSQL si hay cadena de conexión; si no, en memoria.
string? cadenaPostgres = builder.Configuration.GetConnectionString("Chiron");
if (!string.IsNullOrWhiteSpace(cadenaPostgres))
    builder.Services.AddInfrastructurePostgres(cadenaPostgres, jwtOpciones);
else
    builder.Services.AddInfrastructure(jwtOpciones);

// ── Autenticación JWT ──
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOpciones.Emisor,
            ValidAudience = jwtOpciones.Audiencia,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpciones.Clave))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Migraciones automáticas en modo PostgreSQL (con reintentos).
if (!string.IsNullOrWhiteSpace(cadenaPostgres))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<Chiron.Infrastructure.Persistencia.Ef.ChironDbContext>();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    const int maxIntentos = 10;
    for (int intento = 1; intento <= maxIntentos; intento++)
    {
        try { db.Database.Migrate(); logger.LogInformation("Migraciones aplicadas correctamente."); break; }
        catch (Exception ex) when (intento < maxIntentos)
        {
            logger.LogWarning("BD no lista (intento {I}/{M}): {E}. Reintentando en 3s...", intento, maxIntentos, ex.Message);
            Thread.Sleep(3000);
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", "Chiron.Api v1"));

// El orden importa: autenticación antes que autorización.
app.UseAuthentication();
app.UseAuthorization();

// En modo EN MEMORIA (sin PostgreSQL), sembrar datos de prueba para poder
// autenticarse y demostrar la seguridad. En producción (Postgres) NO se siembra.
if (string.IsNullOrWhiteSpace(cadenaPostgres))
{
    using var scope = app.Services.CreateScope();
    var hasheador = scope.ServiceProvider.GetRequiredService<IHasheadorContrasena>();
    var repoUsuarios = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
    var repoVet = scope.ServiceProvider.GetRequiredService<IRepository<Veterinaria>>();

    // Veterinaria de demostración.
    Veterinaria vetDemo = Veterinaria.Crear("Veterinaria Demo", "7770000000").Valor!;
    await repoVet.AgregarAsync(vetDemo);

    // SuperAdmin (dueño de Chiron). Identificador: nombre de usuario "superadmin", PIN 6 dígitos.
    Usuario superAdmin = Usuario.CrearStaff(
        Guid.Empty, "superadmin", "Super Admin",
        hasheador.Hashear("123456"), RolUsuario.SuperAdmin).Valor!;
    await repoUsuarios.AgregarAsync(superAdmin);

    // Administrador de la veterinaria demo. Usuario "admindemo", PIN 6 dígitos.
    Usuario admin = Usuario.CrearStaff(
        vetDemo.Id, "admindemo", "Admin Demo",
        hasheador.Hashear("654321"), RolUsuario.Administrador).Valor!;
    await repoUsuarios.AgregarAsync(admin);

    // Cliente + mascota + vacuna próxima, y un usuario DUEÑO ligado a ese cliente.
    var repoClientes = scope.ServiceProvider.GetRequiredService<Chiron.Application.Clientes.IClienteRepository>();
    var repoMascotas = scope.ServiceProvider.GetRequiredService<Chiron.Application.Mascotas.IMascotaRepository>();
    var repoRegistros = scope.ServiceProvider.GetRequiredService<Chiron.Application.Expedientes.IRegistroMedicoRepository>();

    var cliente = Chiron.Domain.Clientes.Cliente.Crear(
        vetDemo.Id, "María Dueña", "7771234567", Chiron.Domain.Clientes.OrigenCliente.Recomendacion).Valor!;
    await repoClientes.AgregarAsync(cliente);

    var mascota = Chiron.Domain.Mascotas.Mascota.Crear(
        vetDemo.Id, cliente.Id, "Firulais", Chiron.Domain.Mascotas.EspecieMascota.Perro).Valor!;
    await repoMascotas.AgregarAsync(mascota);

    var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
    var vacuna = Chiron.Domain.Expedientes.RegistroMedico.Crear(
        vetDemo.Id, mascota.Id, Chiron.Domain.Expedientes.TipoRegistroMedico.Vacuna,
        hoy.AddDays(-365), "Vacuna antirrábica", hoy.AddDays(5)).Valor!;
    await repoRegistros.AgregarAsync(vacuna);

    // Usuario dueño: identificador = su teléfono, PIN 6 dígitos, ligado al cliente.
    Usuario dueno = Usuario.CrearDueno(
        vetDemo.Id, cliente.Id, "7771234567", "María Dueña",
        hasheador.Hashear("111222")).Valor!;
    await repoUsuarios.AgregarAsync(dueno);
}

app.MapGet("/", () => Results.Redirect("/swagger"));

static IResult ToHttp<T>(Result<T> r) =>
    r.EsExito ? Results.Ok(r.Valor) : Results.BadRequest(new { error = r.Error });

// Nombres de roles como constantes para autorización.
const string SuperAdmin = nameof(RolUsuario.SuperAdmin);
const string Administrador = nameof(RolUsuario.Administrador);
const string Veterinario = nameof(RolUsuario.Veterinario);
const string Recepcionista = nameof(RolUsuario.Recepcionista);
const string DuenoMascota = nameof(RolUsuario.DuenoMascota);

// ═══════════════════ AUTENTICACIÓN (público) ═══════════════════
app.MapPost("/api/auth/login", async (LoginComando cmd, Login uc) =>
    ToHttp(await uc.EjecutarAsync(cmd)))
.WithName("Login").WithTags("Auth").AllowAnonymous();

// ═══════════════════ SUPERADMIN (solo dueño de Chiron) ═══════════════════
// Alta de veterinaria (protegido: solo SuperAdmin). Antes estaba abierto.
app.MapPost("/api/admin/veterinarias", async (CrearVeterinariaDto dto, IRepository<Veterinaria> repo) =>
{
    Result<Veterinaria> r = Veterinaria.Crear(dto.Nombre, dto.Telefono);
    if (!r.EsExito) return Results.BadRequest(new { error = r.Error });
    await repo.AgregarAsync(r.Valor!);
    return Results.Ok(new { r.Valor!.Id, r.Valor.Nombre, r.Valor.Activa });
})
.WithName("CrearVeterinaria").WithTags("SuperAdmin").RequireAuthorization(p => p.RequireRole(SuperAdmin));

// Activar veterinaria (pago recibido).
app.MapPost("/api/admin/veterinarias/{id:guid}/activar", async (Guid id, IRepository<Veterinaria> repo) =>
{
    Veterinaria? vet = await repo.ObtenerPorIdAsync(id);
    if (vet is null) return Results.NotFound();
    vet.Activar();
    await repo.ActualizarAsync(vet);
    return Results.Ok(new { vet.Id, vet.Activa });
})
.WithName("ActivarVeterinaria").WithTags("SuperAdmin").RequireAuthorization(p => p.RequireRole(SuperAdmin));

// Desactivar veterinaria (impago) — bloquea el acceso de sus usuarios.
app.MapPost("/api/admin/veterinarias/{id:guid}/desactivar", async (Guid id, IRepository<Veterinaria> repo) =>
{
    Veterinaria? vet = await repo.ObtenerPorIdAsync(id);
    if (vet is null) return Results.NotFound();
    vet.Desactivar();
    await repo.ActualizarAsync(vet);
    return Results.Ok(new { vet.Id, vet.Activa });
})
.WithName("DesactivarVeterinaria").WithTags("SuperAdmin").RequireAuthorization(p => p.RequireRole(SuperAdmin));

// Listar todas las veterinarias.
app.MapGet("/api/admin/veterinarias", async (IRepository<Veterinaria> repo) =>
    Results.Ok(await repo.ObtenerTodosAsync()))
.WithName("ListarVeterinarias").WithTags("SuperAdmin").RequireAuthorization(p => p.RequireRole(SuperAdmin));

// ═══════════════════ CLIENTES Y MASCOTAS (staff de la veterinaria) ═══════════════════
app.MapPost("/api/registro-rapido", async (RegistrarClienteConMascotaComando cmd, RegistrarClienteConMascota uc) =>
    ToHttp(await uc.EjecutarAsync(cmd)))
.WithName("RegistroRapido").WithTags("Clientes")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

app.MapGet("/api/veterinarias/{veterinariaId:guid}/clientes", async (Guid veterinariaId, string? texto, BuscarClientes uc) =>
    Results.Ok(await uc.EjecutarAsync(veterinariaId, texto)))
.WithName("BuscarClientes").WithTags("Clientes")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

app.MapGet("/api/clientes/{clienteId:guid}/mascotas", async (Guid clienteId, ListarMascotasDeCliente uc) =>
    Results.Ok(await uc.EjecutarAsync(clienteId)))
.WithName("MascotasDeCliente").WithTags("Mascotas")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

// ═══════════════════ EXPEDIENTE (veterinario y admin) ═══════════════════
app.MapPost("/api/expediente", async (AgregarRegistroMedicoComando cmd, AgregarRegistroMedico uc) =>
    ToHttp(await uc.EjecutarAsync(cmd)))
.WithName("AgregarRegistroMedico").WithTags("Expediente")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario));

app.MapGet("/api/mascotas/{mascotaId:guid}/expediente", async (Guid mascotaId, VerExpedienteMascota uc) =>
    Results.Ok(await uc.EjecutarAsync(mascotaId)))
.WithName("VerExpediente").WithTags("Expediente")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario));

// ═══════════════════ CITAS (staff) ═══════════════════
app.MapPost("/api/citas", async (AgendarCitaComando cmd, AgendarCita uc) =>
    ToHttp(await uc.EjecutarAsync(cmd)))
.WithName("AgendarCita").WithTags("Citas")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

app.MapGet("/api/veterinarias/{veterinariaId:guid}/citas/proximas", async (Guid veterinariaId, VerAgenda uc) =>
    Results.Ok(await uc.ProximasAsync(veterinariaId)))
.WithName("ProximasCitas").WithTags("Citas")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

// ═══════════════════ PUNTO DE VENTA ═══════════════════
// Catálogo: admin gestiona productos.
app.MapPost("/api/productos", async (AgregarProductoComando cmd, AgregarProducto uc) =>
    ToHttp(await uc.EjecutarAsync(cmd)))
.WithName("AgregarProducto").WithTags("PuntoVenta")
.RequireAuthorization(p => p.RequireRole(Administrador));

app.MapGet("/api/veterinarias/{veterinariaId:guid}/catalogo", async (Guid veterinariaId, ListarCatalogo uc) =>
    Results.Ok(await uc.EjecutarAsync(veterinariaId)))
.WithName("ListarCatalogo").WithTags("PuntoVenta")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

// Vender: recepción y admin.
app.MapPost("/api/ventas", async (RegistrarVentaComando cmd, RegistrarVenta uc) =>
    ToHttp(await uc.EjecutarAsync(cmd)))
.WithName("RegistrarVenta").WithTags("PuntoVenta")
.RequireAuthorization(p => p.RequireRole(Administrador, Recepcionista));

// ═══════════════════ RECORDATORIOS (admin) ═══════════════════
app.MapPost("/api/veterinarias/{veterinariaId:guid}/recordatorios/enviar", async (Guid veterinariaId, int? dias, EnviarRecordatorios uc) =>
    Results.Ok(await uc.EjecutarAsync(veterinariaId, dias ?? 7)))
.WithName("EnviarRecordatorios").WithTags("Recordatorios")
.RequireAuthorization(p => p.RequireRole(Administrador));

// ═══════════════════ PORTAL DEL DUEÑO DE MASCOTA (H10.1, H10.2) ═══════════════════
// El dueño ve SOLO sus datos. El clienteId se toma del token (no de la URL),
// así es imposible que un dueño consulte los datos de otro.

// Helper local: obtiene el clienteId y veterinariaId del usuario autenticado.
static (Guid clienteId, Guid veterinariaId)? DatosDueno(ClaimsPrincipal user)
{
    string? cli = user.FindFirst("clienteId")?.Value;
    string? vet = user.FindFirst("veterinariaId")?.Value;
    if (Guid.TryParse(cli, out Guid clienteId) && Guid.TryParse(vet, out Guid veterinariaId))
        return (clienteId, veterinariaId);
    return null;
}

// Mis mascotas.
app.MapGet("/api/portal/mis-mascotas", async (ClaimsPrincipal user, ListarMascotasDeCliente uc) =>
{
    var datos = DatosDueno(user);
    if (datos is null) return Results.BadRequest(new { error = "El token no corresponde a un dueño de mascota." });
    return Results.Ok(await uc.EjecutarAsync(datos.Value.clienteId));
})
.WithName("MisMascotas").WithTags("Portal").RequireAuthorization(p => p.RequireRole(DuenoMascota));

// Expediente de una de MIS mascotas (valida que la mascota sea mía).
app.MapGet("/api/portal/mascotas/{mascotaId:guid}/expediente",
    async (Guid mascotaId, ClaimsPrincipal user, ListarMascotasDeCliente misMascotas, VerExpedienteMascota expediente) =>
{
    var datos = DatosDueno(user);
    if (datos is null) return Results.BadRequest(new { error = "Token inválido." });

    // Verificar que la mascota pertenezca al dueño autenticado.
    var mias = await misMascotas.EjecutarAsync(datos.Value.clienteId);
    if (mias.All(m => m.Id != mascotaId))
        return Results.Forbid();

    return Results.Ok(await expediente.EjecutarAsync(mascotaId));
})
.WithName("MiExpediente").WithTags("Portal").RequireAuthorization(p => p.RequireRole(DuenoMascota));

// Mis recordatorios (vacunas/citas próximas de mis mascotas) — H10.2, in-app.
app.MapGet("/api/portal/mis-recordatorios", async (ClaimsPrincipal user, int? dias, GenerarRecordatorios uc) =>
{
    var datos = DatosDueno(user);
    if (datos is null) return Results.BadRequest(new { error = "Token inválido." });

    // El portal muestra los recordatorios del propio dueño (in-app). El consentimiento
    // aplica solo al ENVÍO de notificaciones, no a que el dueño los consulte él mismo.
    var todos = await uc.DetectarParaPortalAsync(datos.Value.veterinariaId, dias ?? 30);
    var mios = todos.Where(r => r.ClienteId == datos.Value.clienteId).ToList();
    return Results.Ok(mios);
})
.WithName("MisRecordatorios").WithTags("Portal").RequireAuthorization(p => p.RequireRole(DuenoMascota));

app.Run();

// DTO de entrada para crear veterinaria.
record CrearVeterinariaDto(string Nombre, string Telefono);
