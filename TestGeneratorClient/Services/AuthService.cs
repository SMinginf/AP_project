using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace TestGeneratorClient.Services
{
    public class AuthService
    {
        private readonly HttpClient _client;

        public AuthService()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://localhost:8000"); // modifica se usi altra porta
        }

        public async Task<string?> LoginAsync(string email, string password)
        {
            var data = new { email, password };
            var response = await _client.PostAsJsonAsync("/auth/login", data);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return json?.AccessToken;
            }

            return null;
        }

        // i nomi dei campi del json inviato dal client al server Uvicorn assieme alla richiesta http
        // devono combaciare con quelli definiti tramite le classi Pydanitc, nel file schemas.py di auth_service
        public async Task<(bool Success, string Message)> RegisterAsync(string ruolo, string username, string email, string nome, string cognome, string genere, string password, string data_nascita)
        {
            var data = new
            {
                ruolo,
                username,
                email,
                nome,
                cognome,
                genere,
                password,
                data_nascita
            };
            var response = await _client.PostAsJsonAsync("/auth/register", data);
            if (response.IsSuccessStatusCode)
            {
                return (true, "Registrazione completata per: " + username);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return (false, "Errore: " + error);
            }
        }
    }

    public class LoginResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = "";

        [System.Text.Json.Serialization.JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "";
    }
}
