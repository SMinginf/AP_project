using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using QuizClient.Models;

namespace QuizClient.Services
{
    
    internal class CatService 
    {
        private readonly HttpClient _client;
        private readonly string _jwtToken;
        
        
        public CatService(string jwtToken)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://localhost:8082"); // Modifica se usi un'altra porta
            _jwtToken = jwtToken;
            // Imposta l'header di autorizzazione delle richieste http che verranno inviate da questo client con il token JWT
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
        }

        // Metodo per ottenere le categorie disponibili
        public async Task<List<Categoria>> GetCategorieAsync()
        {
            try
            {
                var response = await _client.GetAsync("/categorie/list");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Categoria>>() ?? new List<Categoria>();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Errore HTTP {response.StatusCode}: {error}");
                    System.Windows.MessageBox.Show("Errore nel recupero delle categorie dal server.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    return new List<Categoria>();
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Errore di rete: {ex.Message}");
                System.Windows.MessageBox.Show("Errore di rete: impossibile contattare il server.", "Errore di rete", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Categoria>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore imprevisto: {ex.Message}");
                System.Windows.MessageBox.Show("Si è verificato un errore imprevisto.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Categoria>();
            }
        }

    }
}
