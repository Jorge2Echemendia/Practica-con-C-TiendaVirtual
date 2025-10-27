using TiendaVirtual.Context;
using TiendaVirtual.Model;

namespace TiendaVirtual.Service;

public class ItemService
{
    private readonly AppDbContext _context;
    private readonly ItemCarrito _itemCarrito;

    public ItemService(AppDbContext context, ItemCarrito itemCarrito)
    {
        _context = context;
        _itemCarrito = itemCarrito;
    }

    public async Task AgregarItemAsync(ItemCarrito itemCarrito)
    {
         _context.ItemsCarrito.Add(itemCarrito);
        await _context.SaveChangesAsync();
    }

}