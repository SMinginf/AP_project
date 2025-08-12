using QuizClient.Models;
<<<<<<< HEAD
using QuizClient.Utils;
using System;
using System.Data;
=======
using System;
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

<<<<<<< HEAD
=======
using QuizClient.Utils;

>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
namespace QuizClient.Services
{
    public class QuizService
    {
        private readonly HttpClient _client;
        private readonly string _jwtToken;
<<<<<<< HEAD
       
        public QuizService(string jwtToken)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://localhost:8083"); // modifica se usi altra porta
            _jwtToken = jwtToken;


=======

        public QuizService(string jwtToken)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://localhost:8081"); // modifica se usi altra porta
            _jwtToken = jwtToken;

>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
            // Imposta l'header di autorizzazione delle richieste http che verranno inviate da questo client con il token JWT
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        }

        // Crea quiz
<<<<<<< HEAD
        public async Task<ServiceResult<Quiz>> CreateQuizAsync(bool AIg, string AIcat, List<int> id_cat, bool u, string diff, int nd, List<uint> id_ques)
=======
        public async Task<ServiceResult<Quiz>> CreateQuizAsync(bool AIg, string AIcat, List<int> id_cat, bool u, string diff, int nd)
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
        {
            var quiz_data = new
            {
                ai_generated = AIg,
                ai_categoria = AIcat,
                id_categorie = id_cat,
                unione = u,
                difficolta = diff,
                quantita = nd,
<<<<<<< HEAD
                id_quesiti = id_ques
            };
            try
            {
                string route;
                if (JwtUtils.GetClaimAsString(_jwtToken, "ruolo") == "Studente")
                {
                    route = "/quiz/create-for-student";
                }
                else { route = "/quiz/create-for-teacher"; }

                var response = await _client.PostAsJsonAsync(route, quiz_data);
                
                
=======
            };
            try
            {
                var response = await _client.PostAsJsonAsync("/quiz/create", quiz_data);
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
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
<<<<<<< HEAD
=======

>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
        public async Task<ServiceResult<Quiz>> StoreQuizAsync(Quiz q, int corrette, int sbagliate, string data_creazione, string durata, List<int?> risposte_utente) { 

            try
            {
<<<<<<< HEAD
                // Prelevo l'id dll'utente dal token JWT
=======
                // Prelevo l'id dell'utente dal token JWT
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
                uint id_utente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");

                // Preparo la lista di id dei quesiti del quiz (non serve inviare al server l'oggetto Quesito completo, ma solo gli ID)
                List<uint> id_quesiti = q.Quesiti.Select(quesito => quesito.ID).ToList();

                // Creo l'oggetto da inviare al server
                var quiz_data = new
                {
<<<<<<< HEAD
                    categoria = q.Categoria, //potrebbe essere inutile, potenzialmente cancellabile
=======
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
                    difficolta = q.Difficolta,
                    quantita = q.Quantita,
                    id_quesiti,
                    id_utente,
                    corrette,
                    sbagliate,
                    data_creazione,
                    durata,
                    risposte_utente

                };


                var response = await _client.PostAsJsonAsync("/quiz/store", quiz_data);
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
    }
 
}