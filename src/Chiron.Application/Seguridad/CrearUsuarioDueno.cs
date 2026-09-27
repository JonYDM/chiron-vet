using Chiron.Application.Clientes;
using Chiron.Domain.Clientes;
using Chiron.Domain.Common;
using Chiron.Domain.Usuarios;

namespace Chiron.Application.Seguridad;

/// <summary>Datos para crear un acceso de dueño de mascota (ligado a un cliente existente).</summary>
public sealed record CrearUsuarioDuenoComando(Guid VeterinariaId, Guid ClienteId, string Pin);

/// <summary>
/// Caso de uso: crear el acceso de un dueño de mascota. El identificador de login será
/// el teléfono del cliente. Valida el PIN, que el cliente exista y pertenezca a la
/// veterinaria, y que no tenga ya un acceso creado.
/// </summary>
public sealed class CrearUsuarioDueno
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IClienteRepository _clientes;
    private readonly IHasheadorContrasena _hasheador;

    public CrearUsuarioDueno(
        IUsuarioRepository usuarios, IClienteRepository clientes, IHasheadorContrasena hasheador)
    {
        _usuarios = usuarios;
        _clientes = clientes;
        _hasheador = hasheador;
    }

    public async Task<Result<Guid>> EjecutarAsync(
        CrearUsuarioDuenoComando comando, CancellationToken cancellationToken = default)
    {
        Result<bool> pinValido = ValidadorPin.Validar(comando.Pin);
        if (!pinValido.EsExito)
            return Result<Guid>.Falla(pinValido.Error!);

        Cliente? cliente = await _clientes.ObtenerPorIdAsync(comando.ClienteId, cancellationToken);
        if (cliente is null)
            return Result<Guid>.Falla("El cliente no existe.");
        if (cliente.VeterinariaId != comando.VeterinariaId)
            return Result<Guid>.Falla("El cliente no pertenece a la veterinaria indicada.");

        // El identificador del dueño es su teléfono.
        string idNormalizado = Usuario.NormalizarIdentificador(cliente.Telefono);
        Usuario? existente = await _usuarios.ObtenerPorNombreUsuarioAsync(idNormalizado, cancellationToken);
        if (existente is not null)
            return Result<Guid>.Falla("Este cliente ya tiene un acceso creado.");

        string hash = _hasheador.Hashear(comando.Pin);
        Result<Usuario> usuarioResult = Usuario.CrearDueno(
            comando.VeterinariaId, cliente.Id, cliente.Telefono, cliente.Nombre, hash);
        if (!usuarioResult.EsExito)
            return Result<Guid>.Falla(usuarioResult.Error!);

        Usuario usuario = usuarioResult.Valor!;
        await _usuarios.AgregarAsync(usuario, cancellationToken);
        return Result<Guid>.Exito(usuario.Id);
    }
}
