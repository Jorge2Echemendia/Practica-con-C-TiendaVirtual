using TiendaVirtual.Model;

namespace TiendaVirtual.Model;

public class Compra
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }

    public double? Lon { get; set; }

    public double? Lat { get; set; }

    public string? Check { get; set; }
    public List<ItemCarrito> Items { get; set; } = new();

    public int? DeliveryPersonId { get; set; }
    public DeliveryPerson? DeliveryPerson { get; set; }
    public string Coordenadas => $"{Lat}, {Lon}";
}
