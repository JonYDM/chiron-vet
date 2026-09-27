using Chiron.Application.Citas;
using Chiron.Application.Clientes;
using Chiron.Application.Common;
using Chiron.Application.Expedientes;
using Chiron.Application.Mascotas;
using Chiron.Application.PuntoVenta;
using Chiron.Application.Recordatorios;
using Chiron.Application.Seguridad;
using Chiron.Domain.Clientes;
using Chiron.Domain.Mascotas;
using Chiron.Domain.Usuarios;
using Chiron.Infrastructure.Mensajeria;
using Chiron.Infrastructure.Persistencia;
using Chiron.Infrastructure.Persistencia.Ef;
using Chiron.Infrastructure.Seguridad;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chiron.Infrastructure;

/// <summary>
/// Registro de dependencias de la capa de Infraestructura.
/// Dos modos de persistencia intercambiables (ambos cumplen los mismos contratos):
///  - En memoria (AddInfrastructure): para consola/demo, sin base de datos.
///  - PostgreSQL con EF Core (AddInfrastructurePostgres): para producción.
/// El segundo parámetro (jwtOpciones) configura la firma de tokens; si es null se usa
/// una configuración por defecto (solo apta para pruebas locales).
/// </summary>
public static class DependencyInjection
{
    /// <summary>Persistencia EN MEMORIA (por defecto para consola/demo).</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, JwtOpciones? jwtOpciones = null)
    {
        services.AddSingleton(typeof(IRepository<>), typeof(RepositorioEnMemoria<>));

        services.AddSingleton<ClienteRepositorioEnMemoria>();
        services.AddSingleton<IClienteRepository>(sp => sp.GetRequiredService<ClienteRepositorioEnMemoria>());
        services.AddSingleton<IRepository<Cliente>>(sp => sp.GetRequiredService<ClienteRepositorioEnMemoria>());

        services.AddSingleton<MascotaRepositorioEnMemoria>();
        services.AddSingleton<IMascotaRepository>(sp => sp.GetRequiredService<MascotaRepositorioEnMemoria>());
        services.AddSingleton<IRepository<Mascota>>(sp => sp.GetRequiredService<MascotaRepositorioEnMemoria>());

        services.AddSingleton<RegistroMedicoRepositorioEnMemoria>();
        services.AddSingleton<IRegistroMedicoRepository>(sp => sp.GetRequiredService<RegistroMedicoRepositorioEnMemoria>());

        services.AddSingleton<CitaRepositorioEnMemoria>();
        services.AddSingleton<ICitaRepository>(sp => sp.GetRequiredService<CitaRepositorioEnMemoria>());

        services.AddSingleton<ProductoRepositorioEnMemoria>();
        services.AddSingleton<IProductoRepository>(sp => sp.GetRequiredService<ProductoRepositorioEnMemoria>());

        services.AddSingleton<VentaRepositorioEnMemoria>();
        services.AddSingleton<IVentaRepository>(sp => sp.GetRequiredService<VentaRepositorioEnMemoria>());

        services.AddSingleton<UsuarioRepositorioEnMemoria>();
        services.AddSingleton<IUsuarioRepository>(sp => sp.GetRequiredService<UsuarioRepositorioEnMemoria>());
        services.AddSingleton<IRepository<Usuario>>(sp => sp.GetRequiredService<UsuarioRepositorioEnMemoria>());

        AddMensajeria(services);
        AddSeguridad(services, jwtOpciones);
        return services;
    }

    /// <summary>Persistencia con PostgreSQL vía EF Core (producción).</summary>
    public static IServiceCollection AddInfrastructurePostgres(
        this IServiceCollection services, string connectionString, JwtOpciones? jwtOpciones = null)
    {
        services.AddDbContext<ChironDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(RepositorioEf<>));

        services.AddScoped<IClienteRepository, ClienteRepositorioEf>();
        services.AddScoped<IMascotaRepository, MascotaRepositorioEf>();
        services.AddScoped<IRegistroMedicoRepository, RegistroMedicoRepositorioEf>();
        services.AddScoped<ICitaRepository, CitaRepositorioEf>();
        services.AddScoped<IProductoRepository, ProductoRepositorioEf>();
        services.AddScoped<IVentaRepository, VentaRepositorioEf>();
        services.AddScoped<IUsuarioRepository, UsuarioRepositorioEf>();

        AddMensajeria(services);
        AddSeguridad(services, jwtOpciones);
        return services;
    }

    // Servicio de mensajería: implementación de PRUEBA (log), común a ambos modos.
    private static void AddMensajeria(IServiceCollection services)
        => services.AddSingleton<IServicioMensajeria, MensajeriaConsola>();

    // Servicios de seguridad comunes: hasheo de contraseñas y generación de JWT.
    private static void AddSeguridad(IServiceCollection services, JwtOpciones? jwtOpciones)
    {
        services.AddSingleton<IHasheadorContrasena, HasheadorBCrypt>();

        // Clave por defecto SOLO para pruebas locales; en producción viene de configuración.
        var opciones = jwtOpciones ?? new JwtOpciones
        {
            Clave = "clave-de-desarrollo-solo-local-cambiar-en-produccion-1234567890"
        };
        services.AddSingleton(opciones);
        services.AddSingleton<IGeneradorToken, GeneradorTokenJwt>();
    }
}
