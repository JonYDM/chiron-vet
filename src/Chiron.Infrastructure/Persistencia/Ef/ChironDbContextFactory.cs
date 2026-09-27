using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Chiron.Infrastructure.Persistencia.Ef;

/// <summary>
/// Fábrica usada por las herramientas de EF Core (dotnet ef) en tiempo de diseño
/// para crear el DbContext al generar migraciones, sin necesidad de una base de datos activa.
/// La cadena aquí es solo un placeholder para el diseño; en ejecución se usa la real.
/// </summary>
public sealed class ChironDbContextFactory : IDesignTimeDbContextFactory<ChironDbContext>
{
    public ChironDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ChironDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=chiron;Username=chiron;Password=chiron_dev")
            .Options;
        return new ChironDbContext(options);
    }
}
