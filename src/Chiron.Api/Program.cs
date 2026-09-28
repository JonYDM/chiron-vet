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
using Chiron.Domain.Citas;
using Chiron.Domain.Clientes;
using Chiron.Domain.Mascotas;
using Chiron.Domain.PuntoVenta;
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

// Almacenamiento de fotos (Cloudflare R2). Credenciales SIEMPRE desde variables de
// entorno (R2_*), nunca en el código. Si faltan, se usa un almacenamiento nulo.
var r2Opciones = new Chiron.Infrastructure.Almacenamiento.R2Opciones
{
    AccessKeyId = builder.Configuration["R2_ACCESS_KEY_ID"] ?? "",
    SecretAccessKey = builder.Configuration["R2_SECRET_ACCESS_KEY"] ?? "",
    Endpoint = builder.Configuration["R2_ENDPOINT"] ?? "",
    Bucket = builder.Configuration["R2_BUCKET"] ?? "",
    PublicUrl = builder.Configuration["R2_PUBLIC_URL"] ?? ""
};
builder.Services.AddAlmacenamiento(r2Opciones);

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

// CORS: permite el/los origen(es) del frontend (Netlify/Vercel/dominio propio).
// Los orígenes se configuran por variable de entorno Cors__Origenes (separados por coma),
// p. ej. "https://chiron.netlify.app,https://app.chiron.mx". Sin variable, no permite
// orígenes externos (seguro por defecto). Nunca usa AllowAnyOrigin.
const string PoliticaCors = "FrontendPermitido";
string[] origenesCors = (builder.Configuration["Cors:Origenes"] ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy(PoliticaCors, politica =>
    {
        if (origenesCors.Length > 0)
            politica.WithOrigins(origenesCors).AllowAnyHeader().AllowAnyMethod();
    });
});

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

    // Seed del SuperAdmin inicial (bootstrap). Solo si se configuran las variables
    // SuperAdmin__Usuario y SuperAdmin__Pin, y solo si NO existe ya ese usuario.
    // Credenciales SIEMPRE desde variables de entorno, nunca en el código.
    string? saUsuario = builder.Configuration["SuperAdmin:Usuario"];
    string? saPin = builder.Configuration["SuperAdmin:Pin"];
    if (!string.IsNullOrWhiteSpace(saUsuario) && !string.IsNullOrWhiteSpace(saPin))
    {
        var repoUsuarios = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
        string idNorm = Usuario.NormalizarIdentificador(saUsuario);
        Usuario? existente = await repoUsuarios.ObtenerPorNombreUsuarioAsync(idNorm);
        if (existente is null)
        {
            var hasheador = scope.ServiceProvider.GetRequiredService<IHasheadorContrasena>();
            Result<Usuario> sa = Usuario.CrearStaff(
                Guid.Empty, saUsuario, "Super Admin", hasheador.Hashear(saPin), RolUsuario.SuperAdmin);
            if (sa.EsExito)
            {
                await repoUsuarios.AgregarAsync(sa.Valor!);
                logger.LogInformation("SuperAdmin inicial creado.");
            }
            else
            {
                logger.LogWarning("No se pudo crear el SuperAdmin: {Error}", sa.Error);
            }
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", "Chiron.Api v1"));

// El orden importa: CORS antes de autenticación/autorización.
app.UseCors(PoliticaCors);

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

// Paso 1 del login (estilo Nubank): valida el identificador y devuelve el primer
// nombre para saludar antes de pedir el PIN. Público. Devuelve { existe, nombre }.
app.MapPost("/api/auth/identificar", async (IdentificarComando cmd, Identificar uc) =>
    ToHttp(await uc.EjecutarAsync(cmd)))
.WithName("Identificar").WithTags("Auth").AllowAnonymous();

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

// Configura si el Administrador de una veterinaria puede operar (true) o es supervisor puro (false).
app.MapPost("/api/admin/veterinarias/{id:guid}/admin-operativo", async (Guid id, AdminOperativoDto dto, IRepository<Veterinaria> repo) =>
{
    Veterinaria? vet = await repo.ObtenerPorIdAsync(id);
    if (vet is null) return Results.NotFound();
    vet.EstablecerAdminOperativo(dto.Operativo);
    await repo.ActualizarAsync(vet);
    return Results.Ok(new { vet.Id, vet.AdminOperativo });
})
.WithName("AdminOperativo").WithTags("SuperAdmin").RequireAuthorization(p => p.RequireRole(SuperAdmin));

// SuperAdmin crea el usuario ADMINISTRADOR de una veterinaria.
app.MapPost("/api/admin/usuarios-admin", async (CrearUsuarioStaffComando cmd, CrearUsuarioStaff uc) =>
{
    // Fuerza el rol a Administrador sin importar lo que venga en el body.
    var comando = cmd with { Rol = RolUsuario.Administrador };
    return ToHttp(await uc.EjecutarAsync(comando));
})
.WithName("CrearAdminVeterinaria").WithTags("SuperAdmin").RequireAuthorization(p => p.RequireRole(SuperAdmin));

// ═══════════════════ GESTIÓN DE USUARIOS (Administrador de la veterinaria) ═══════════════════
// El Administrador crea staff (veterinario/recepcionista) de SU veterinaria.
// El VeterinariaId se toma del token del admin, no del body (aislamiento multi-tenant).
app.MapPost("/api/usuarios/staff", async (CrearStaffDto dto, ClaimsPrincipal user, CrearUsuarioStaff uc) =>
{
    string? vetClaim = user.FindFirst("veterinariaId")?.Value;
    if (!Guid.TryParse(vetClaim, out Guid veterinariaId))
        return Results.BadRequest(new { error = "Token sin veterinaria válida." });

    // Un admin solo puede crear Veterinario o Recepcionista (no otros admins ni superadmin).
    if (dto.Rol != RolUsuario.Veterinario && dto.Rol != RolUsuario.Recepcionista)
        return Results.BadRequest(new { error = "Rol no permitido. Use Veterinario o Recepcionista." });

    var comando = new CrearUsuarioStaffComando(veterinariaId, dto.NombreUsuario, dto.Nombre, dto.Pin, dto.Rol);
    return ToHttp(await uc.EjecutarAsync(comando));
})
.WithName("CrearStaff").WithTags("Usuarios").RequireAuthorization(p => p.RequireRole(Administrador));

// El Administrador/Recepcionista crea el acceso de un dueño de mascota (por su cliente).
app.MapPost("/api/usuarios/dueno", async (CrearDuenoDto dto, ClaimsPrincipal user, CrearUsuarioDueno uc) =>
{
    string? vetClaim = user.FindFirst("veterinariaId")?.Value;
    if (!Guid.TryParse(vetClaim, out Guid veterinariaId))
        return Results.BadRequest(new { error = "Token sin veterinaria válida." });

    var comando = new CrearUsuarioDuenoComando(veterinariaId, dto.ClienteId, dto.Pin);
    return ToHttp(await uc.EjecutarAsync(comando));
})
.WithName("CrearDueno").WithTags("Usuarios").RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

// Resetear el PIN de un usuario (recuperación de acceso).
// - Administrador: resetea a su staff (Veterinario/Recepcionista) y dueños de SU veterinaria.
// - SuperAdmin: resetea a Administradores.
// El rol y la veterinaria del solicitante se toman del token (no del body).
app.MapPost("/api/usuarios/{id:guid}/resetear-pin", async (Guid id, ResetearPinDto dto, ClaimsPrincipal user, ResetearPin uc) =>
{
    string? rolClaim = user.FindFirst(ClaimTypes.Role)?.Value ?? user.FindFirst("role")?.Value;
    if (!Enum.TryParse<RolUsuario>(rolClaim, out RolUsuario solicitanteRol))
        return Results.BadRequest(new { error = "Token sin rol válido." });

    // La veterinaria del solicitante (vacía/ausente para SuperAdmin).
    Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid solicitanteVet);

    var comando = new ResetearPinComando(id, dto.NuevoPin, solicitanteRol, solicitanteVet);
    return ToHttp(await uc.EjecutarAsync(comando));
})
.WithName("ResetearPin").WithTags("Usuarios").RequireAuthorization(p => p.RequireRole(Administrador, SuperAdmin));

// El Administrador lista los usuarios (staff + dueños) de SU veterinaria.
// El VeterinariaId se toma del token (aislamiento multi-tenant).
app.MapGet("/api/usuarios/staff", async (ClaimsPrincipal user, FiltroEstado? estado, ListarUsuariosDeVeterinaria uc) =>
{
    if (!Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid veterinariaId))
        return Results.BadRequest(new { error = "Token sin veterinaria válida." });
    return Results.Ok(await uc.EjecutarAsync(veterinariaId, estado ?? FiltroEstado.Activos));
})
.WithName("ListarUsuariosStaff").WithTags("Usuarios").RequireAuthorization(p => p.RequireRole(Administrador));

// El SuperAdmin lista todos los Administradores de veterinarias (con filtro de estado).
app.MapGet("/api/admin/administradores", async (FiltroEstado? estado, ListarAdministradores uc) =>
    Results.Ok(await uc.EjecutarAsync(estado ?? FiltroEstado.Activos)))
.WithName("ListarAdministradores").WithTags("SuperAdmin").RequireAuthorization(p => p.RequireRole(SuperAdmin));

// Obtener el usuario (acceso al portal) de un cliente: indica si ya tiene acceso
// y su usuarioId (para resetear su PIN). Devuelve 204 si el cliente no tiene acceso.
app.MapGet("/api/clientes/{clienteId:guid}/usuario", async (Guid clienteId, ObtenerUsuarioDeCliente uc) =>
{
    UsuarioDto? dto = await uc.EjecutarAsync(clienteId);
    return dto is null ? Results.NoContent() : Results.Ok(dto);
})
.WithName("UsuarioDeCliente").WithTags("Usuarios")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

// Cualquier usuario autenticado cambia su propio PIN (autoservicio). El id sale del token (sub).
app.MapPost("/api/mi-pin", async (CambiarMiPinDto dto, ClaimsPrincipal user, CambiarMiPin uc) =>
{
    string? sub = user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                  ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(sub, out Guid usuarioId))
        return Results.BadRequest(new { error = "Token inválido." });
    var comando = new CambiarMiPinComando(usuarioId, dto.PinActual, dto.NuevoPin);
    return ToHttp(await uc.EjecutarAsync(comando));
})
.WithName("CambiarMiPin").WithTags("Usuarios").RequireAuthorization();

// Editar nombre y/o activar-desactivar un usuario (Admin sobre su staff/dueños; SuperAdmin sobre admins).
app.MapPost("/api/usuarios/{id:guid}/gestionar", async (Guid id, GestionarUsuarioDto dto, ClaimsPrincipal user, GestionarUsuario uc) =>
{
    string? rolClaim = user.FindFirst(ClaimTypes.Role)?.Value ?? user.FindFirst("role")?.Value;
    if (!Enum.TryParse<RolUsuario>(rolClaim, out RolUsuario solicitanteRol))
        return Results.BadRequest(new { error = "Token sin rol válido." });
    Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid solicitanteVet);
    var comando = new GestionarUsuarioComando(id, dto.NuevoNombre, dto.Accion, solicitanteRol, solicitanteVet);
    return ToHttp(await uc.EjecutarAsync(comando));
})
.WithName("GestionarUsuario").WithTags("Usuarios").RequireAuthorization(p => p.RequireRole(Administrador, SuperAdmin));

// ═══════════════════ CLIENTES Y MASCOTAS (staff de la veterinaria) ═══════════════════
app.MapPost("/api/registro-rapido", async (RegistrarClienteConMascotaComando cmd, RegistrarClienteConMascota uc) =>
    ToHttp(await uc.EjecutarAsync(cmd)))
.WithName("RegistroRapido").WithTags("Clientes")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

app.MapGet("/api/veterinarias/{veterinariaId:guid}/clientes", async (
    Guid veterinariaId, string? texto, FiltroEstado? estado, int? pagina, int? tamano, BuscarClientes uc) =>
    Results.Ok(await uc.EjecutarAsync(
        veterinariaId, texto, estado ?? FiltroEstado.Activos, pagina ?? 1, tamano ?? 20)))
.WithName("BuscarClientes").WithTags("Clientes")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

app.MapGet("/api/clientes/{clienteId:guid}/mascotas", async (Guid clienteId, FiltroEstado? estado, ListarMascotasDeCliente uc) =>
    Results.Ok(await uc.EjecutarAsync(clienteId, estado ?? FiltroEstado.Activos)))
.WithName("MascotasDeCliente").WithTags("Mascotas")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

// Listar TODAS las mascotas de la veterinaria (pacientes) con su dueño y búsqueda.
// El veterinariaId sale del token (aislamiento multi-tenant).
app.MapGet("/api/mascotas", async (string? texto, ClaimsPrincipal user, ListarMascotasDeVeterinaria uc) =>
{
    if (!Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid veterinariaId))
        return Results.BadRequest(new { error = "Token sin veterinaria válida." });
    return Results.Ok(await uc.EjecutarAsync(veterinariaId, texto));
})
.WithName("ListarPacientes").WithTags("Mascotas")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

// Obtener una mascota completa por id (peso, esterilizado, padecimientos, etc.).
app.MapGet("/api/mascotas/{id:guid}", async (Guid id, ClaimsPrincipal user, ObtenerMascota uc) =>
{
    if (!Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid veterinariaId))
        return Results.BadRequest(new { error = "Token sin veterinaria válida." });
    return ToHttp(await uc.EjecutarAsync(id, veterinariaId));
})
.WithName("ObtenerMascota").WithTags("Mascotas")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista, DuenoMascota));

// Activar/desactivar (baja lógica) un cliente.
app.MapPost("/api/clientes/{id:guid}/estado", async (Guid id, EstadoActivoDto dto, ClaimsPrincipal user, CambiarEstadoCliente uc) =>
{
    if (!Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid veterinariaId))
        return Results.BadRequest(new { error = "Token sin veterinaria válida." });
    return ToHttp(await uc.EjecutarAsync(id, veterinariaId, dto.Activar));
})
.WithName("CambiarEstadoCliente").WithTags("Clientes")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

// Activar/desactivar (baja lógica) una mascota.
app.MapPost("/api/mascotas/{id:guid}/estado", async (Guid id, EstadoActivoDto dto, ClaimsPrincipal user, CambiarEstadoMascota uc) =>
{
    if (!Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid veterinariaId))
        return Results.BadRequest(new { error = "Token sin veterinaria válida." });
    return ToHttp(await uc.EjecutarAsync(id, veterinariaId, dto.Activar));
})
.WithName("CambiarEstadoMascota").WithTags("Mascotas")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

// ═══════════════════ FOTOS DE MASCOTA (galería) ═══════════════════
// Subir la foto de PERFIL (avatar) de la mascota (una sola, reemplaza la anterior).
app.MapPost("/api/mascotas/{id:guid}/foto-perfil", async (
    Guid id, IFormFile archivo, ClaimsPrincipal user, SubirFotoPerfil uc) =>
{
    if (!Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid veterinariaId))
        return Results.BadRequest(new { error = "Token sin veterinaria válida." });
    if (archivo is null || archivo.Length == 0)
        return Results.BadRequest(new { error = "No se recibió ninguna imagen." });
    using var ms = new MemoryStream();
    await archivo.CopyToAsync(ms);
    return ToHttp(await uc.EjecutarAsync(id, veterinariaId, ms.ToArray()));
})
.WithName("SubirFotoPerfil").WithTags("Mascotas")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista))
.DisableAntiforgery();

// Subir una foto a la galería de una mascota (multipart/form-data, campo "archivo").
// Opcionalmente se liga a un registro médico vía query ?registroMedicoId=...
app.MapPost("/api/mascotas/{id:guid}/fotos", async (
    Guid id, IFormFile archivo, Guid? registroMedicoId, ClaimsPrincipal user, GestionFotoMascota uc) =>
{
    if (!Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid veterinariaId))
        return Results.BadRequest(new { error = "Token sin veterinaria válida." });

    string? subId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("sub")?.Value;
    if (!Guid.TryParse(subId, out Guid usuarioId))
        return Results.BadRequest(new { error = "Token sin usuario válido." });

    if (archivo is null || archivo.Length == 0)
        return Results.BadRequest(new { error = "No se recibió ninguna imagen." });

    using var ms = new MemoryStream();
    await archivo.CopyToAsync(ms);

    var comando = new SubirFotoComando(veterinariaId, id, ms.ToArray(), usuarioId, registroMedicoId);
    return ToHttp(await uc.SubirAsync(comando));
})
.WithName("SubirFotoMascota").WithTags("Mascotas")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista))
.DisableAntiforgery();

// Listar la galería de una mascota (staff y también el dueño la ve en su portal).
app.MapGet("/api/mascotas/{id:guid}/fotos", async (Guid id, ClaimsPrincipal user, GestionFotoMascota uc) =>
{
    if (!Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid veterinariaId))
        return Results.BadRequest(new { error = "Token sin veterinaria válida." });
    return ToHttp(await uc.ListarAsync(veterinariaId, id));
})
.WithName("ListarFotosMascota").WithTags("Mascotas")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista, DuenoMascota));

// Eliminar una foto (cualquier staff de la veterinaria).
app.MapDelete("/api/mascotas/{id:guid}/fotos/{fotoId:guid}", async (
    Guid id, Guid fotoId, ClaimsPrincipal user, GestionFotoMascota uc) =>
{
    if (!Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid veterinariaId))
        return Results.BadRequest(new { error = "Token sin veterinaria válida." });
    return ToHttp(await uc.EliminarAsync(veterinariaId, id, fotoId));
})
.WithName("EliminarFotoMascota").WithTags("Mascotas")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

// Crear SOLO un cliente (sin mascota). El veterinariaId sale del token.
app.MapPost("/api/clientes", async (CrearClienteDto dto, ClaimsPrincipal user, CrearCliente uc) =>
{
    if (!Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid veterinariaId))
        return Results.BadRequest(new { error = "Token sin veterinaria válida." });
    var comando = new CrearClienteComando(veterinariaId, dto.Nombre, dto.Telefono, dto.Origen);
    return ToHttp(await uc.EjecutarAsync(comando));
})
.WithName("CrearCliente").WithTags("Clientes")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

// Editar un cliente.
app.MapPut("/api/clientes/{id:guid}", async (Guid id, EditarClienteDto dto, ClaimsPrincipal user, EditarCliente uc) =>
{
    if (!Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid veterinariaId))
        return Results.BadRequest(new { error = "Token sin veterinaria válida." });
    var comando = new EditarClienteComando(id, veterinariaId, dto.Nombre, dto.Telefono, dto.Origen);
    return ToHttp(await uc.EjecutarAsync(comando));
})
.WithName("EditarCliente").WithTags("Clientes")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

// Agregar una mascota a un cliente existente.
app.MapPost("/api/mascotas", async (AgregarMascotaDto dto, ClaimsPrincipal user, AgregarMascota uc) =>
{
    if (!Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid veterinariaId))
        return Results.BadRequest(new { error = "Token sin veterinaria válida." });
    var comando = new AgregarMascotaComando(veterinariaId, dto.ClienteId, dto.Nombre, dto.Especie,
        dto.Sexo, dto.Raza, dto.FechaNacimiento, dto.PesoKg, dto.Padecimientos, dto.Esterilizado);
    return ToHttp(await uc.EjecutarAsync(comando));
})
.WithName("AgregarMascota").WithTags("Mascotas")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

// Editar una mascota.
app.MapPut("/api/mascotas/{id:guid}", async (Guid id, EditarMascotaDto dto, ClaimsPrincipal user, EditarMascota uc) =>
{
    if (!Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid veterinariaId))
        return Results.BadRequest(new { error = "Token sin veterinaria válida." });
    var comando = new EditarMascotaComando(id, veterinariaId, dto.Nombre, dto.Especie,
        dto.Sexo, dto.Raza, dto.FechaNacimiento, dto.PesoKg, dto.Padecimientos, dto.Esterilizado);
    return ToHttp(await uc.EjecutarAsync(comando));
})
.WithName("EditarMascota").WithTags("Mascotas")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

// ═══════════════════ EXPEDIENTE (veterinario y admin) ═══════════════════
app.MapPost("/api/expediente", async (AgregarRegistroMedicoComando cmd, AgregarRegistroMedico uc) =>
    ToHttp(await uc.EjecutarAsync(cmd)))
.WithName("AgregarRegistroMedico").WithTags("Expediente")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario));

app.MapGet("/api/mascotas/{mascotaId:guid}/expediente", async (Guid mascotaId, VerExpedienteMascota uc) =>
    Results.Ok(await uc.EjecutarAsync(mascotaId)))
.WithName("VerExpediente").WithTags("Expediente")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

// ═══════════════════ CITAS (staff) ═══════════════════
app.MapPost("/api/citas", async (AgendarCitaComando cmd, AgendarCita uc) =>
    ToHttp(await uc.EjecutarAsync(cmd)))
.WithName("AgendarCita").WithTags("Citas")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

app.MapGet("/api/veterinarias/{veterinariaId:guid}/citas/proximas", async (Guid veterinariaId, VerAgenda uc) =>
    Results.Ok(await uc.ProximasAsync(veterinariaId)))
.WithName("ProximasCitas").WithTags("Citas")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

// Lista TODAS las citas de la veterinaria (opcionalmente por estado), con nombre de
// mascota y dueño resueltos. Es la fuente para el historial y los filtros por estado.
// El veterinariaId sale del token (aislamiento multi-tenant).
app.MapGet("/api/citas", async (EstadoCita? estado, ClaimsPrincipal user, VerAgenda uc) =>
{
    if (!Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid veterinariaId))
        return Results.BadRequest(new { error = "El token no tiene veterinariaId." });
    return Results.Ok(await uc.ListarAsync(veterinariaId, estado));
})
.WithName("ListarCitas").WithTags("Citas")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

// Cambiar el estado de una cita (atender / cancelar / no asistió). El veterinariaId sale del token.
app.MapPost("/api/citas/{id:guid}/estado", async (Guid id, CambiarEstadoCitaDto dto, ClaimsPrincipal user, CambiarEstadoCita uc) =>
{
    if (!Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid veterinariaId))
        return Results.BadRequest(new { error = "Token sin veterinaria válida." });
    var comando = new CambiarEstadoCitaComando(id, dto.Accion, veterinariaId);
    return ToHttp(await uc.EjecutarAsync(comando));
})
.WithName("CambiarEstadoCita").WithTags("Citas")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

// ═══════════════════ PUNTO DE VENTA ═══════════════════
// Catálogo: admin gestiona productos.
app.MapPost("/api/productos", async (AgregarProductoComando cmd, AgregarProducto uc) =>
    ToHttp(await uc.EjecutarAsync(cmd)))
.WithName("AgregarProducto").WithTags("PuntoVenta")
.RequireAuthorization(p => p.RequireRole(Administrador));

app.MapGet("/api/veterinarias/{veterinariaId:guid}/catalogo", async (Guid veterinariaId, FiltroEstado? estado, ListarCatalogo uc) =>
    Results.Ok(await uc.EjecutarAsync(veterinariaId, estado ?? FiltroEstado.Activos)))
.WithName("ListarCatalogo").WithTags("PuntoVenta")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

// Vender: recepción y admin.
app.MapPost("/api/ventas", async (RegistrarVentaComando cmd, RegistrarVenta uc) =>
    ToHttp(await uc.EjecutarAsync(cmd)))
.WithName("RegistrarVenta").WithTags("PuntoVenta")
.RequireAuthorization(p => p.RequireRole(Administrador, Recepcionista));

// Editar un producto (Admin). El veterinariaId sale del token.
app.MapPut("/api/productos/{id:guid}", async (Guid id, EditarProductoDto dto, ClaimsPrincipal user, EditarProducto uc) =>
{
    if (!Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid veterinariaId))
        return Results.BadRequest(new { error = "Token sin veterinaria válida." });
    var comando = new EditarProductoComando(id, veterinariaId, dto.Nombre, dto.Categoria, dto.Precio);
    return ToHttp(await uc.EjecutarAsync(comando));
})
.WithName("EditarProducto").WithTags("PuntoVenta").RequireAuthorization(p => p.RequireRole(Administrador));

// Reabastecer stock de un producto (Admin).
app.MapPost("/api/productos/{id:guid}/reabastecer", async (Guid id, ReabastecerStockDto dto, ClaimsPrincipal user, ReabastecerStock uc) =>
{
    if (!Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid veterinariaId))
        return Results.BadRequest(new { error = "Token sin veterinaria válida." });
    var comando = new ReabastecerStockComando(id, veterinariaId, dto.Cantidad);
    return ToHttp(await uc.EjecutarAsync(comando));
})
.WithName("ReabastecerStock").WithTags("PuntoVenta").RequireAuthorization(p => p.RequireRole(Administrador));

// Desactivar (baja lógica) un producto del catálogo (Admin).
app.MapPost("/api/productos/{id:guid}/desactivar", async (Guid id, ClaimsPrincipal user, DesactivarProducto uc) =>
{
    if (!Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid veterinariaId))
        return Results.BadRequest(new { error = "Token sin veterinaria válida." });
    return ToHttp(await uc.EjecutarAsync(id, veterinariaId));
})
.WithName("DesactivarProducto").WithTags("PuntoVenta").RequireAuthorization(p => p.RequireRole(Administrador));

// Historial de ventas de la veterinaria (Admin), con rango de fechas opcional.
app.MapGet("/api/veterinarias/{veterinariaId:guid}/ventas", async (Guid veterinariaId, DateTime? desde, DateTime? hasta, ListarVentas uc) =>
    Results.Ok(await uc.EjecutarAsync(veterinariaId, desde, hasta)))
.WithName("ListarVentas").WithTags("PuntoVenta").RequireAuthorization(p => p.RequireRole(Administrador));

// Resumen de ventas (total, conteo, desglose por método de pago) en un rango.
app.MapGet("/api/veterinarias/{veterinariaId:guid}/ventas/resumen", async (Guid veterinariaId, DateTime? desde, DateTime? hasta, ResumenVentas uc) =>
    Results.Ok(await uc.EjecutarAsync(veterinariaId, desde, hasta)))
.WithName("ResumenVentas").WithTags("PuntoVenta").RequireAuthorization(p => p.RequireRole(Administrador));

// Métricas del dashboard (ventas hoy/mes, citas próximas, clientes activos). Solo Admin.
app.MapGet("/api/veterinarias/{veterinariaId:guid}/metricas", async (Guid veterinariaId, Chiron.Application.Metricas.MetricasDashboard uc) =>
    Results.Ok(await uc.EjecutarAsync(veterinariaId)))
.WithName("MetricasDashboard").WithTags("Metricas").RequireAuthorization(p => p.RequireRole(Administrador));

// Historial de compras de un cliente (Admin/Recepcionista).
app.MapGet("/api/clientes/{clienteId:guid}/ventas", async (Guid clienteId, ListarVentasDeCliente uc) =>
    Results.Ok(await uc.EjecutarAsync(clienteId)))
.WithName("VentasDeCliente").WithTags("PuntoVenta").RequireAuthorization(p => p.RequireRole(Administrador, Recepcionista));

// ═══════════════════ RECORDATORIOS (admin) ═══════════════════
app.MapPost("/api/veterinarias/{veterinariaId:guid}/recordatorios/enviar", async (Guid veterinariaId, int? dias, EnviarRecordatorios uc) =>
    Results.Ok(await uc.EjecutarAsync(veterinariaId, dias ?? 7)))
.WithName("EnviarRecordatorios").WithTags("Recordatorios")
.RequireAuthorization(p => p.RequireRole(Administrador));

// Lista los recordatorios pendientes de la veterinaria para que el STAFF los vea y actúe
// (el "gancho": saber a quién recordar para que el cliente vuelva). El veterinariaId sale
// del token (aislamiento multi-tenant). Lo pueden ver los roles operativos y el admin.
app.MapGet("/api/recordatorios", async (ClaimsPrincipal user, int? dias, GenerarRecordatorios uc) =>
{
    if (!Guid.TryParse(user.FindFirst("veterinariaId")?.Value, out Guid veterinariaId))
        return Results.BadRequest(new { error = "El token no tiene veterinariaId." });
    return Results.Ok(await uc.DetectarParaStaffAsync(veterinariaId, dias ?? 30));
})
.WithName("ListarRecordatorios").WithTags("Recordatorios")
.RequireAuthorization(p => p.RequireRole(Administrador, Veterinario, Recepcionista));

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

// DTO para que el Administrador cree staff de su veterinaria (el VeterinariaId sale del token).
record CrearStaffDto(string NombreUsuario, string Nombre, string Pin, RolUsuario Rol);

// DTO para crear el acceso de un dueño de mascota (por su cliente).
record CrearDuenoDto(Guid ClienteId, string Pin);

// DTO para resetear el PIN de un usuario.
record ResetearPinDto(string NuevoPin);

// ── DTOs de las mejoras (mejoras-mvp) ──
record CambiarEstadoCitaDto(AccionCita Accion);
record EditarProductoDto(string Nombre, CategoriaProducto Categoria, decimal Precio);
record ReabastecerStockDto(int Cantidad);
record CrearClienteDto(string Nombre, string Telefono, OrigenCliente Origen);
record EditarClienteDto(string Nombre, string Telefono, OrigenCliente Origen);
record AgregarMascotaDto(
    Guid ClienteId, string Nombre, EspecieMascota Especie, SexoMascota Sexo,
    string? Raza, DateOnly? FechaNacimiento, decimal? PesoKg, string? Padecimientos, bool? Esterilizado);
record EditarMascotaDto(
    string Nombre, EspecieMascota Especie, SexoMascota Sexo,
    string? Raza, DateOnly? FechaNacimiento, decimal? PesoKg, string? Padecimientos, bool? Esterilizado);
record CambiarMiPinDto(string PinActual, string NuevoPin);
record GestionarUsuarioDto(string? NuevoNombre, AccionUsuario? Accion);
record EstadoActivoDto(bool Activar);
record AdminOperativoDto(bool Operativo);
