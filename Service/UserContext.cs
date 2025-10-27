using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TiendaVirtual.Context;
using TiendaVirtual.Provider;

public class UserContextService
{
    private readonly AuthenticationStateProvider _authProvider;
    private readonly CustomAuthStateProvider _customProvider;
    private ClaimsPrincipal _user = new ClaimsPrincipal(new ClaimsIdentity());

    public event Action? OnUserStateChanged;

    private readonly IDbContextFactory<AppDbContext> _context;

    public UserContextService(AuthenticationStateProvider authProvider, IDbContextFactory<AppDbContext> context)
    {
        _authProvider = authProvider;
        _context = context;

        if (_authProvider is CustomAuthStateProvider customProvider)
        {
            customProvider.OnAuthenticationStateChangedExternally += () =>
            {
                NotifyStateChanged();
            };
        }
    }

    public async Task<ClaimsPrincipal> GetUserAsync()
    {
        var authState = await _authProvider.GetAuthenticationStateAsync();
        _user = authState.User;
        return authState.User;
    }

    public async Task<bool> IsAdminAsync()
    {
        var user = await GetUserAsync();

        var role = user.FindFirst(ClaimTypes.Role)?.Value ?? user.FindFirst("role")?.Value
        ?? user.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
        Console.WriteLine($"role:{role}");
        return user.Identity?.IsAuthenticated == true && role == "Administrador";
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var user = await GetUserAsync();
        return user.Identity?.IsAuthenticated == true;
    }
    public async Task<string?> GetUsernameAsync()
    {
        var user = await GetUserAsync();
        if (user.Identity?.IsAuthenticated == true)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value
                ?? user.FindFirst("name")?.Value;
        }
        return null;
    }
    public async Task<int> GetClienteIdAsync()
    {
        try
        {
            var user = await GetUserAsync();
            var claim = user.FindFirst("clienteId");

            if (claim == null)
                throw new Exception("El claim 'clienteId' no está presente.");

            Console.WriteLine($"Valor del claim 'clienteId': {claim.Value}");

            if (!int.TryParse(claim.Value, out int userId))
                throw new Exception($"El claim 'clienteId' no es un número válido: '{claim.Value}'");

            using var context = _context.CreateDbContext();
            var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cliente == null)
                throw new Exception($"No se encontró un cliente con UserId = {userId}");

            Console.WriteLine($"Cliente encontrado: ID = {cliente.Id}");

            return cliente.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener id de cliente: {ex.Message}");
            throw;
        }
    }



    public void NotifyStateChanged()
    {
        OnUserStateChanged?.Invoke();
    }
}
