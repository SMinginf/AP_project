using System;
using System.Globalization;
using System.Windows.Data;
using QuizClient.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace QuizClient.Utils
{
        public class CategoriaToStringConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                // Controlla se il valore è una collezione di Categoria
                if (value is IEnumerable<Categoria> categorie)
                {
                    // Unisci i nomi delle categorie in una singola stringa, separati da ", "
                    // Se la collezione è vuota, restituisci un messaggio specifico
                    if (!categorie.Any())
                    {
                        return "Nessuna categoria selezionata";
                    }
                    return string.Join(", ", categorie.Select(c => c.Nome));
                }

                // Se il valore non è del tipo atteso, o è null, restituisci una stringa vuota o un errore
                return "N/A"; // Not Applicable
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                // La conversione inversa non è supportata per questo scenario
                throw new NotImplementedException();
            }
        }
    }
