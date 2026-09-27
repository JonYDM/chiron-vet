using Chiron.Application.Clientes;
using Chiron.Application.Common;
using Chiron.Application.Mascotas;
using Chiron.Domain.Clientes;
using Chiron.Domain.Mascotas;
using Chiron.Infrastructure.Persistencia;
using Microsoft.Extensions.DependencyInjection;

namespace Chiron.Infrastructure;

/// <summary>
/// Punto único de registro de dependencias de la capa de Infraestructura.
/// Registra las implementaciones concretas que satisfacen los contratos de Application.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Repositorio genérico en memoria por defecto (para entidades sin repositorio específico,
        // como Veterinaria y Usuario). Singleton para que los datos vivan toda la ejecución.
        services.AddSingleton(typeof(IRepository<>), typeof(RepositorioEnMemoria<>));

        // ── Repositorios específicos ──
        // Se registran como Singleton (una sola instancia con sus datos).
        // Además, IRepository<Cliente> se redirige a ESA MISMA instancia de
        // IClienteRepository, para que los datos sean consistentes sin importar
        // por cuál contrato se pida. Igual para Mascota.
        services.AddSingleton<ClienteRepositorioEnMemoria>();
        services.AddSingleton<IClienteRepository>(sp => sp.GetRequiredService<ClienteRepositorioEnMemoria>());
        services.AddSingleton<IRepository<Cliente>>(sp => sp.GetRequiredService<ClienteRepositorioEnMemoria>());

        services.AddSingleton<MascotaRepositorioEnMemoria>();
        services.AddSingleton<IMascotaRepository>(sp => sp.GetRequiredService<MascotaRepositorioEnMemoria>());
        services.AddSingleton<IRepository<Mascota>>(sp => sp.GetRequiredService<MascotaRepositorioEnMemoria>());

        return services;
    }
}
