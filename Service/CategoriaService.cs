// CategoriaService.cs
using TiendaVirtual.Model;
using TiendaVirtual.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Forms;

namespace TiendaVirtual.Service;

public class CategoriaService
{
    private readonly IDbContextFactory<AppDbContext> _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CategoriaService(IDbContextFactory<AppDbContext> context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<List<Categoria>> ObtenerTodasCategoriasAsync(bool incluirProductos = false)
    {
        using var context = _context.CreateDbContext();
        
        if (incluirProductos)
        {
            return await context.Categorias
                .Include(c => c.Productos.Where(p => p.CantidadProducto > 0))
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }
        
        return await context.Categorias
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<Categoria?> ObtenerCategoriaPorIdAsync(int id, bool incluirProductos = false)
    {
        using var context = _context.CreateDbContext();
        
        if (incluirProductos)
        {
            return await context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
        }
        
        return await context.Categorias
            .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
    }

    public async Task<Categoria> CrearCategoriaAsync(Categoria categoria)
    {
        using var context = _context.CreateDbContext();
        
        // Verificar si ya existe una categoría con el mismo nombre
        var existe = await context.Categorias
            .AnyAsync(c => c.Nombre.ToLower() == categoria.Nombre.ToLower());
            
        if (existe)
        {
            throw new InvalidOperationException($"Ya existe una categoría con el nombre '{categoria.Nombre}'");
        }
        
        context.Categorias.Add(categoria);
        await context.SaveChangesAsync();
        
        return categoria;
    }

    public async Task<Categoria> ActualizarCategoriaAsync(Categoria categoria)
    {
        using var context = _context.CreateDbContext();
        
        var categoriaExistente = await context.Categorias
            .FirstOrDefaultAsync(c => c.Id == categoria.Id && c.Activo);
            
        if (categoriaExistente == null)
        {
            throw new KeyNotFoundException($"Categoría con ID {categoria.Id} no encontrada");
        }
        
        // Verificar si el nombre ya existe (excluyendo la categoría actual)
        var existeNombre = await context.Categorias
            .AnyAsync(c => c.Id != categoria.Id && 
                          c.Nombre.ToLower() == categoria.Nombre.ToLower());
            
        if (existeNombre)
        {
            throw new InvalidOperationException($"Ya existe otra categoría con el nombre '{categoria.Nombre}'");
        }
        
        // Actualizar propiedades
        categoriaExistente.Nombre = categoria.Nombre;
        categoriaExistente.Descripcion = categoria.Descripcion;
        categoriaExistente.ImagenUrl = categoria.ImagenUrl;
        
        await context.SaveChangesAsync();
        
        return categoriaExistente;
    }

    public async Task<bool> EliminarCategoriaAsync(int id)
    {
        using var context = _context.CreateDbContext();
        
        var categoria = await context.Categorias
            .Include(c => c.Productos)
            .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
            
        if (categoria == null)
        {
            return false;
        }
        
        // Verificar si hay productos asociados
        if (categoria.Productos.Any())
        {
            // Opción 1: No permitir eliminar si hay productos
            throw new InvalidOperationException($"No se puede eliminar la categoría '{categoria.Nombre}' porque tiene productos asociados");
            
            // Opción 2: Desactivar en lugar de eliminar (soft delete)
            categoria.Activo = false;
            await context.SaveChangesAsync();
            return true;
        }
        
        // Si no hay productos, eliminar físicamente
        context.Categorias.Remove(categoria);
        await context.SaveChangesAsync();
        
        return true;
    }

    public async Task<string> GuardarImagenAsync(IBrowserFile archivo)
    {
        if (archivo == null)
        {
            return "/img/categorias/default.jpg";
        }
        
        // Validar tipo de archivo
        var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(archivo.Name).ToLower();
        
        if (!extensionesPermitidas.Contains(extension))
        {
            throw new ArgumentException("Formato de imagen no válido. Use JPG, PNG, GIF o WebP");
        }
        
        // Validar tamaño (máximo 5MB)
        if (archivo.Size > 5 * 1024 * 1024)
        {
            throw new ArgumentException("La imagen no puede superar los 5MB");
        }
        
        // Crear directorio si no existe
        var carpetaImagenes = Path.Combine(_webHostEnvironment.WebRootPath, "img", "categorias");
        if (!Directory.Exists(carpetaImagenes))
        {
            Directory.CreateDirectory(carpetaImagenes);
        }
        
        // Generar nombre único
        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        var rutaArchivo = Path.Combine(carpetaImagenes, nombreArchivo);
        
        // Guardar archivo
        await using var stream = new FileStream(rutaArchivo, FileMode.Create);
        await archivo.OpenReadStream(5 * 1024 * 1024).CopyToAsync(stream);
        
        return $"/img/categorias/{nombreArchivo}";
    }
}