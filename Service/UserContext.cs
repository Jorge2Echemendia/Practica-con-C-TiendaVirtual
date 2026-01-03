using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TiendaVirtual.Context;
using TiendaVirtual.Provider;

public class UserContextService
{
    private readonly AuthenticationStateProvider _authProvider;
    private readonly IDbContextFactory<AppDbContext> _context;

    // Cache del estado del usuario
    private AuthenticationState? _cachedAuthState;
    private DateTime _cacheTime;
    private const int CACHE_DURATION_MINUTES = 5;

    public event Action? OnUserStateChanged;

    public UserContextService(AuthenticationStateProvider authProvider, IDbContextFactory<AppDbContext> context)
    {
        _authProvider = authProvider;
        _context = context;

        if (_authProvider is CustomAuthStateProvider customProvider)
        {
            customProvider.OnAuthenticationStateChangedExternally += () =>
            {
                // Invalidar cache cuando cambia la autenticación
                _cachedAuthState = null;
                NotifyStateChanged();
            };
        }
    }

    // Método principal para obtener el usuario con cache
    public async Task<ClaimsPrincipal> GetUserAsync(bool forceRefresh = false)
    {
        if (forceRefresh || _cachedAuthState == null ||
            DateTime.UtcNow > _cacheTime.AddMinutes(CACHE_DURATION_MINUTES))
        {
            _cachedAuthState = await _authProvider.GetAuthenticationStateAsync();
            _cacheTime = DateTime.UtcNow;
        }

        return _cachedAuthState.User;
    }

    // Método que obtiene todos los roles en una sola llamada
    public async Task<(bool isAdmin, bool isRepartidor, bool isAuthenticated, string? role)> GetUserRolesAsync()
    {
        var user = await GetUserAsync();

        if (!user.Identity.IsAuthenticated)
            return (false, false, false, null);

        string? role = user.FindFirst(ClaimTypes.Role)?.Value ??
                      user.FindFirst("role")?.Value ??
                      user.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

        return (
            isAdmin: role == "Administrador",
            isRepartidor: role == "Repartidor",
            isAuthenticated: role == "Cliente" || role == "Administrador" || role == "Repartidor",
            role: role
        );
    }

    // Métodos individuales que usan el cache
    public async Task<bool> IsAdminAsync()
    {
        var user = await GetUserAsync();
        return await CheckUserRoleAsync(user, "Administrador");
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var user = await GetUserAsync();
        return user.Identity?.IsAuthenticated == true;
    }

    public async Task<bool> IsReparAsync()
    {
        var user = await GetUserAsync();
        return await CheckUserRoleAsync(user, "Repartidor");
    }

    private async Task<bool> CheckUserRoleAsync(ClaimsPrincipal user, string roleName)
    {
        if (user.Identity?.IsAuthenticated != true)
            return false;

        string? role = user.FindFirst(ClaimTypes.Role)?.Value ??
                      user.FindFirst("role")?.Value ??
                      user.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

        return role == roleName;
    }

    // Método optimizado para NavMenu
    public async Task<UserMenuState> GetMenuStateAsync()
    {
        var user = await GetUserAsync();

        if (user.Identity?.IsAuthenticated != true)
            return new UserMenuState(false, false, false,false);

        string? role = user.FindFirst(ClaimTypes.Role)?.Value ??
                      user.FindFirst("role")?.Value ??
                      user.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

        return new UserMenuState(
            isAdmin: role == "Administrador",
            isRepartidor: role == "Repartidor",
            isClient:role=="Cliente",
            isAuthenticated: role == "Cliente" || role == "Administrador" || role == "Repartidor"
        );
    }

    public record UserMenuState(bool isAdmin, bool isRepartidor, bool isAuthenticated,bool isClient);
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

    public void InvalidateCache()
    {
        _cachedAuthState = null;
        Console.WriteLine("Cache invalidado en UserContextService");
    }

    public void NotifyStateChanged()
    {
        OnUserStateChanged?.Invoke();
    }
}
