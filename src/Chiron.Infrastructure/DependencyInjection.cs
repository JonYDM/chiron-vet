using Chiron.Application.Citas;
using Chiron.Application.Clientes;
using Chiron.Application.Common;
using Chiron.Application.Expedientes;
using Chiron.Application.Mascotas;
using Chiron.Application.PuntoVenta;
using Chiron.Application.Recordatorios;
using Chiron.Domain.Clientes;
using Chiron.Domain.Mascotas;
using Chiron.Infrastructure.Mensajeria;
using Chiron.Infrastructure.Persistencia;
using Chiron.Infrastructure.Persistencia.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chiron.Infrastructure;

/// <summary>
/// Registro de dependencias de la capa de Infraestructura.
/// Dos modos de persistencia intercambiables (ambos cumplen los mismos contratos):
///  - En memoria (AddInfrastructure): para consola/demo, sin base de datos.
///  - PostgreSQL con EF Core (AddInfrastructurePostgres): para producción.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Persistencia EN MEMORIA (por defecto para consola/demo).</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Repositorio genérico en memoria (Veterinaria, Usuario, etc.).
        services.AddSingleton(typeof(IRepository<>), typeof(RepositorioEnMemoria<>));

        // Repositorios específicos como Singleton; IRepository<T> redirige a la misma instancia.
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

        AddMensajeria(services);
        return services;
    }

    /// <summary>Persistencia con PostgreSQL vía EF Core (producción).</summary>
    public static IServiceCollection AddInfrastructurePostgres(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ChironDbContext>(options => options.UseNpgsql(connectionString));

        // Repositorio genérico EF para entidades sin repositorio específico.
        services.AddScoped(typeof(IRepository<>), typeof(RepositorioEf<>));

        // Repositorios específicos EF.
        services.AddScoped<IClienteRepository, ClienteRepositorioEf>();
        services.AddScoped<IMascotaRepository, MascotaRepositorioEf>();
        services.AddScoped<IRegistroMedicoRepository, RegistroMedicoRepositorioEf>();
        services.AddScoped<ICitaRepository, CitaRepositorioEf>();
        services.AddScoped<IProductoRepository, ProductoRepositorioEf>();
        services.AddScoped<IVentaRepository, VentaRepositorioEf>();

        AddMensajeria(services);
        return services;
    }

    // Servicio de mensajería: implementación de PRUEBA (log), común a ambos modos.
    // Se sustituirá por WhatsApp Cloud API sin cambiar la lógica de negocio.
    private static void AddMensajeria(IServiceCollection services)
        => services.AddSingleton<IServicioMensajeria, MensajeriaConsola>();
}
