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


        public Task<ServiceResult<Utente>> UpdateUserAsync(Utente utente) => PutAsync("/utente/update", utente);

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
