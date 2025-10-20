/*
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

        // Servizi CRUD per le categorie
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
        // Aggiunto SQ
        public async Task<ServiceResult<List<Categoria>>> GetCategorieByDocenteAndNomeAsync(string nome)
        {
            try
            {
                var id_docente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");

                var response = await _client.GetAsync($"/categorie/docente/{id_docente}?categoria={nome}");
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



        // Aggiunto SQ
        // Servizi CRUD per i quesiti 
        public async Task<ServiceResult<List<Quesito>>> GetQuesitiByDocenteAsync()
        {
            try
            {
                var id_docente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
                var response = await _client.GetAsync($"/quesiti/docente/{id_docente}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<Quesito>>() ?? new List<Quesito>();
                    return new ServiceResult<List<Quesito>> { Data = data };
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
                    return new ServiceResult<List<Quesito>> { ErrorMessage = errorMsg };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<List<Quesito>> { ErrorMessage = $"Errore di rete: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<Quesito>> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
            }
        }
        public async Task<ServiceResult<Quesito>> UpdateQuesitoAsync(Quesito quesito)
        {
            try
            {
                var response = await _client.PutAsJsonAsync($"/quesiti/update", quesito);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<Quesito>();
                    return new ServiceResult<Quesito> { Data = data };
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
                    return new ServiceResult<Quesito> { ErrorMessage = errorMsg };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<Quesito> { ErrorMessage = $"Errore di rete: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult<Quesito> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
            }
        }
        public async Task<ServiceResult<Quesito>> CreateQuesitoAsync(Quesito quesito)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("/quesiti/create", quesito);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<Quesito>();
                    return new ServiceResult<Quesito> { Data = data };
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
                    return new ServiceResult<Quesito> { ErrorMessage = errorMsg };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<Quesito> { ErrorMessage = $"Errore di rete: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult<Quesito> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
            }
        }
        public async Task<ServiceResult<bool>> DeleteQuesitoAsync(List<uint> ids)
        {
            try
            {
                // Non esiste un metodo DeleteAsJsonAsync, quindi si usa HttpRequestMessage per inviare una richiesta DELETE con il corpo JSON
                var request = new HttpRequestMessage(HttpMethod.Delete, "/quesiti/delete")
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

        
        // Servizi CRUD per gli utenti
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
    /// Servizio CRUD per la gestione di categorie, quesiti e utenti.
    /// Tutte le richieste HTTP includono il token JWT dell'utente autenticato.
    /// </summary>
    internal class CRUDService : IDisposable
    {
        private readonly HttpClient _client;
        private readonly string _jwtToken;
        private bool _disposed;

        public CRUDService(string jwtToken)
        {
            _jwtToken = jwtToken;

            _client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:8082") // Modifica se usi un'altra porta
            };

            // Imposta l'header di autorizzazione per tutte le richieste HTTP inviate da questo client
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
        }

        // -------------------- //
        //     CATEGORIE        //
        // -------------------- //

        public Task<ServiceResult<List<Categoria>>> GetCategoriePubblicheAsync()
            => GetAsync<List<Categoria>>("/categorie/pubbliche");

        public Task<ServiceResult<List<Categoria>>> GetCategorieByStudenteAsync()
        {
            var idStudente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
            return GetAsync<List<Categoria>>($"/categorie/studente/{idStudente}");
        }

        public Task<ServiceResult<List<Categoria>>> GetCategorieByDocenteAndNomeAsync(string nome)
        {
            var idDocente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
            return GetAsync<List<Categoria>>($"/categorie/docente/{idDocente}?categoria={nome}");
        }

        public Task<ServiceResult<List<Categoria>>> GetCategoriePubblicheByDocenteAsync()
        {
            var idDocente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
            return GetAsync<List<Categoria>>($"/categorie/pubbliche/docente/{idDocente}");
        }

        public Task<ServiceResult<List<Categoria>>> GetCategorieByDocenteAsync()
        {
            var idDocente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
            return GetAsync<List<Categoria>>($"/categorie/docente/{idDocente}");
        }

        public Task<ServiceResult<Categoria>> CreateCategoriaAsync(Categoria categoria)
            => PostAsync("/categorie/create", categoria);

        public Task<ServiceResult<Categoria>> UpdateCategoriaAsync(Categoria categoria)
            => PutAsync("/categorie/update", categoria);

        public Task<ServiceResult<bool>> DeleteCategoriaAsync(List<uint> ids)
            => DeleteAsync("/categorie/delete", ids);

        // -------------------- //
        //       QUESITI        //
        // -------------------- //

        public Task<ServiceResult<List<Quesito>>> GetQuesitiByDocenteAsync()
        {
            var idDocente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
            return GetAsync<List<Quesito>>($"/quesiti/docente/{idDocente}");
        }

        public Task<ServiceResult<Quesito>> CreateQuesitoAsync(Quesito quesito)
            => PostAsync("/quesiti/create", quesito);

        public Task<ServiceResult<Quesito>> UpdateQuesitoAsync(Quesito quesito)
            => PutAsync("/quesiti/update", quesito);

        public Task<ServiceResult<bool>> DeleteQuesitoAsync(List<uint> ids)
            => DeleteAsync("/quesiti/delete", ids);

        // -------------------- //
        //        UTENTI        //
        // -------------------- //

        public Task<ServiceResult<Utente>> GetUserAsync()
        {
            var idUtente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
            return GetAsync<Utente>($"/utente/{idUtente}");
        }

        // -------------------- //
        //   METODI AUSILIARI   //
        // -------------------- //

        /// <summary>
        /// Esegue una richiesta HTTP GET e deserializza la risposta JSON nel tipo T.
        /// </summary>
        private async Task<ServiceResult<T>> GetAsync<T>(string url)
        {
            try
            {
                var response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<T>() ?? default;
                    return new ServiceResult<T> { Data = data };
                }

                return new ServiceResult<T> { ErrorMessage = await ExtractErrorMessageAsync(response) };
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

        /// <summary>
        /// Esegue una richiesta HTTP POST con corpo JSON e restituisce la risposta deserializzata.
        /// </summary>
        private async Task<ServiceResult<T>> PostAsync<T>(string url, T body)
        {
            try
            {
                var response = await _client.PostAsJsonAsync(url, body);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<T>();
                    return new ServiceResult<T> { Data = data };
                }

                return new ServiceResult<T> { ErrorMessage = await ExtractErrorMessageAsync(response) };
            }
            catch (Exception ex)
            {
                return new ServiceResult<T> { ErrorMessage = $"Errore: {ex.Message}" };
            }
        }

        /// <summary>
        /// Esegue una richiesta HTTP PUT con corpo JSON e restituisce la risposta deserializzata.
        /// </summary>
        private async Task<ServiceResult<T>> PutAsync<T>(string url, T body)
        {
            try
            {
                var response = await _client.PutAsJsonAsync(url, body);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<T>();
                    return new ServiceResult<T> { Data = data };
                }

                return new ServiceResult<T> { ErrorMessage = await ExtractErrorMessageAsync(response) };
            }
            catch (Exception ex)
            {
                return new ServiceResult<T> { ErrorMessage = $"Errore: {ex.Message}" };
            }
        }

        /// <summary>
        /// Esegue una richiesta HTTP DELETE con un corpo JSON.
        /// </summary>
        private async Task<ServiceResult<bool>> DeleteAsync(string url, object body)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, url)
                {
                    Content = JsonContent.Create(body)
                };

                var response = await _client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                    return new ServiceResult<bool> { Data = true };

                return new ServiceResult<bool> { ErrorMessage = await ExtractErrorMessageAsync(response) };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool> { ErrorMessage = $"Errore: {ex.Message}" };
            }
        }

        /// <summary>
        /// Tenta di estrarre un messaggio di errore dal corpo della risposta HTTP.
        /// </summary>
        private static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var message = $"Errore HTTP {response.StatusCode}";

            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("error", out var errorProp))
                    message = errorProp.GetString() ?? message;
            }
            catch
            {
                // Se il parsing fallisce, mantiene il messaggio generico
            }

            return message;
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
