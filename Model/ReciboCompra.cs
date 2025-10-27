using TiendaVirtual.Model;

namespace TiendaVirtual.Model;

public class ReciboCompra
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public List<ItemCarrito> Items { get; set; } = new();
    public decimal Total { get; set; }
    public string CodigoAutenticacion { get; set; } = Guid.NewGuid().ToString();
}
