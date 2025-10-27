using Microsoft.EntityFrameworkCore;
using TiendaVirtual.Context;
using TiendaVirtual.Model;

namespace TiendaVirtual.Service;

public class TarjetaService
{
    private readonly IDbContextFactory<AppDbContext> _context;


    public TarjetaService(IDbContextFactory<AppDbContext> context)
    {
        _context = context;
    }
    public async Task<TarjetaFicticia> CrearTarjetaVirtualAsync(int clienteId, int number, string password)
    {
        using var context = _context.CreateDbContext();
        var cliente = await context.Clientes.FindAsync(clienteId);
        if (cliente == null)
            throw new ArgumentException("Cliente no encontrado");

        if (cliente.Tarjeta != null)
            throw new InvalidOperationException("El cliente ya tiene una tarjeta virtual");

        var tarjetaExistente = await context.TarjetasFicticias
.FirstOrDefaultAsync(t => t.Numero == number);

        if (tarjetaExistente != null)
            throw new InvalidOperationException("Ya existe una tarjeta con ese número");

        if (string.IsNullOrWhiteSpace(password))
    throw new ArgumentException("La contraseña no puede estar vacía");
        var tarjeta = new TarjetaFicticia
        {
            Numero = number,
            Saldo = TarjetaFicticia.GenerarSaldoFicticio(),
            TarjPassword = HashPassword(password)

        };

        cliente.Tarjeta = tarjeta;
        await context.SaveChangesAsync();

        return tarjeta;
    }

    public async Task<TarjetaFicticia?> ObtenerTarjetaVirtualAsync(int clienteId)
    {
        using var context = _context.CreateDbContext();
        return await context.Clientes
            .Include(c => c.Tarjeta)
            .Where(c => c.Id == clienteId)
            .Select(c => c.Tarjeta)
            .FirstOrDefaultAsync();
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
