using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TestGeneratorClient.Models;
using TestGeneratorClient.Utils;
using System.Net.Http.Json;

namespace TestGeneratorClient.Services
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

        public async Task<ServiceResult<StudentStatsResponse>> GetStudentStatsAsync()
        {
            try
            {
                var id_studente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
                var response = await _client.GetAsync($"/stats/student/{id_studente}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<StudentStatsResponse>();
                    return new ServiceResult<StudentStatsResponse> { Data = data };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    string? errorMsg = $"Errore HTTP {response.StatusCode}";
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(error);
                        if (doc.RootElement.TryGetProperty("error", out var errorProp))
                            errorMsg = errorProp.GetString() ?? errorMsg;
                    }
                    catch { }
                    return new ServiceResult<StudentStatsResponse> { ErrorMessage = errorMsg };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<StudentStatsResponse> { ErrorMessage = $"Errore di rete: {ex.Message}"};
            }
            catch (Exception ex)
            {
                return new ServiceResult<StudentStatsResponse> { ErrorMessage = $"Errore imprevisto: {ex.Message}"};
            }
        }
    }
}
