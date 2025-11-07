using TiendaVirtual.Model;

namespace TiendaVirtual.Model;

public class Notificacion
{
    public int Id { get; set; }
    public int UserId { get; set; } // Usuario que recibirá la notificación
    public string ? Mensaje { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.Now;
    public string? Leido { get; set; } = "No";

    public User User { get; set; } = null!;
}
