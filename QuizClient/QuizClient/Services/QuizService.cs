/*
using QuizClient.Models;
using QuizClient.Utils;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
        public async Task<ServiceResult<Quiz>> CreateQuizAsync(bool AIg, string AIcat, List<uint> id_cat, bool u, string diff, int nd, List<uint> id_ques)
        {
            var quiz_data = new
            {
                ai_generated = AIg,
                ai_categoria = AIcat,
                id_categorie = id_cat,
                unione = u,
                difficolta = diff,
                quantita = nd,
                id_quesiti = id_ques // Aggiunto SQ 
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

        public async Task<ServiceResult<Quiz>> StoreQuizAsync(Quiz q, int corrette, int sbagliate, string data_creazione, string durata, List<int?> risposte_utente)
        {

            try
            {
                // Prelevo l'id dell'utente dal token JWT
                uint id_utente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");

                // Preparo la lista di id dei quesiti del quiz (non serve inviare al server l'oggetto Quesito completo, ma solo gli ID)
                List<uint> id_quesiti = q.Quesiti.Select(quesito => quesito.ID).ToList();

                // Creo l'oggetto da inviare al server
                var quiz_data = new
                {
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



        // Scarica lo ZIP delle schede generato dal server in streaming.
        public async Task<ServiceResult<string>> GenerateAndDownloadExamsZipAsync(
            int n,                          // numero di schede d'esame da generare
            IEnumerable<Quesito> questions // elenco quesiti da inviare al server
)
        {

            try
            {
                // Costruisci percorso automatico in cui salvare i file: <base>/exams/exams_YYYYMMDD_HHMMSS.zip
                var exeDir = AppContext.BaseDirectory;                  // cartella dell’eseguibile

                // Risali di tre livelli per raggiungere la radice del progetto.
                var projectRoot = Path.GetFullPath(Path.Combine(exeDir, "..", "..", ".."));

                var examsDir = Path.Combine(projectRoot, "exams");           
                Directory.CreateDirectory(examsDir);                     // crea se non esiste

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"exams_{timestamp}.zip";
                var outputPath = Path.Combine(examsDir, fileName);
                
                // Prepara il payload che verrà inviato al server in formato JSON
                var payload = new
                {
                    n,              // numero schede
                    questions       // lista domande
                };

                // Costruisce la richiesta HTTP POST verso l'endpoint che genera lo ZIP
                // Usando JsonContent.Create per serializzare automaticamente payload in JSON
                using var req = new HttpRequestMessage(HttpMethod.Post, "/quiz/exams")
                {
                    Content = JsonContent.Create(payload)
                };

                HttpResponseMessage? response = null;

                try
                {

                    // Invia la richiesta al server, ma con HttpCompletionOption.ResponseHeadersRead:
                    // significa che scarica subito solo gli header HTTP, lasciando il body da leggere a stream
                    // Questo è utile per evitare di caricare tutto il body in RAM prima di iniziare a scrivere su disco

                    // PostAsJsonAsync usa il comportamento predefinito (ResponseContentRead), quindi attende tutto il body
                    // prima di restituire la HttpResponseMessage. Per ZIP piccoli/medi va benissimo ed è molto più semplice;
                    // per ZIP grandi preferisci la versione “streaming” con SendAsync(..., ResponseHeadersRead).
                    response = await _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);

                    if (response.IsSuccessStatusCode)
                    {
                        // Ottiene lo stream HTTP dal body (lo ZIP generato in streaming dal server)
                        await using var httpStream = await response.Content.ReadAsStreamAsync();

                        // Crea (o sovrascrive) il file ZIP in locale, con buffer 80KB e async attivo
                        await using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);

                        // Copia i dati dal network stream direttamente al file, a blocchi, senza caricarli tutti in RAM
                        await httpStream.CopyToAsync(fileStream);

                        // Restituisce un ServiceResult con il percorso del file salvato
                        return new ServiceResult<string> { Data = outputPath };
                    }
                    else
                    {
                        // Se la risposta NON è success leggi il body per estrarre eventuale messaggio d'errore
                        var error = await response.Content.ReadAsStringAsync();
                        string? errorMsg = $"Errore HTTP {response.StatusCode}";
                        try
                        {
                            // Se il body è JSON e contiene un campo "error", usa quello come messaggio
                            using var doc = JsonDocument.Parse(error);
                            if (doc.RootElement.TryGetProperty("error", out var errorProp))
                                errorMsg = errorProp.GetString() ?? errorMsg;
                        }
                        catch { // body non-JSON: ignora, mantieni messaggio generico  
                                }

                        // Restituisce errore incapsulato in ServiceResult
                        return new ServiceResult<string> { ErrorMessage = errorMsg };
                    }
                }
                catch (HttpRequestException ex)
                {
                    // Errore di rete (connessione, DNS, timeout basso livello)
                    return new ServiceResult<string> { ErrorMessage = $"Errore di rete: {ex.Message}" };
                }
                catch (Exception ex)
                {
                    // Qualsiasi altro errore non previsto (scrittura file, serializzazione, ecc.)
                    return new ServiceResult<string> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
                }
                finally
                {
                    // Rilascia le risorse HTTP (connessione, stream, ecc.)
                    response?.Dispose();
                }
            }
            catch (Exception ex)
            {
                // errore nel preparare cartella/percorso
                return new ServiceResult<string> { ErrorMessage = $"Errore nel preparare il percorso di salvataggio: {ex.Message}" };

            }

        }

    }

}
*/

using QuizClient.Models;
using QuizClient.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuizClient.Services
{
    public sealed class QuizService : IDisposable
    {
        private readonly HttpClient _client;
        private readonly string _jwtToken;
        private bool _disposed;

        public QuizService(string jwtToken)
        {
            _jwtToken = jwtToken;

            _client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:8081") // modifica se usi altra porta
            };

            // Imposta l’header di autorizzazione per tutte le richieste HTTP
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        }

        /// <summary>
        /// Crea un nuovo quiz sul server.
        /// </summary>
        public async Task<ServiceResult<Quiz>> CreateQuizAsync(
            bool aiGenerated,
            string aiCategory,
            List<uint> categoryIds,
            bool merge,
            string difficulty,
            int questionCount,
            List<uint> questionIds)
        {
            var payload = new
            {
                ai_generated = aiGenerated,
                ai_categoria = aiCategory,
                id_categorie = categoryIds,
                unione = merge,
                difficolta = difficulty,
                quantita = questionCount,
                id_quesiti = questionIds
            };

            try
            {
                var response = await _client.PostAsJsonAsync("/quiz/create", payload);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<Quiz>();
                    return new ServiceResult<Quiz> { Data = data };
                }

                var error = await response.Content.ReadAsStringAsync();
                var errorMsg = ExtractErrorMessage(response.StatusCode.ToString(), error);

                return new ServiceResult<Quiz> { ErrorMessage = errorMsg };
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

        /// <summary>
        /// Salva un quiz completato dall’utente sul server.
        /// </summary>
        public async Task<ServiceResult<Quiz>> StoreQuizAsync(
            Quiz quiz,
            int correct,
            int incorrect,
            string creationDate,
            string duration,
            List<int?> userAnswers)
        {
            try
            {
                var userId = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
                var questionIds = quiz.Quesiti.Select(q => q.ID).ToList();

                var payload = new
                {
                    difficolta = quiz.Difficolta,
                    quantita = quiz.Quantita,
                    id_quesiti = questionIds,
                    id_utente = userId,
                    corrette = correct,
                    sbagliate = incorrect,
                    data_creazione = creationDate,
                    durata = duration,
                    risposte_utente = userAnswers
                };

                var response = await _client.PostAsJsonAsync("/quiz/store", payload);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<Quiz>();
                    return new ServiceResult<Quiz> { Data = data };
                }

                var error = await response.Content.ReadAsStringAsync();
                var errorMsg = ExtractErrorMessage(response.StatusCode.ToString(), error);

                return new ServiceResult<Quiz> { ErrorMessage = errorMsg };
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

        /// <summary>
        /// Genera e scarica in streaming un file ZIP con le schede d’esame.
        /// </summary>
        public async Task<ServiceResult<string>> GenerateAndDownloadExamsZipAsync(
            int examCount,
            IEnumerable<Quesito> questions)
        {
            try
            {
                // Costruisci il percorso del file ZIP
                var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
                var examsDir = Path.Combine(projectRoot, "exams");
                Directory.CreateDirectory(examsDir);

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var filePath = Path.Combine(examsDir, $"exams_{timestamp}.zip");

                var payload = new { n = examCount, questions };

                using var request = new HttpRequestMessage(HttpMethod.Post, "/quiz/exams")
                {
                    Content = JsonContent.Create(payload)
                };

                using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                {
                    await using var httpStream = await response.Content.ReadAsStreamAsync();
                    await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
                    await httpStream.CopyToAsync(fileStream);

                    return new ServiceResult<string> { Data = filePath };
                }

                var error = await response.Content.ReadAsStringAsync();
                var errorMsg = ExtractErrorMessage(response.StatusCode.ToString(), error);
                return new ServiceResult<string> { ErrorMessage = errorMsg };
            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult<string> { ErrorMessage = $"Errore di rete: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult<string> { ErrorMessage = $"Errore imprevisto: {ex.Message}" };
            }
        }

        private static string ExtractErrorMessage(string statusCode, string responseBody)
        {
            var defaultMessage = $"Errore HTTP {statusCode}";

            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                if (doc.RootElement.TryGetProperty("error", out var errorProp))
                    return errorProp.GetString() ?? defaultMessage;
            }
            catch
            {
                // non JSON — ignora, restituisci messaggio generico
            }

            return defaultMessage;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _client.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
