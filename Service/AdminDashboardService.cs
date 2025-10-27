using Microsoft.EntityFrameworkCore;
using TiendaVirtual.Context;
using TiendaVirtual.Model;

namespace TiendaVirtual.Service;

public class AdminDashboardService
{
    private readonly IDbContextFactory<AppDbContext> _context;

    public AdminDashboardService(IDbContextFactory<AppDbContext> context)
    {
        _context = context;
    }

    public async Task<List<(string Nombre, int Stock)>> ObtenerInventarioAsync()
    {
        using var db = _context.CreateDbContext();
        return await db.Productos
            .Select(p => new ValueTuple<string, int>(p.Nombre ?? "Sin nombre", p.CantidadProducto ?? 0))
            .ToListAsync();
    }

    public async Task<List<(string Cliente, decimal Total)>> ObtenerTopCompradoresAsync()
    {
        using var db = _context.CreateDbContext();

        var compras = await db.Compra
            .Include(c => c.Cliente)
            .ThenInclude(c => c.User)
            .ToListAsync();

        var resultado = compras
            .GroupBy(c => c.Cliente.User.Username)
            .Select(g => (Cliente: g.Key, Total: g.Sum(c => c.Total)))
            .OrderByDescending(g => g.Total)
            .Take(10)
            .ToList();

        return resultado;
    }

    public async Task<List<(string Categoria, int Vendidos)>> ObtenerVentasPorCategoriaAsync()
    {
        using var db = _context.CreateDbContext();
        return await db.ItemsCarrito
            .Include(i => i.Producto)
            .GroupBy(i => i.Producto.Categoria)
            .Select(g => new ValueTuple<string, int>(g.Key ?? "Sin categoría", g.Sum(i => i.Cantidad)))
            .ToListAsync();
    }

    public async Task<List<Producto>> ObtenerPromocionesVigentesAsync()
    {
        using var db = _context.CreateDbContext();
        var hoy = DateTime.Now;
        return await db.Productos
            .Where(p => p.EnPromocion == "Yes" && p.TiempoPromocion > hoy)
            .ToListAsync();
    }
    public async Task<List<User>> ObtenerUsuariosRecientesAsync()
    {
        // Devuelve los últimos 10 usuarios registrados
        using var db = _context.CreateDbContext();
        return await db.Users
            .OrderByDescending(u => u.FechaRegistro)
            .Take(10)
            .ToListAsync();
    }

  public async Task<List<Compra>> ObtenerComprasRecientesAsync()
    {
        // Devuelve los últimos 10 usuarios registrados
        using var db = _context.CreateDbContext();
        return await db.Compra
            .Include(c => c.Cliente)
            .ThenInclude(cl => cl.User)
        .Include(c => c.Items)
            .ThenInclude(i => i.Producto)
        .OrderByDescending(c => c.Fecha)
        .Take(10)
        .ToListAsync();
    }
}
