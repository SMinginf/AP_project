using QuizClient.Utils;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace QuizClient.Services
{
    public class AuthService
    {
        private readonly HttpClient _client;

        public AuthService()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://localhost:8000"); // modifica se usi altra porta
        }

        public async Task<ServiceResult<string>> LoginAsync(string email, string password)
        {
            try
            {
                var data = new { email, password };
                var response = await _client.PostAsJsonAsync("/auth/login", data);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    var token = json?.AccessToken ?? string.Empty;
                    return new ServiceResult<string> { Data = token };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    string errorMsg = $"Errore HTTP {response.StatusCode}";

                    try
                    {
                        using var doc = JsonDocument.Parse(error);
                        if (doc.RootElement.TryGetProperty("error", out var errorProp))
                            errorMsg = errorProp.GetString() ?? errorMsg;
                    }
                    catch
                    {
                        // parsing fallito, si mantiene il messaggio generico
                    }

                    return new ServiceResult<string> { ErrorMessage = errorMsg };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<string> { ErrorMessage = $"Errore di rete: {ex.Message}" };
            }
            catch (JsonException ex)
            {
                return new ServiceResult<string> { ErrorMessage = $"Errore di deserializzazione: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult<string> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
            }
        }

        // i nomi dei campi del json inviato dal client al server Uvicorn assieme alla richiesta http
        // devono combaciare con quelli definiti tramite le classi Pydanitc, nel file schemas.py di auth_service
        public async Task<ServiceResult<string>> RegisterAsync(string ruolo, string username, string email, string nome, string cognome, string genere, string password, string data_nascita)
        {
            try
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
                    return new ServiceResult<string> { Data = $"Registrazione completata per l'utente: {username}" };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    string errorMsg = $"Errore HTTP {response.StatusCode}";

                    try
                    {
                        using var doc = JsonDocument.Parse(error);
                        if (doc.RootElement.TryGetProperty("error", out var errorProp))
                            errorMsg = errorProp.GetString() ?? errorMsg;
                    }
                    catch
                    {
                        // parsing fallito, si mantiene il messaggio generico
                    }

                    return new ServiceResult<string> { ErrorMessage = errorMsg };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<string> { ErrorMessage = $"Errore di rete: {ex.Message}" };
            }
            catch (JsonException ex)
            {
                return new ServiceResult<string> { ErrorMessage = $"Errore di deserializzazione: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult<string> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
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
