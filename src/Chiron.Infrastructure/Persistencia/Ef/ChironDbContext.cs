using Chiron.Domain.Citas;
using Chiron.Domain.Clientes;
using Chiron.Domain.Cobros;
using Chiron.Domain.Expedientes;
using Chiron.Domain.Mascotas;
using Chiron.Domain.PuntoVenta;
using Chiron.Domain.Usuarios;
using Chiron.Domain.Veterinarias;
using Microsoft.EntityFrameworkCore;

namespace Chiron.Infrastructure.Persistencia.Ef;

/// <summary>
/// Contexto de Entity Framework Core para PostgreSQL.
/// Configura el mapeo de las entidades de dominio (que tienen diseño rico:
/// constructores y setters privados) a las tablas de la base de datos.
/// </summary>
public sealed class ChironDbContext : DbContext
{
    public ChironDbContext(DbContextOptions<ChironDbContext> options) : base(options) { }

    public DbSet<Veterinaria> Veterinarias => Set<Veterinaria>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Mascota> Mascotas => Set<Mascota>();
    public DbSet<FotoMascota> FotosMascota => Set<FotoMascota>();
    public DbSet<RegistroMedico> RegistrosMedicos => Set<RegistroMedico>();
    public DbSet<Cita> Citas => Set<Cita>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<Cargo> Cargos => Set<Cargo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Todas las entidades usan Id (Guid) heredado de EntidadBase como clave.
        modelBuilder.Entity<Veterinaria>(vet =>
        {
            vet.HasKey(e => e.Id);
            // Las veterinarias existentes quedan en plan Mensual al migrar.
            vet.Property(e => e.Plan).HasDefaultValue(PlanSuscripcion.Mensual);
            vet.Property(e => e.Direccion).HasMaxLength(250);
        });
        modelBuilder.Entity<Usuario>().HasKey(e => e.Id);
        modelBuilder.Entity<Cliente>().HasKey(e => e.Id);
        modelBuilder.Entity<Mascota>().HasKey(e => e.Id);
        modelBuilder.Entity<FotoMascota>().HasKey(e => e.Id);
        modelBuilder.Entity<RegistroMedico>().HasKey(e => e.Id);
        modelBuilder.Entity<Cita>().HasKey(e => e.Id);
        modelBuilder.Entity<Producto>().HasKey(e => e.Id);

        // Producto: el precio como decimal con precisión monetaria.
        modelBuilder.Entity<Producto>().Property(p => p.Precio).HasPrecision(18, 2);

        // Mascota: peso con precisión.
        modelBuilder.Entity<Mascota>().Property(m => m.PesoKg).HasPrecision(6, 2);

        // RegistroMedico: peso y temperatura con precisión.
        modelBuilder.Entity<RegistroMedico>().Property(r => r.PesoKg).HasPrecision(6, 2);
        modelBuilder.Entity<RegistroMedico>().Property(r => r.TemperaturaC).HasPrecision(4, 1);

        // Venta: clave, total con precisión, y sus líneas como entidad propiedad (owned).
        modelBuilder.Entity<Venta>(venta =>
        {
            venta.HasKey(e => e.Id);
            venta.Property(e => e.Total).HasPrecision(18, 2);
            venta.Property(e => e.MontoRecibido).HasPrecision(18, 2);
            venta.Property(e => e.Cambio).HasPrecision(18, 2);
            // La colección se expone como Lineas (solo lectura) pero se respalda en el campo _lineas.
            venta.Navigation(e => e.Lineas).HasField("_lineas").UsePropertyAccessMode(PropertyAccessMode.Field);
            // LineaVenta no tiene Id propio: se modela como colección "owned" de la Venta.
            venta.OwnsMany(e => e.Lineas, linea =>
            {
                linea.Property(l => l.PrecioUnitario).HasPrecision(18, 2);
                linea.WithOwner();
            });
            // Cargos cobrados en la venta (consultas/servicios), también owned.
            venta.Navigation(e => e.Cargos).HasField("_cargos").UsePropertyAccessMode(PropertyAccessMode.Field);
            venta.OwnsMany(e => e.Cargos, cargo =>
            {
                cargo.Property(c => c.Monto).HasPrecision(18, 2);
                cargo.WithOwner();
            });
        });

        // Cargo (cuenta por cobrar): clave + monto con precisión monetaria.
        modelBuilder.Entity<Cargo>(cargo =>
        {
            cargo.HasKey(e => e.Id);
            cargo.Property(e => e.Monto).HasPrecision(18, 2);
        });

        base.OnModelCreating(modelBuilder);
    }
}
