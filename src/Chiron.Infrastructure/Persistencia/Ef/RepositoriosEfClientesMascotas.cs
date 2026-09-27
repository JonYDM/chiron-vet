using Chiron.Application.Clientes;
using Chiron.Application.Mascotas;
using Chiron.Domain.Clientes;
using Chiron.Domain.Mascotas;
using Microsoft.EntityFrameworkCore;

namespace Chiron.Infrastructure.Persistencia.Ef;

/// <summary>Implementación EF Core de IClienteRepository.</summary>
public sealed class ClienteRepositorioEf : RepositorioEf<Cliente>, IClienteRepository
{
    public ClienteRepositorioEf(ChironDbContext contexto) : base(contexto) { }

    public async Task<IReadOnlyList<Cliente>> BuscarPorNombreAsync(
        Guid veterinariaId, string texto, CancellationToken cancellationToken = default)
        => await Conjunto
            .Where(c => c.VeterinariaId == veterinariaId && EF.Functions.ILike(c.Nombre, $"%{texto}%"))
            .ToListAsync(cancellationToken);

    public async Task<Cliente?> ObtenerPorTelefonoAsync(
        Guid veterinariaId, string telefono, CancellationToken cancellationToken = default)
        => await Conjunto.FirstOrDefaultAsync(
            c => c.VeterinariaId == veterinariaId && c.Telefono == telefono, cancellationToken);

    public async Task<IReadOnlyList<Cliente>> ListarPorVeterinariaAsync(
        Guid veterinariaId, CancellationToken cancellationToken = default)
        => await Conjunto.Where(c => c.VeterinariaId == veterinariaId).ToListAsync(cancellationToken);
}

/// <summary>Implementación EF Core de IMascotaRepository.</summary>
public sealed class MascotaRepositorioEf : RepositorioEf<Mascota>, IMascotaRepository
{
    public MascotaRepositorioEf(ChironDbContext contexto) : base(contexto) { }

    public async Task<IReadOnlyList<Mascota>> ObtenerPorClienteAsync(
        Guid clienteId, CancellationToken cancellationToken = default)
        => await Conjunto.Where(m => m.ClienteId == clienteId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Mascota>> ListarPorVeterinariaAsync(
        Guid veterinariaId, CancellationToken cancellationToken = default)
        => await Conjunto.Where(m => m.VeterinariaId == veterinariaId).ToListAsync(cancellationToken);
}

/// <summary>Implementación EF Core de IFotoMascotaRepository.</summary>
public sealed class FotoMascotaRepositorioEf : RepositorioEf<FotoMascota>, IFotoMascotaRepository
{
    public FotoMascotaRepositorioEf(ChironDbContext contexto) : base(contexto) { }

    public async Task<IReadOnlyList<FotoMascota>> ListarPorMascotaAsync(
        Guid mascotaId, CancellationToken cancellationToken = default)
        => await Conjunto
            .Where(f => f.MascotaId == mascotaId)
            .OrderByDescending(f => f.FechaSubida)
            .ToListAsync(cancellationToken);
}
