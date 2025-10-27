using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;


namespace TiendaVirtual.Provider;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private ClaimsPrincipal _cachedUser;
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
            string token = rawToken?.Trim();
            if (token != null && token.StartsWith("\"") && token.EndsWith("\""))
            {
                token = token.Substring(1, token.Length - 2);
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Validación explícita del formato JWT
            if (!token.Contains('.'))
            {
                throw new ArgumentException("Token JWT mal formateado");
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            //LeerClaimsDelToken(jwtToken);

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
            //PrintClaims(_cachedUser);

            return new AuthenticationState(_cachedUser);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al procesar el token: {ex.Message}");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }
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

    public async Task RefreshAuthenticationStateAsync()
    {
        _initialized = false;
        var authState = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

    public async void NotifyUserAuthentication()
    {
        try
        {
            var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                var handler = new JwtSecurityTokenHandler();

                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    var identity = new ClaimsIdentity(jwtToken.Claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
                    var user = new ClaimsPrincipal(identity);

                    _cachedUser = user;
                    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
                    OnAuthenticationStateChangedExternally?.Invoke();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al notificar autentification: {ex.Message}");
        }
    }

    public void NotifyUserLogout()
    {
        _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cachedUser)));
        OnAuthenticationStateChangedExternally?.Invoke();
    }
}