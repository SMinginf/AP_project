using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizClient.Utils
{
<<<<<<< HEAD
    // Questa è una classe generica progettata per incapsulare il risultato di un'operazione di servizio,
    // fornendo un modo standardizzato per comunicare successo, fallimento, dati e messaggi di errore.
    // Il <T> la rende una classe generica (Generics): T è un placeholder per un tipo. Quando utilizzi ServiceResult,
    // specifichi il tipo effettivo dei dati che l'operazione dovrebbe restituire
    // (es. ServiceResult<List<Quesito>>, ServiceResult<bool>, ServiceResult<Utente>).
    // Questo rende la classe riutilizzabile per qualsiasi tipo di dato.
    public class ServiceResult<T> 
    {
        public T? Data { get; set; }
        public bool Success => ErrorMessage == null;
        //Questa è una "expression-bodied property" (proprietà con corpo espressione).
        //È una forma concisa per definire una proprietà il cui valore è il risultato di una singola espressione.
        //Significa che la proprietà Success sarà true se e solo se la proprietà ErrorMessage è null.
        //Altrimenti, sarà false. Non ha un setter, il suo valore è calcolato dinamicamente.
=======
    public class ServiceResult<T>
    {
        public T? Data { get; set; }
        public bool Success => ErrorMessage == null;
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
        public string? ErrorMessage { get; set; }
    }
}
