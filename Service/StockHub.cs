using Microsoft.AspNetCore.SignalR;
namespace TiendaVirtual;

public class StockHub : Hub
{
    public async Task NotificarCambioStock(int productoId, int nuevaCantidad)
    {
        await Clients.All.SendAsync("ActualizarStock", productoId, nuevaCantidad);
    }
}