using System.ComponentModel.DataAnnotations;
using TiendaVirtual.Model;

namespace TiendaVirtual.Model;

public class Categoria
{
    public int Id { get; set; }
    [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }

    public string? ImagenUrl { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public bool Activo { get; set; } = true;

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}