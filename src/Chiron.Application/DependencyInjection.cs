using Chiron.Application.Citas;
using Chiron.Application.Clientes;
using Chiron.Application.Expedientes;
using Chiron.Application.Mascotas;
using Chiron.Application.PuntoVenta;
using Chiron.Application.Recordatorios;
using Chiron.Application.Seguridad;
using Microsoft.Extensions.DependencyInjection;

namespace Chiron.Application;

/// <summary>
/// Punto único de registro de dependencias de la capa de Aplicación.
/// Cada capa es responsable de registrar sus propios servicios (SOLID: SRP).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra los casos de uso de la capa de Aplicación.
    /// Se registran como Transient: son operaciones sin estado, se crea una por uso.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddTransient<RegistrarClienteConMascota>();
        services.AddTransient<BuscarClientes>();
        services.AddTransient<ListarMascotasDeCliente>();

        services.AddTransient<AgregarRegistroMedico>();
        services.AddTransient<VerExpedienteMascota>();

        services.AddTransient<AgendarCita>();
        services.AddTransient<VerAgenda>();

        services.AddTransient<GenerarRecordatorios>();
        services.AddTransient<EnviarRecordatorios>();

        services.AddTransient<AgregarProducto>();
        services.AddTransient<ListarCatalogo>();
        services.AddTransient<RegistrarVenta>();

        services.AddTransient<Login>();

        return services;
    }
}
