using Blazored.LocalStorage;

namespace TiendaVirtual.Service
{
    public class AuthStateService
    {
        private readonly ILocalStorageService _localStorage;

        public AuthStateService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            return !string.IsNullOrEmpty(token);
        }
    }
}
