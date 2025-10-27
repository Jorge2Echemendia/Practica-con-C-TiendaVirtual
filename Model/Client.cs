namespace TiendaVirtual.Model;

public class Cliente
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }

    public List<ItemCarrito> Carrito { get; set; } = new();
    public int? TarjetaFicticiaId { get; set; }
    public TarjetaFicticia? Tarjeta { get; set; }
    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
    public ICollection<ReciboCompra> ReciboCompra { get; set; } = new List<ReciboCompra>();
}
