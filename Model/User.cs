namespace TiendaVirtual.Model;

public class User
{
    public int Id { get; set; } // Clave primaria
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public string Rol { get; set; } = string.Empty;

    public Cliente? Cliente { get; set; }
    public string? Image { get; set; }
    public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();

}


