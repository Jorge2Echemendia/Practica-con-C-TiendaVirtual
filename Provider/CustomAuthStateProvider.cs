using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace TiendaVirtual.Provider;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private ClaimsPrincipal? _cachedUser;
    private bool _initialized = false;
    public event Action? OnAuthenticationStateChangedExternally;

    public CustomAuthStateProvider(IJSRuntime js)
    {
        _js = js;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {

            var rawToken = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");

            // Limpieza más segura del token
            string? token = rawToken?.Trim();
            if (token != null && token.StartsWith("\"") && token.EndsWith("\""))
            {
                token = token.Substring(1, token.Length - 2);
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
                Console.WriteLine("CustomAuthStateProvider: No hay token, usuario no autenticado");
                return new AuthenticationState(_cachedUser);
            }

            // Validación explícita del formato JWT
            if (!token.Contains('.'))
            {
                throw new ArgumentException("Token JWT mal formateado");
            }

            var handler = new JwtSecurityTokenHandler();

            try
            {
                var jwtToken = handler.ReadJwtToken(token);

                var claims = jwtToken.Claims.Select(c =>
                {
                    var type = c.Type switch
                    {
                        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" => ClaimTypes.Name,
                        "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" => ClaimTypes.Role,
                        "name" => ClaimTypes.Name,
                        "role" => ClaimTypes.Role,
                        _ => c.Type
                    };
                    return new Claim(type, c.Value);
                });

                var identity = new ClaimsIdentity(claims, "jwt");
                _cachedUser = new ClaimsPrincipal(identity);

                Console.WriteLine($"CustomAuthStateProvider: Usuario autenticado: {_cachedUser.Identity.Name}, Rol: {_cachedUser.FindFirst(ClaimTypes.Role)?.Value}");

                return new AuthenticationState(_cachedUser);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("prerendering"))
            {
                // Estamos en prerrenderizado, devolvemos un estado no autenticado.
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CustomAuthStateProvider: Error general: {ex.Message}");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    // Método para login - ACTUALIZADO
    public async Task NotifyUserAuthenticationAsync()
    {
        try
        {
            Console.WriteLine("CustomAuthStateProvider: NotifyUserAuthenticationAsync llamado");

            // Invalidar el cache actual
            _cachedUser = null;

            // Obtener el nuevo estado de autenticación
            var authState = await GetAuthenticationStateAsync();

            // Notificar a Blazor del cambio
            NotifyAuthenticationStateChanged(Task.FromResult(authState));

            // Notificar a componentes externos
            Console.WriteLine("CustomAuthStateProvider: Disparando OnAuthenticationStateChangedExternally");
            OnAuthenticationStateChangedExternally?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CustomAuthStateProvider: Error en NotifyUserAuthenticationAsync: {ex.Message}");
        }
    }

    // Método para logout
    public async Task NotifyUserLogoutAsync()
    {
        try
        {
            Console.WriteLine("CustomAuthStateProvider: NotifyUserLogoutAsync llamado");

            // Limpiar el token
            await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");

            // Invalidar el cache
            _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());

            // Notificar a Blazor del cambio
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cachedUser)));

            // Notificar a componentes externos
            Console.WriteLine("CustomAuthStateProvider: Disparando OnAuthenticationStateChangedExternally (logout)");
            OnAuthenticationStateChangedExternally?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CustomAuthStateProvider: Error en NotifyUserLogoutAsync: {ex.Message}");
        }
    }

    // Método para invalidar cache explícitamente
    public void InvalidateCache()
    {
        Console.WriteLine("CustomAuthStateProvider: Invalidando cache interno");
        _cachedUser = null;
    }

    // Mantén estos métodos auxiliares si los necesitas
    public void LeerClaimsDelToken(JwtSecurityToken token)
    {
        var nameClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var roleClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        var idClaim = token.Claims.FirstOrDefault(c => c.Type == "clienteId")?.Value;

        Console.WriteLine($"Name: {nameClaim}");
        Console.WriteLine($"Role: {roleClaim}");
        Console.WriteLine($"Id: {idClaim}");
    }

    private void PrintClaims(ClaimsPrincipal user)
    {
        Console.WriteLine("=== Claims del usuario ===");
        foreach (var claim in user.Claims)
        {
            Console.WriteLine($"Tipo: {claim.Type}");
            Console.WriteLine($"Valor: {claim.Value}");
            Console.WriteLine("--------------------------");
        }
    }
}