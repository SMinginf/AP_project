using System;
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
