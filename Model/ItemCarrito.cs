
namespace TiendaVirtual.Model;

// ItemCarrito.cs
public class ItemCarrito
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public Cliente Cliente { get; set; }

    public int ProductoId { get; set; }
    public Producto Producto { get; set; }

    public int Cantidad { get; set; }

    public int CompraId { get; set; }
    public Compra Compra { get; set; }

    public decimal PrecioUnitario { get; set; }
}
