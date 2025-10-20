/*
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
*/
using QuizClient.Utils;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QuizClient.Services
{
    /// <summary>
    /// Servizio per l'autenticazione e la registrazione degli utenti.
    /// Gestisce la comunicazione HTTP con il microservizio auth_service.
    /// </summary>
    public class AuthService : IDisposable
    {
        private readonly HttpClient _client;
        private bool _disposed;

        public AuthService()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:8000") // Modifica se usi un'altra porta
            };
        }

        /// <summary>
        /// Esegue il login dell’utente e restituisce il token JWT in caso di successo.
        /// </summary>
        public async Task<ServiceResult<string>> LoginAsync(string email, string password)
        {
            try
            {
                var payload = new { email, password };
                var response = await _client.PostAsJsonAsync("/auth/login", payload);

                if (!response.IsSuccessStatusCode)
                    return await HandleErrorAsync(response);

                var json = await response.Content.ReadFromJsonAsync<LoginResponse>();
                var token = json?.AccessToken;

                return new ServiceResult<string> { Data = token };
            }
            catch (HttpRequestException ex)
            {
                return ServiceResult<string>.FromError($"Errore di rete: {ex.Message}");
            }
            catch (JsonException ex)
            {
                return ServiceResult<string>.FromError($"Errore di deserializzazione: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.FromError($"Errore imprevisto: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra un nuovo utente.  
        /// I nomi dei campi del JSON inviato devono combaciare con quelli
        /// definiti nelle classi Pydantic (file schemas.py di auth_service).
        /// </summary>
        public async Task<ServiceResult<string>> RegisterAsync(
            string ruolo,
            string username,
            string email,
            string nome,
            string cognome,
            string genere,
            string password,
            string data_nascita)
        {
            try
            {
                var payload = new
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

                var response = await _client.PostAsJsonAsync("/auth/register", payload);

                if (!response.IsSuccessStatusCode)
                    return await HandleErrorAsync(response);

                return new ServiceResult<string>
                {
                    Data = $"Registrazione completata per l'utente: {username}"
                };
            }
            catch (HttpRequestException ex)
            {
                return ServiceResult<string>.FromError($"Errore di rete: {ex.Message}");
            }
            catch (JsonException ex)
            {
                return ServiceResult<string>.FromError($"Errore di deserializzazione: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.FromError($"Errore imprevisto: {ex.Message}");
            }
        }

        /// <summary>
        /// Gestisce e interpreta un errore HTTP restituendo un messaggio leggibile.
        /// </summary>
        private static async Task<ServiceResult<string>> HandleErrorAsync(HttpResponseMessage response)
        {
            var errorMsg = $"Errore HTTP {response.StatusCode}";
            try
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(errorJson);

                if (doc.RootElement.TryGetProperty("error", out JsonElement errorProp))
                    errorMsg = errorProp.GetString() ?? errorMsg;
            }
            catch
            {
                // Parsing fallito, mantieni messaggio generico
            }

            return ServiceResult<string>.FromError(errorMsg);
        }

        public void Dispose()
        {
            // Best practice: controllo con flag per garantire idempotenza
            if (_disposed)
                return;
            _client.Dispose();
            _disposed = true;

            // Sopprime la finalizzazione se non è necessaria (buona pratica):
            // permette di saltare l'esecuzione del finalizzatore, poichè le risorse critiche sono già
            // state rilasciate, e pulire la sua memoria nel modo più veloce possibile.
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Rappresenta la risposta JSON ricevuta al login.
    /// </summary>
    public class LoginResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;
    }
}
