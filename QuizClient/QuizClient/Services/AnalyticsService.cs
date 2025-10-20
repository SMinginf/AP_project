/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using QuizClient.Models;
using QuizClient.Utils;
using System.Net.Http.Json;

namespace QuizClient.Services
{
    public class AnalyticsService
    {
        private readonly HttpClient _client;
        private readonly string _jwtToken;

        public AnalyticsService(string jwtToken)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://localhost:8005"); // modifica se usi altra porta
            _jwtToken = jwtToken;
            // Imposta l'header di autorizzazione delle richieste http che verranno inviate da questo client con il token JWT
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
        }


        public async Task<ServiceResult<TeacherGeneralStats>> GetTeacherGeneralStatsAsync()
        { 
            try
            {
                var id_docente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
                var response = await _client.GetAsync($"/stats/teacher/{id_docente}/general");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<TeacherGeneralStats>();
                    return new ServiceResult<TeacherGeneralStats> { Data = data };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    string? errorMsg = $"Errore HTTP {response.StatusCode}";
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(error);
                        if (doc.RootElement.TryGetProperty("detail", out var errorProp))
                            errorMsg = errorProp.GetString() ?? errorMsg;
                    }
                    catch { }
                    return new ServiceResult<TeacherGeneralStats> { ErrorMessage = errorMsg };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<TeacherGeneralStats> { ErrorMessage = $"Errore di rete: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult<TeacherGeneralStats> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
            }

        }


        public async Task<ServiceResult<TeacherCategoryStatsResponse>> GetTeacherStatsPerCategoryAsync(List<uint> categoria_ids) {

            try
            {
                var id_docente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
                // Unisci gli ID in una stringa separata da virgole
                var idsQuery = string.Join(",", categoria_ids);
                var url = $"/stats/teacher/{id_docente}?categoria_ids={idsQuery}";
                var response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<TeacherCategoryStatsResponse>();
                    return new ServiceResult<TeacherCategoryStatsResponse> { Data = data };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    string? errorMsg = $"Errore HTTP {response.StatusCode}";
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(error);
                        if (doc.RootElement.TryGetProperty("detail", out var errorProp))
                            errorMsg = errorProp.GetString() ?? errorMsg;
                    }
                    catch { }
                    return new ServiceResult<TeacherCategoryStatsResponse> { ErrorMessage = errorMsg };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<TeacherCategoryStatsResponse> { ErrorMessage = $"Errore di rete: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult<TeacherCategoryStatsResponse> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
            }


        }
        
        public async Task<ServiceResult<StudentGeneralStats>> GetStudentGeneralStatsAsync()
        {
            try
            {
                var id_studente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
                var response = await _client.GetAsync($"/stats/student/{id_studente}/general");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<StudentGeneralStats>();
                    return new ServiceResult<StudentGeneralStats> { Data = data };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    string? errorMsg = $"Errore HTTP {response.StatusCode}";
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(error);
                        if (doc.RootElement.TryGetProperty("detail", out var errorProp))
                            errorMsg = errorProp.GetString() ?? errorMsg;
                    }
                    catch { }
                    return new ServiceResult<StudentGeneralStats> { ErrorMessage = errorMsg };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<StudentGeneralStats> { ErrorMessage = $"Errore di rete: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult<StudentGeneralStats> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
            }
        }
        public async Task<ServiceResult<StudentStatsResponse>> GetStudentStatsPerCategoryAsync(List<uint> categoria_ids)
        {
            try
            {
                var id_studente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
                // Unisci gli ID in una stringa separata da virgole
                var idsQuery = string.Join(",", categoria_ids);
                var url = $"/stats/student/{id_studente}?categoria_ids={idsQuery}";
                var response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<StudentStatsResponse>();
                    return new ServiceResult<StudentStatsResponse> { Data = data};
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    string? errorMsg = $"Errore HTTP {response.StatusCode}";
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(error);
                        if (doc.RootElement.TryGetProperty("detail", out var errorProp))
                            errorMsg = errorProp.GetString() ?? errorMsg;
                    }
                    catch { }
                    return new ServiceResult<StudentStatsResponse> { ErrorMessage = errorMsg };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<StudentStatsResponse> { ErrorMessage = $"Errore di rete: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult<StudentStatsResponse> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
            }
        }
    }
}
*/

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using QuizClient.Models;
using QuizClient.Utils;

namespace QuizClient.Services
{
    /// <summary>
    /// Servizio per ottenere statistiche dal microservizio Analytics.
    /// Tutte le richieste HTTP includono il token JWT dell'utente autenticato.
    /// </summary>
    public class AnalyticsService: IDisposable
    {
        private readonly HttpClient _client;
        private readonly string _jwtToken;
        private bool _disposed;

        public AnalyticsService(string jwtToken)
        {
            _jwtToken = jwtToken;

            _client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:8005") // Modifica se usi un'altra porta
            };

            // Imposta l'header di autorizzazione per tutte le richieste HTTP inviate da questo client
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
        }

        // --- METODI PER IL DOCENTE ---

        public async Task<ServiceResult<TeacherGeneralStats>> GetTeacherGeneralStatsAsync()
            => await GetAsync<TeacherGeneralStats>($"/stats/teacher/{JwtUtils.GetClaimAsUInt(_jwtToken, "user_id")}/general");

        public async Task<ServiceResult<TeacherCategoryStatsResponse>> GetTeacherStatsPerCategoryAsync(List<uint> categoriaIds)
        {
            var idDocente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
            var idsQuery = string.Join(",", categoriaIds);
            return await GetAsync<TeacherCategoryStatsResponse>($"/stats/teacher/{idDocente}?categoria_ids={idsQuery}");
        }

        // --- METODI PER LO STUDENTE ---

        public async Task<ServiceResult<StudentGeneralStats>> GetStudentGeneralStatsAsync()
            => await GetAsync<StudentGeneralStats>($"/stats/student/{JwtUtils.GetClaimAsUInt(_jwtToken, "user_id")}/general");

        public async Task<ServiceResult<StudentStatsResponse>> GetStudentStatsPerCategoryAsync(List<uint> categoriaIds)
        {
            var idStudente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
            var idsQuery = string.Join(",", categoriaIds);
            return await GetAsync<StudentStatsResponse>($"/stats/student/{idStudente}?categoria_ids={idsQuery}");
        }

        // --- METODO AUSILIARIO GENERICO PER LE RICHIESTE GET ---
        private async Task<ServiceResult<T>> GetAsync<T>(string url)
        {
            try
            {
                var response = await _client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<T>();
                    return new ServiceResult<T> { Data = data };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMsg = $"Errore HTTP {response.StatusCode}";

                try
                {
                    using var doc = JsonDocument.Parse(errorContent);
                    if (doc.RootElement.TryGetProperty("detail", out var errorProp))
                        errorMsg = errorProp.GetString() ?? errorMsg;
                }
                catch
                {
                    // parsing fallito, si mantiene il messaggio generico
                }

                return new ServiceResult<T> { ErrorMessage = errorMsg };
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<T> { ErrorMessage = $"Errore di rete: {ex.Message}" };
            }
            catch (JsonException ex)
            {
                return new ServiceResult<T> { ErrorMessage = $"Errore di deserializzazione: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult<T> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
            }
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
}
