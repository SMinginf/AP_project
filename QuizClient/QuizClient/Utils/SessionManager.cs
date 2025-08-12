namespace QuizClient.Utils
{
<<<<<<< HEAD
    // "static" rende la classe un contenitore univoco di cui non possono essere create istanze
    // "public" lo rende globale e visibile a tutte le altre classi del progetto
    public static class SessionManager
    {
        // tutti i metodi della classe devono essere statici per definizione 
        // dunque questa proprietà è accessibile direttamente dalla classe stessa,
        // senza bisogno di un'istanza (es. SessionManager.AccessToken).
        // Essendo static, esiste una sola copia di questa proprietà per tutta la durata dell'applicazione,
        // condivisa da tutti.
=======
    public static class SessionManager
    {
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
        public static string? AccessToken { get; set; }
    }
}
