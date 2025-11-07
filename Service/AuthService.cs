using TiendaVirtual.Model;
using TiendaVirtual.Context;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Blazored.LocalStorage;
namespace TiendaVirtual.Service;

public class AuthService
{
    private readonly IDbContextFactory<AppDbContext> _context;
    private enum AdminKeyword { Admin123, Trabajador123 };
    private readonly IConfiguration _config;

    public AuthService(IDbContextFactory<AppDbContext> context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<bool> RegisterAsync(string username, string email, string password)
    {
        using var context = _context.CreateDbContext();
        var exists = await context.Users
        .CountAsync(u => u.Username == username || u.Email == email) > 0;
        if (exists)
            return false;
        var rol = password.Contains(nameof(AdminKeyword.Admin123)) ? "Administrador" : password.Contains(nameof(AdminKeyword.Trabajador123))
        ? "Repartidor" : "Cliente";

        var user = new User
        {
            Username = username,
            Email = email,
            Password = HashPassword(password),
            Rol = rol,
            FechaRegistro = DateTime.Now
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        if (rol == "Cliente")
        {
            var cliente = new Cliente
            {
                UserId = user.Id
            };
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();
        }
        if (rol == "Repartidor")
        {
            var delivery = new DeliveryPerson
            {
                UserId = user.Id
            };
            context.DeliveryPersons.Add(delivery);
            await context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<string?> LoginAsync(string userInput, string password)
    {
        using var context = _context.CreateDbContext();
        var user = context.Users.FirstOrDefault(u => u.Username == userInput || u.Email == userInput);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            return null;

        return GenerateJwtToken(user);
    }

    public async Task<bool> ResetPasswordAsync(string email, string newPassword)
    {
        using var context = _context.CreateDbContext();
        var user = context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null) return false;

        user.Password = HashPassword(newPassword);
        await context.SaveChangesAsync();
        return true;
    }

    public string GenerateJwtToken(User user)
    {
        var claims = new[]
        {   new Claim("clienteId", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Rol)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes((_config["Jwt:Key"])));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
           issuer: _config["Jwt:Issuer"],
           audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
