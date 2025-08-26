using QuizClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace QuizClient.Utils
{
    public static class JSONLoader
    {
        private static int count = 0;
        private static bool err = false;
        private static readonly HashSet<string> DifficoltaConsentite = ["Facile", "Intermedia", "Difficile", "Qualsiasi"];
                    
        public static List<Quesito>? JSONtoQuesiti(string fileContent)
        {
            List<Quesito> quesitiDaJSON = new List<Quesito>();
            count = 0;
            err = false;
            
            try
            {
                // Deserializzo il contenuto JSON in una lista di Quesiti
                quesitiDaJSON = JsonSerializer.Deserialize<List<Quesito>>(fileContent) ?? new List<Quesito>();
                // Controllo se la lista è vuota
                if (quesitiDaJSON.Count == 0)
                {
                    MessageBox.Show("Il file JSON non contiene quesiti validi.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }

                // Controllo se i quesiti hanno tutti i campi obbligatori
                foreach (var que in quesitiDaJSON) {
                    count++;
                    if (!DifficoltaConsentite.Contains(que.Difficolta))
                    {
                        MessageBox.Show($"Il quesito numero {count} ha una difficoltà non valida: {que.Difficolta}. Deve essere 'Facile', 'Intermedia', 'Difficile' o 'Qualsiasi'.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                        err = true;
                    }
                    else if (string.IsNullOrWhiteSpace(que.Testo))
                    {
                        MessageBox.Show($"Il quesito numero {count} non ha un testo valido.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                        err = true;
                    }
                    else if (string.IsNullOrWhiteSpace(que.OpzioneA))
                    {
                        MessageBox.Show($"Il quesito numero {count} non ha un'opzione A valida.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                        err = true;
                    }
                    else if (string.IsNullOrWhiteSpace(que.OpzioneB)) {
                        MessageBox.Show($"Il quesito numero {count} non ha un'opzione B valida.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                        err = true;
                    }
                    else if (string.IsNullOrWhiteSpace(que.OpzioneC)) {
                        MessageBox.Show($"Il quesito numero {count} non ha un'opzione C valida.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                        err = true;
                    }
                    else if (string.IsNullOrWhiteSpace(que.OpzioneD)) {
                        MessageBox.Show($"Il quesito numero {count} non ha un'opzione D valida.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                        err = true;
                    }
                    else if (que.OpCorretta < 1 || que.OpCorretta > 4) {
                        MessageBox.Show($"Il quesito numero {count} ha un'opzione corretta non valida: {que.OpCorretta}. Deve essere un numero tra 1 e 4.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                        err = true;
                    }
                    // in caso di errore, interrompo il ciclo
                    if (err)
                    {
                        MessageBox.Show("Si prega di correggere gli errori e riprovare.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                }
                return quesitiDaJSON;
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Errore durante la deserializzazione del JSON: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            catch (ArgumentNullException ex) {
                MessageBox.Show($"Il contenuto del file è 'null': {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }

}
