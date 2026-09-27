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
}
