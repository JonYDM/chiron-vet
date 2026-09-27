using Chiron.Application.Common;
using Chiron.Domain.Clientes;
using Chiron.Domain.Common;
using Chiron.Domain.Veterinarias;

namespace Chiron.Application.Clientes;

/// <summary>Datos para crear un cliente (sin mascota).</summary>
public sealed record CrearClienteComando(
    Guid VeterinariaId,
    string Nombre,
    string Telefono,
    OrigenCliente Origen);

/// <summary>
/// Caso de uso: crear SOLO un cliente (sin mascota). Para quien llega a comprar o
/// aún no registra mascota. Complementa al registro rápido (cliente + mascota).
/// </summary>
public sealed class CrearCliente
{
    private readonly IClienteRepository _clientes;
    private readonly IRepository<Veterinaria> _veterinarias;

    public CrearCliente(IClienteRepository clientes, IRepository<Veterinaria> veterinarias)
    {
        _clientes = clientes;
        _veterinarias = veterinarias;
    }

    public async Task<Result<Guid>> EjecutarAsync(
        CrearClienteComando comando, CancellationToken cancellationToken = default)
    {
        Veterinaria? vet = await _veterinarias.ObtenerPorIdAsync(comando.VeterinariaId, cancellationToken);
        if (vet is null)
            return Result<Guid>.Falla("La veterinaria indicada no existe.");
        if (!vet.Activa)
            return Result<Guid>.Falla("La veterinaria está inactiva.");

        Result<Cliente> clienteResult = Cliente.Crear(
            comando.VeterinariaId, comando.Nombre, comando.Telefono, comando.Origen);
        if (!clienteResult.EsExito)
            return Result<Guid>.Falla(clienteResult.Error!);

        Cliente cliente = clienteResult.Valor!;
        await _clientes.AgregarAsync(cliente, cancellationToken);
        return Result<Guid>.Exito(cliente.Id);
    }
}

/// <summary>Datos para editar un cliente.</summary>
public sealed record EditarClienteComando(
    Guid ClienteId,
    Guid VeterinariaId,
    string Nombre,
    string Telefono,
    OrigenCliente Origen);

/// <summary>Caso de uso: editar los datos de un cliente (multi-tenant).</summary>
public sealed class EditarCliente
{
    private readonly IClienteRepository _clientes;

    public EditarCliente(IClienteRepository clientes) => _clientes = clientes;

    public async Task<Result<bool>> EjecutarAsync(
        EditarClienteComando comando, CancellationToken cancellationToken = default)
    {
        Cliente? cliente = await _clientes.ObtenerPorIdAsync(comando.ClienteId, cancellationToken);
        if (cliente is null)
            return Result<bool>.Falla("El cliente no existe.");
        if (cliente.VeterinariaId != comando.VeterinariaId)
            return Result<bool>.Falla("El cliente no pertenece a tu veterinaria.");

        Result<bool> res = cliente.ActualizarDatos(comando.Nombre, comando.Telefono, comando.Origen);
        if (!res.EsExito) return res;

        await _clientes.ActualizarAsync(cliente, cancellationToken);
        return Result<bool>.Exito(true);
    }
}

/// <summary>
/// Caso de uso: activar o desactivar (baja lógica) un cliente.
/// </summary>
public sealed class CambiarEstadoCliente
{
    private readonly IClienteRepository _clientes;

    public CambiarEstadoCliente(IClienteRepository clientes) => _clientes = clientes;

    public async Task<Result<bool>> EjecutarAsync(
        Guid clienteId, Guid veterinariaId, bool activar, CancellationToken cancellationToken = default)
    {
        Cliente? cliente = await _clientes.ObtenerPorIdAsync(clienteId, cancellationToken);
        if (cliente is null)
            return Result<bool>.Falla("El cliente no existe.");
        if (cliente.VeterinariaId != veterinariaId)
            return Result<bool>.Falla("El cliente no pertenece a tu veterinaria.");

        if (activar) cliente.Activar();
        else cliente.Desactivar();

        await _clientes.ActualizarAsync(cliente, cancellationToken);
        return Result<bool>.Exito(true);
    }
}
