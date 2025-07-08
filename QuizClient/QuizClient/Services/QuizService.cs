using QuizClient.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using QuizClient.Utils;

namespace QuizClient.Services
{
    public class QuizService
    {
        private readonly HttpClient _client;
        private readonly string _jwtToken;

        public QuizService(string jwtToken)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://localhost:8081"); // modifica se usi altra porta
            _jwtToken = jwtToken;

            // Imposta l'header di autorizzazione delle richieste http che verranno inviate da questo client con il token JWT
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        }

        // Crea quiz
        public async Task<ServiceResult<Quiz>> CreateQuizAsync(bool AIg, string AIcat, List<int> id_cat, bool u, string diff, int nd)
        {
            var quiz_data = new
            {
                ai_generated = AIg,
                ai_categoria = AIcat,
                id_categorie = id_cat,
                unione = u,
                difficolta = diff,
                quantita = nd,
            };
            try
            {
                var response = await _client.PostAsJsonAsync("/quiz/create", quiz_data);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<Quiz>();
                    return new ServiceResult<Quiz> { Data = data };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    string? errorMsg = $"Errore HTTP {response.StatusCode}";
                    try
                    {
                        using var doc = JsonDocument.Parse(error);
                        if (doc.RootElement.TryGetProperty("error", out var errorProp))
                            errorMsg = errorProp.GetString() ?? errorMsg;
                    }
                    catch { }
                    return new ServiceResult<Quiz> { ErrorMessage = errorMsg };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<Quiz> { ErrorMessage = $"Errore di rete: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult<Quiz> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
            }
        }

        /*
        public async Task<Session[]?> GetSessionsAsync() { 
            var response = await _client.GetAsync("/sessions/list");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Session[]>() ?? Array.Empty<Session>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Errore nel recupero delle sessioni: {error}");
                return null;
            }
        }

        public async Task<Session?> GetSessionAsync(string sessionId)
        {
            var response = await _client.GetAsync($"/sessions/{sessionId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Session>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Errore nel recupero della sessione: {error}");
                return null;
            }
        }

        public async Task<Session?> JoinSessionAsync(string sessionId)
        {
            int? userId = JwtUtils.GetClaimAsInt(_jwtToken, "user_id");
            string? username = JwtUtils.GetClaim(_jwtToken, "username");
            var join_data = new
            {
                session_id = sessionId,
                player_id = userId,
                player_username = username
            };
            var response = await _client.PostAsJsonAsync($"/sessions/join", join_data);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Session>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Errore nell'unirsi alla sessione: {error}");
                return null;
            }

        }

        public async Task<bool> LeaveSessionAsync(string sessionId)
        {
            int? userId = JwtUtils.GetClaimAsInt(_jwtToken, "user_id");
            var leave_data = new
            {
                session_id = sessionId,
                player_id = userId
            };
            var response = await _client.PostAsJsonAsync($"/sessions/leave", leave_data);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Errore nell'abbandonare la sessione: {error}");
                return false;
            }
        }
        
        */
    }
 
}