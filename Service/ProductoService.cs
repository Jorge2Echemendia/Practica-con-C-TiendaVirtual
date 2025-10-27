using TiendaVirtual.Model;
using Microsoft.EntityFrameworkCore;
using TiendaVirtual.Context;
using Microsoft.AspNetCore.SignalR;
using TiendaVirtual;
using System.Threading.Tasks.Dataflow;

public class ProductoService
{
    private readonly IDbContextFactory<AppDbContext> _context;
    private readonly IHubContext<StockHub> _hubContext;

    public ProductoService(IDbContextFactory<AppDbContext> context, IHubContext<StockHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<List<Producto>> ObtenerProductosDisponiblesAsync()
    {
        using var context = _context.CreateDbContext();
        return await context.Productos
            .ToListAsync();
    }
    public async Task<bool> ReducirStockAsync(int productoId, int cantidadReducir)
    {
        using var context = _context.CreateDbContext();
        var producto = await context.Productos.FindAsync(productoId);
        if (producto == null || producto.CantidadProducto < cantidadReducir)
            return false;

        producto.CantidadProducto -= cantidadReducir;
        await context.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("ActualizarStock", producto.Id, producto.CantidadProducto);
        return true;
    }
    public async Task<bool> ValidarStockAsync(int productoId, int cantidadSolicitada)
    {
        using var context = _context.CreateDbContext();
        var producto = await context.Productos.FindAsync(productoId);
        return producto != null && producto.CantidadProducto >= cantidadSolicitada;
    }


    public async Task AgregarProductoAsync(Producto producto)
    {
        using var context = _context.CreateDbContext();
        context.Productos.Add(producto);
        await context.SaveChangesAsync();
    }
    public async Task<List<Producto>> ObtenerProductosEnPromocionAsync()
    {
        using var context = _context.CreateDbContext();
        return await context.Productos
            .Where(p => p.EnPromocion == "Yes" && p.CantidadProducto > 0)
            .ToListAsync();
    }

    public async Task<Producto> ObtenerProductoPorIdAsync(int id)
    {
        using var context = _context.CreateDbContext();
        return await context.Productos.FindAsync(id);
    }

    public async Task ActualizarProductoAsync(Producto producto)
    {
        using var context = _context.CreateDbContext();

        var productoDb = await context.Productos.FindAsync(producto.Id);
        if (productoDb == null)
        {
            Console.WriteLine("Producto no encontrado en la base de datos.");
            return;
        }

        // Asignar propiedades manualmente
        productoDb.Nombre = producto.Nombre;
        productoDb.Descripcion = producto.Descripcion;
        productoDb.Precio = producto.Precio;
        productoDb.CantidadProducto = producto.CantidadProducto;
        productoDb.Categoria = producto.Categoria;
        productoDb.EnPromocion = producto.EnPromocion;
        productoDb.PrecioPromocional = producto.PrecioPromocional;
        productoDb.TiempoPromocion = producto.TiempoPromocion;

        Console.WriteLine($"Guardando: {productoDb.PrecioPromocional} - {productoDb.TiempoPromocion}");

        await context.SaveChangesAsync();
    }

    public async Task EliminarPromocionesExpiradasAsync()
    {
        using var context = _context.CreateDbContext();
        var cubaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Cuba Standard Time");
        var hoy = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cubaTimeZone);
        Console.WriteLine($"Hoy:{hoy}");

        var productosExpirados = await context.Productos
            .Where(p => p.EnPromocion == "Yes" && p.TiempoPromocion != null && p.TiempoPromocion < hoy)
            .ToListAsync();

        foreach (var producto in productosExpirados)
        {
            producto.PrecioPromocional = null;
            producto.TiempoPromocion = null;
            producto.EnPromocion = "No";
        }

        await context.SaveChangesAsync();
    }


}

