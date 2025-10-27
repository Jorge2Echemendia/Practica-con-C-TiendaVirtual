using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using TiendaVirtual.Context;
using TiendaVirtual.Model;
using TiendaVirtual.Service;

namespace TiendaVirtual.Service;


public class CarritoService
{
    private List<ItemCarrito> itemsCarrito = new();

    public event EventHandler? OnChange;

    private readonly IDbContextFactory<AppDbContext> _context;
    private readonly TarjetaService _tarjetaService;

    public CarritoService(IDbContextFactory<AppDbContext> context, TarjetaService tarjetaService)
    {
        _context = context;
        _tarjetaService = tarjetaService;
    }

    public void AgregarAlCarrito(Producto producto, int clienteId)
    {
        if (producto == null) return;

        var itemExistente = itemsCarrito.FirstOrDefault(i => i.Producto?.Id == producto.Id && i.ClientId == clienteId);
        if (itemExistente != null)
        {
            itemExistente.Cantidad++;
        }
        else
        {
            itemsCarrito.Add(new ItemCarrito
            {
                Producto = producto,
                ProductoId = producto.Id,
                Cantidad = 1,
                ClientId = clienteId
            });
        }

        OnChange?.Invoke(this, EventArgs.Empty);
    }

    public void EliminarDelCarrito(Producto producto, int clienteId)
    {
        if (producto == null) return;

        var itemExistente = itemsCarrito.FirstOrDefault(i => i.Producto?.Id == producto.Id && i.ClientId == clienteId);
        if (itemExistente != null)
        {
            itemExistente.Cantidad--;
            if (itemExistente.Cantidad <= 0)
            {
                itemsCarrito.Remove(itemExistente);
            }
        }

        OnChange?.Invoke(this, EventArgs.Empty);
    }

    public List<ItemCarrito> GetItemsCarrito(int clienteId)
    {
        return itemsCarrito.Where(i => i.ClientId == clienteId).ToList();
    }

    public List<ItemCarrito> GetItemsCarritoAdmin()
    {
        return itemsCarrito.ToList();
    }


    public async Task<bool> GetItemsTarjeta(int clienteId)
    {
        using var context = _context.CreateDbContext();
        var cliente = await context.Clientes
            .Include(c => c.Tarjeta)
            .FirstOrDefaultAsync(c => c.Id == clienteId);

        if (cliente == null)
            throw new Exception("Cliente no encontrado");

        return cliente.TarjetaFicticiaId != null;
    }
    private decimal ObtenerPrecioFinal(Producto producto)
    {
        if (producto == null) return 0;

        var precioBase = producto.Precio ?? 0;
        var precioPromo = producto.PrecioPromocional ?? precioBase;

        return (precioPromo < precioBase && precioPromo > 0) ? precioPromo : precioBase;
    }


    public async Task<ReciboCompra> ProcesarCompraAsync(int clienteId, decimal totalPagar, string adreess, string pin)
    {
        using var context = _context.CreateDbContext();
        var items = GetItemsCarrito(clienteId);

        if (!items.Any())
            throw new Exception("El carrito está vacío");

        var total = items.Sum(i => ObtenerPrecioFinal(i.Producto) * i.Cantidad);

        var tarjeta = await _tarjetaService.ObtenerTarjetaVirtualAsync(clienteId);
        if (tarjeta == null)
            throw new Exception("Tarjeta no encontrada");

        if (!BCrypt.Net.BCrypt.Verify(pin, tarjeta.TarjPassword))
            throw new Exception("PIN incorrecto");

        if (tarjeta.Saldo < total)
            throw new Exception("Saldo insuficiente");

        tarjeta.Saldo -= total;

        // 🔹 Crear la entidad Compra
        var compra = new Compra
        {
            ClienteId = clienteId,
            Fecha = DateTime.Now,
            Total = total,
            Items = new List<ItemCarrito>(),
            Adreess = adreess
        };

        // 🔹 Asociar los items a la compra
        foreach (var item in items)
        {
            var precioFinal = ObtenerPrecioFinal(item.Producto);
            item.Compra = compra;
            item.PrecioUnitario = precioFinal;
            context.Entry(item.Producto).State = EntityState.Unchanged;
            compra.Items.Add(item);
            context.ItemsCarrito.Add(item);
        }

        // 🔹 Guardar la compra y actualizar la tarjeta
        context.Compra.Add(compra);

        // 🔹 Crear el recibo desde la compra
        var recibo = new ReciboCompra
        {
            ClienteId = clienteId,
            Fecha = compra.Fecha,
            Total = compra.Total,
            CodigoAutenticacion = Guid.NewGuid().ToString(),
            Items = compra.Items
        };

        // 🔹 Guardar el recibo
        context.ReciboCompra.Add(recibo);
        await context.SaveChangesAsync();

        // 🔹 Limpiar el carrito
        itemsCarrito.RemoveAll(i => i.ClientId == clienteId);

        return recibo;
    }

    public byte[] GenerarReciboPdf(ReciboCompra recibo)
    {
        var doc = new ReciboCompraDocument(recibo);
        return doc.GeneratePdf();
    }

}
