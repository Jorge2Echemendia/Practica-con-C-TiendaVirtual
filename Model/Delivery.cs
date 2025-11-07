namespace TiendaVirtual.Model;

public class DeliveryPerson
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }

    public string? Vehiculo { get; set; } // Ej: moto, bicicleta, auto
    public string? Estado { get; set; } = "Disponible"; // Disponible, Ocupado, Sinservicio

    public ICollection<Compra> Entregas { get; set; } = new List<Compra>();
}
