using Microsoft.EntityFrameworkCore;
using TiendaVirtual.Model;
using Oracle.EntityFrameworkCore;

namespace TiendaVirtual.Context;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<ItemCarrito> ItemsCarrito { get; set; }
    public DbSet<TarjetaFicticia> TarjetasFicticias { get; set; }
    public DbSet<ReciboCompra> ReciboCompra { get; set; }
    public DbSet<Compra> Compra { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<DeliveryPerson> DeliveryPersons { get; set; }

    public DbSet<Notificacion> Notificacion { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Relación 1:1 entre User y Cliente
        modelBuilder.Entity<User>()
            .HasOne(u => u.Cliente)
            .WithOne(c => c.User)
            .HasForeignKey<Cliente>(c => c.UserId);

        //  Relación 1:1 entre Cliente y TarjetaFicticia
        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.Tarjeta)
            .WithOne()
            .HasForeignKey<Cliente>(c => c.TarjetaFicticiaId)
            .OnDelete(DeleteBehavior.SetNull);
        // Relación 1:N entre Compra y ItemCarrito
        modelBuilder.Entity<ItemCarrito>()
            .HasOne(i => i.Compra)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CompraId)
            .OnDelete(DeleteBehavior.SetNull);
        // Relación 1:N entre Cliente y Compra
        modelBuilder.Entity<Compra>()
            .HasOne(c => c.Cliente)
            .WithMany(c => c.Compras)
            .HasForeignKey(c => c.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        //  Relación 1:N entre Cliente y ItemCarrito
        modelBuilder.Entity<ItemCarrito>()
      .HasOne(i => i.Cliente)
      .WithMany(c => c.Carrito)
      .HasForeignKey(i => i.ClientId)
      .OnDelete(DeleteBehavior.Cascade);

     modelBuilder.Entity<Notificacion>()
    .HasOne(n => n.User)
    .WithMany(u => u.Notificaciones)
    .HasForeignKey(n => n.UserId)
    .OnDelete(DeleteBehavior.SetNull);

        //  Relación 1:N entre Cliente y ReciboCompra
        modelBuilder.Entity<ReciboCompra>()
            .HasOne(r => r.Cliente)
            .WithMany(c => c.ReciboCompra)
            .HasForeignKey(r => r.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // 🔗 Relación N:1 entre ItemCarrito y Producto
        modelBuilder.Entity<ItemCarrito>()
            .HasOne(i => i.Producto)
            .WithMany(p => p.ItemsCarrito)
            .HasForeignKey(i => i.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<DeliveryPerson>()
    .HasOne(dp => dp.User)
    .WithOne()
    .HasForeignKey<DeliveryPerson>(dp => dp.UserId)
    .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Compra>()
            .HasOne(c => c.DeliveryPerson)
            .WithMany(dp => dp.Entregas)
            .HasForeignKey(c => c.DeliveryPersonId)
            .OnDelete(DeleteBehavior.SetNull);


        modelBuilder.Entity<Producto>()
            .Property(p => p.Precio)
             .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Producto>()
            .Property(p => p.PrecioPromocional)
            .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Producto>()
           .Property(p => p.CantidadProducto)
           .HasPrecision(10, 2);
        modelBuilder.Entity<TarjetaFicticia>()
             .Property(t => t.Saldo)
             .HasPrecision(10, 2);
        modelBuilder.Entity<Compra>()
   .Property(c => c.Total)
   .HasColumnType("decimal(18,2)");
           modelBuilder.Entity<Compra>()
   .Property(c => c.Lat)
   .HasColumnType("decimal(18,2)");
           modelBuilder.Entity<Compra>()
   .Property(c => c.Lon)
   .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<ReciboCompra>()
   .Property(r => r.Total)
   .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<ReciboCompra>()
    .HasIndex(r => r.CodigoAutenticacion)
    .IsUnique();
        modelBuilder.Entity<ItemCarrito>()
      .Property(r => r.PrecioUnitario)
      .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Producto>()
      .Property(p => p.Id)
      .ValueGeneratedOnAdd();

        modelBuilder.Entity<ItemCarrito>()
            .Property(i => i.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Compra>()
            .Property(c => c.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<ReciboCompra>()
            .Property(r => r.Id)
            .ValueGeneratedOnAdd();


    }
}
