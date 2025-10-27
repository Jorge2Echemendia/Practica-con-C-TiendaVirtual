using System;

namespace TiendaVirtual.Model;

public class Producto
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public decimal? Precio { get; set; }
    public string? ImagenUrl { get; set; }
    public string? Descripcion { get; set; }

    public string? Categoria { get; set; }

    public int? CantidadProducto { get; set; }

    public decimal? PrecioPromocional { get; set; }

    public string? EnPromocion { get; set; } = "No";
    public DateTime? TiempoPromocion { get; set; }
    public ICollection<ItemCarrito> ItemsCarrito { get; set; } = new List<ItemCarrito>();

}