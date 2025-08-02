using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using QuizClient.Models;
using QuizClient.Utils;

namespace QuizClient.Services
{
    internal class CRUDService
    {
        private readonly HttpClient _client;
        private readonly string _jwtToken;

        public CRUDService(string jwtToken)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://localhost:8082"); // Modifica se usi un'altra porta
            _jwtToken = jwtToken;
            // Imposta l'header di autorizzazione delle richieste http che verranno inviate da questo client con il token JWT
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
        }


        public async Task<ServiceResult<List<Categoria>>> GetCategoriePubblicheAsync()
        {
            try
            {
                var response = await _client.GetAsync("/categorie/pubbliche");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<Categoria>>() ?? new List<Categoria>();
                    return new ServiceResult<List<Categoria>> { Data = data };
                }
                else
                {
                    // Legge il contenuto della risposta HTTP come stringa (di solito JSON con dettagli dell'errore)
                    var error = await response.Content.ReadAsStringAsync();
                    // Prepara un messaggio di errore generico con il codice di stato HTTP
                    string? errorMsg = $"Errore HTTP {response.StatusCode}";
                    try
                    {
                        // Prova a interpretare la stringa come JSON
                        using var doc = JsonDocument.Parse(error);
                        // Se il JSON contiene una proprietà "error", usa il suo valore come messaggio di errore
                        if (doc.RootElement.TryGetProperty("error", out var errorProp))
                            errorMsg = errorProp.GetString() ?? errorMsg;
                    }
                    catch
                    {
                        // Se il parsing fallisce, mantiene il messaggio di errore generico
                    }
                    return new ServiceResult<List<Categoria>> { ErrorMessage = errorMsg };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<List<Categoria>> { ErrorMessage = $"Errore di rete: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<Categoria>> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
            }
        }

        public async Task<ServiceResult<List<Categoria>>> GetCategorieByStudenteAsync()
        {
            try 
            { 
                var id_studente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
                var response = await _client.GetAsync($"/categorie/studente/{id_studente}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<Categoria>>() ?? new List<Categoria>();
                    return new ServiceResult<List<Categoria>> { Data = data };
                }
                else
                {
                    // Legge il contenuto della risposta HTTP come stringa (di solito JSON con dettagli dell'errore)
                    var error = await response.Content.ReadAsStringAsync();
                    // Prepara un messaggio di errore generico con il codice di stato HTTP
                    string? errorMsg = $"Errore HTTP {response.StatusCode}";
                    try
                    {
                        // Prova a interpretare la stringa come JSON
                        using var doc = JsonDocument.Parse(error);
                        // Se il JSON contiene una proprietà "error", usa il suo valore come messaggio di errore
                        if (doc.RootElement.TryGetProperty("error", out var errorProp))
                            errorMsg = errorProp.GetString() ?? errorMsg;
                    }
                    catch
                    {
                        // Se il parsing fallisce, mantiene il messaggio di errore generico
                    }
                    return new ServiceResult<List<Categoria>> { ErrorMessage = errorMsg };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<List<Categoria>> { ErrorMessage = $"Errore di rete: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<Categoria>> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
            }
        }



        public async Task<ServiceResult<List<Categoria>>> GetCategoriePubblicheByDocenteAsync() { 
            
            try
            {
                var id_docente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
                var response = await _client.GetAsync($"/categorie/pubbliche/docente/{id_docente}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<Categoria>>() ?? new List<Categoria>();
                    return new ServiceResult<List<Categoria>> { Data = data };
                }
                else
                {
                    // Legge il contenuto della risposta HTTP come stringa (di solito JSON con dettagli dell'errore)
                    var error = await response.Content.ReadAsStringAsync();
                    // Prepara un messaggio di errore generico con il codice di stato HTTP
                    string? errorMsg = $"Errore HTTP {response.StatusCode}";
                    try
                    {
                        // Prova a interpretare la stringa come JSON
                        using var doc = JsonDocument.Parse(error);
                        // Se il JSON contiene una proprietà "error", usa il suo valore come messaggio di errore
                        if (doc.RootElement.TryGetProperty("error", out var errorProp))
                            errorMsg = errorProp.GetString() ?? errorMsg;
                    }
                    catch
                    {
                        // Se il parsing fallisce, mantiene il messaggio di errore generico
                    }
                    return new ServiceResult<List<Categoria>> { ErrorMessage = errorMsg };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<List<Categoria>> { ErrorMessage = $"Errore di rete: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<Categoria>> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
            }
        }
        public async Task<ServiceResult<List<Categoria>>> GetCategorieByDocenteAsync()
        {
            try
            {

                var id_docente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");

                var response = await _client.GetAsync($"/categorie/docente/{id_docente}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<Categoria>>() ?? new List<Categoria>();
                    return new ServiceResult<List<Categoria>> { Data = data };
                }
                else
                {
                    // Legge il contenuto della risposta HTTP come stringa (di solito JSON con dettagli dell'errore)
                    var error = await response.Content.ReadAsStringAsync();

                    // Prepara un messaggio di errore generico con il codice di stato HTTP
                    string? errorMsg = $"Errore HTTP {response.StatusCode}";
                    try
                    {
                        // Prova a interpretare la stringa come JSON
                        using var doc = JsonDocument.Parse(error);

                        // Se il JSON contiene una proprietà "error", usa il suo valore come messaggio di errore
                        if (doc.RootElement.TryGetProperty("error", out var errorProp))
                            errorMsg = errorProp.GetString() ?? errorMsg;
                    }
                    catch
                    {
                        // Se il parsing fallisce, mantiene il messaggio di errore generico
                    }
                    return new ServiceResult<List<Categoria>> { ErrorMessage = errorMsg };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<List<Categoria>> { ErrorMessage = $"Errore di rete: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<Categoria>> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
            }
        }

        public async Task<ServiceResult<Categoria>> CreateCategoriaAsync(Categoria categoria)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("/categorie/create", categoria);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<Categoria>();
                    return new ServiceResult<Categoria> { Data = data };
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
                    catch
                    {
                    }
                    return new ServiceResult<Categoria> { ErrorMessage = errorMsg };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<Categoria> { ErrorMessage = $"Errore di rete: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult<Categoria> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
            }
        }

        public async Task<ServiceResult<Categoria>> UpdateCategoriaAsync(Categoria categoria)
        {
            try
            {
                var response = await _client.PutAsJsonAsync($"/categorie/update", categoria);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<Categoria>();
                    return new ServiceResult<Categoria> { Data = data };
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
                    catch
                    {
                    }
                    return new ServiceResult<Categoria> { ErrorMessage = errorMsg };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<Categoria> { ErrorMessage = $"Errore di rete: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult<Categoria> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
            }
        }



        public async Task<ServiceResult<bool>> DeleteCategoriaAsync(List<uint> ids)
        {
            try
            {
                // Non esiste un metodo DeleteAsJsonAsync, quindi si usa HttpRequestMessage per inviare una richiesta DELETE con il corpo JSON
                var request = new HttpRequestMessage(HttpMethod.Delete, "/categorie/delete")
                {
                    Content = JsonContent.Create(ids) // ids è la lista di ID da inviare
                };
                var response = await _client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return new ServiceResult<bool> { Data = true };
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
                    catch
                    {
                    }
                    return new ServiceResult<bool> { ErrorMessage = errorMsg };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<bool> { ErrorMessage = $"Errore di rete: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
            }
        }

        public async Task<ServiceResult<Utente>> GetUserAsync() { 
                try
                {
                    var id_utente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
                    var response = await _client.GetAsync($"/utente/{id_utente}");
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadFromJsonAsync<Utente>();
                        return new ServiceResult<Utente> { Data = data };
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
                        catch
                        {
                        }
                        return new ServiceResult<Utente> { ErrorMessage = errorMsg };
                    }
                }
                catch (HttpRequestException ex)
                {
                    return new ServiceResult<Utente> { ErrorMessage = $"Errore di rete: {ex.Message}" };
                }
                catch (Exception ex)
                {
                    return new ServiceResult<Utente> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
            }
        }
    
    
    
    
    }
}
