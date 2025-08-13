using System;
using System.Globalization;
using System.Windows.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

//utilizzato nello XAML per convertire il valore booleano in una stringa "Pubblica" o "Privata"
namespace QuizClient.Utils
{
    //la classe PubblicaToStringConverter è un Value Converter (convertitore di valore) che implementa 
    //l'interfaccia IValueConverter. Il suo scopo è prendere un valore booleano (bool) e convertirlo 
    //in una stringa testuale ("pubblica" o "privata") per la visualizzazione nella UI
    // L'implementazione di questa interfaccia è ciò che rende la classe un
    // "convertitore di valore" utilizzabile nel data binding XAML.
    public class PubblicaToStringConverter : IValueConverter
    {
        // l'unico metodo richiesto dall'interfaccia IValueConverter per la conversione "in avanti" (dal sorgente dati alla UI).
        // "object value" valore sorgente da convertire (in questo caso, un booleano).
        // "Type targetType" è il tipo di destinazione della conversione (in questo caso, una stringa).
        // "object parameter" è un parametro opzionale che può essere passato al convertitore dal binding XAML (non usato qui).
        // Utile se il convertitore ha bisogno di comportamenti diversi a seconda del contesto.
        // "CultureInfo culture" è la cultura corrente, che può influenzare la formattazione dei dati (non usata qui).
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Logica di conversione
            // "(value is bool b)": Questo è un pattern matching di C#. Tenta di convertire value in un booleano.
            // Se value è un bool, lo assegna a una nuova variabile b di tipo bool e l'espressione è vera.
            // "&& b": Se la conversione a bool b ha avuto successo, controlla anche se il valore di b è true
            // "? "Pubblica" : "Privata"": (Operatore Ternario) Espressione condizionale concisa
            // Se la condizione (value is bool b && b) è vera (cioè, value è un booleano e quel booleano è true),
            // il metodo restituisce la stringa "Pubblica".
            // Altrimenti (cioè, value non è un booleano, o è un booleano ma è false), il metodo restituisce la stringa "Privata".
            return (value is bool b && b) ? "Pubblica" : "Privata";
        }

        // Il secondo metodo richiesto dall'interfaccia IValueConverter per la conversione "in indietro" (dalla UI al sorgente dati).
        // In questo caso, non è implementato perché non è necessario convertire da stringa a booleano.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}