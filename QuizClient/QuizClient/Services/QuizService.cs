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
        public async Task<ServiceResult<Quiz>> CreateQuizAsync(bool AIg, string AIcat, List<uint> id_cat, bool u, string diff, int nd)
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
                var baseDir = AppContext.BaseDirectory;                  // cartella dell’eseguibile
                var examsDir = Path.Combine(baseDir, "exams");           // <base>/exams
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
                        catch { /* body non-JSON: ignora, mantieni messaggio generico */ }

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