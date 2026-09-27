using Chiron.Application.Seguridad;
using Chiron.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Chiron.Infrastructure.Persistencia.Ef;

/// <summary>Implementación EF Core de IUsuarioRepository.</summary>
public sealed class UsuarioRepositorioEf : RepositorioEf<Usuario>, IUsuarioRepository
{
    public UsuarioRepositorioEf(ChironDbContext contexto) : base(contexto) { }

    public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default)
        => await Conjunto.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario, cancellationToken);

    public async Task<IReadOnlyList<Usuario>> ListarPorVeterinariaAsync(
        Guid veterinariaId, CancellationToken cancellationToken = default)
        => await Conjunto.Where(u => u.VeterinariaId == veterinariaId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Usuario>> ListarPorRolAsync(
        RolUsuario rol, CancellationToken cancellationToken = default)
        => await Conjunto.Where(u => u.Rol == rol).ToListAsync(cancellationToken);

    public async Task<Usuario?> ObtenerPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
        => await Conjunto.FirstOrDefaultAsync(u => u.ClienteId == clienteId, cancellationToken);
}
