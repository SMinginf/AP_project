using System.Text.Json.Serialization;

namespace QuizClient.Models
{
    // Classi che rispecchiano le strutture dei JSON restituiti dal server: C# client <-- JSON -- Server
    public class Quiz
    {
        [JsonPropertyName("titolo")]
        public string Titolo { get; set; } = "";

        
        [JsonPropertyName("difficolta")]
        public string Difficolta { get; set; } = "";

        [JsonPropertyName("quantita")]
        public int Quantita { get; set; } = 0 ;

        [JsonPropertyName("quesiti")]
        public List<Quesito> Quesiti { get; set; } = new();
        
        
        [JsonPropertyName("ai_generated")]
        public bool AiGenerated { get; set; }
        
    }

    public class Quesito
    {
        [JsonPropertyName("id")]
        public uint ID { get; set; }

        [JsonPropertyName("difficolta")]
        public string Difficolta { get; set; } = "";

        [JsonPropertyName("testo")]
        public string Testo { get; set; } = "";

        [JsonPropertyName("opzione_a")]
        public string OpzioneA { get; set; } = "";

        [JsonPropertyName("opzione_b")]
        public string OpzioneB { get; set; } = "";

        [JsonPropertyName("opzione_c")]
        public string OpzioneC { get; set; } = "";

        [JsonPropertyName("opzione_d")]
        public string OpzioneD { get; set; } = "";

        [JsonPropertyName("op_corretta")]
        public int OpCorretta { get; set; }

        [JsonPropertyName("id_docente")]
        public uint IDDocente { get; set; }

        [JsonPropertyName("docente")]
        public Utente Docente { get; set; } = new Utente();

        [JsonPropertyName("categorie")]
        public List<Categoria> Categorie { get; set; } = new();
    }


    public class Utente
    {
        [JsonPropertyName("id")]
        public uint ID { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = "";

        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("nome")]
        public string Nome { get; set; } = "";

        [JsonPropertyName("cognome")]
        public string Cognome { get; set; } = "";

        [JsonPropertyName("data_nascita")]
        public DateTime DataNascita { get; set; }

        [JsonPropertyName("genere")]
        public string Genere { get; set; } = "";

        [JsonPropertyName("ruolo")]
        public string Ruolo { get; set; } = "";

        [JsonPropertyName("quesiti")]
        public List<Quesito>? Quesiti { get; set; }

        [JsonPropertyName("categorie")]
        public List<Categoria>? Categorie { get; set; }
    }

    public class Categoria
    {
        [JsonPropertyName("id")]
        public uint ID { get; set; }

        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = "";

        [JsonPropertyName("nome")]
        public string Nome { get; set; } = "";

        [JsonPropertyName("descrizione")]
        public string Descrizione { get; set; } = "";

        [JsonPropertyName("id_docente")]
        public uint IDDocente { get; set; }

        [JsonPropertyName("pubblica")]
        public bool Pubblica { get; set; }

        [JsonPropertyName("docente")]
        public Utente? Docente { get; set; }

        [JsonPropertyName("quesiti")]
        public List<Quesito>? Quesiti { get; set; }

        public bool Selezionata { get; set; } // Per bind alla checkbox
    }



    // Modello per la risposta del server quando si richiedono le statistiche di uno studente
    public class StudentStatsResponse
    {
        [JsonPropertyName("categorie_filtrate")]
        public List<uint> CategorieFiltrate { get; set; } = new();

        [JsonPropertyName("percentuali_totali")]
        public PercentualiTotali PercentualiTotali { get; set; } = new();

        [JsonPropertyName("quiz_e_quesiti_totali")]
        public QuizEQuesitiTotali QuizEQuesitiTotali { get; set; } = new();

        [JsonPropertyName("stats_per_categoria_difficolta")]
        public List<StatsPerCategoriaDifficolta> StatsPerCategoriaDifficolta { get; set; } = new();

        [JsonPropertyName("andamento_temporale")]
        public List<TimelineStats> AndamentoTemporale { get; set; } = new();
    }

    public class PercentualiTotali
    {
        [JsonPropertyName("perc_corrette")]
        public double PercCorrette { get; set; }

        [JsonPropertyName("perc_sbagliate")]
        public double PercSbagliate { get; set; }

        [JsonPropertyName("perc_non_date")]
        public double PercNonDate { get; set; }

        [JsonPropertyName("totale_risposte")]
        public int TotaleRisposte { get; set; }

        [JsonPropertyName("totale_corrette")]
        public int TotaleCorrette { get; set; }

        [JsonPropertyName("totale_sbagliate")]
        public int TotaleSbagliate { get; set; }

        [JsonPropertyName("totale_non_date")]
        public int TotaleNonDate { get; set; }
    }

    public class QuizEQuesitiTotali
    {
        [JsonPropertyName("quiz_unici")]
        public int QuizUnici { get; set; }

        [JsonPropertyName("quesiti_totali")]
        public int QuesitiTotali { get; set; }

        [JsonPropertyName("categorie_coinvolte")]
        public int CategorieCoinvolte { get; set; }
    }

    public class StatsPerCategoriaDifficolta
    {
        [JsonPropertyName("difficolta")]
        public string Difficolta { get; set; } = "";

        [JsonPropertyName("corrette")]
        public int Corrette { get; set; }

        [JsonPropertyName("sbagliate")]
        public int Sbagliate { get; set; }

        [JsonPropertyName("non_date")]
        public int NonDate { get; set; }
    }

    public class TimelineStats
    {
        [JsonPropertyName("data_quiz")]
        public DateTime DataQuiz { get; set; }

        [JsonPropertyName("difficolta")]
        public string Difficolta { get; set; } = "";

        [JsonPropertyName("corrette")]
        public int Corrette { get; set; }

        [JsonPropertyName("totale_quesiti")]
        public int TotaleQuesiti { get; set; }

    }

    // MODELLO PER LE STATISTICHE GENERALI DI UNO STUDENTE
    public class StudentGeneralStats
    {
        [JsonPropertyName("statistiche_generali")]
        public StatisticheGenerali StatisticheGenerali { get; set; } = new();

        [JsonPropertyName("quiz_completati")]
        public int QuizCompletati { get; set; }

        [JsonPropertyName("punteggi_per_categoria")]
        public List<PunteggioCategoria> PunteggiPerCategoria { get; set; } = new();

        [JsonPropertyName("categoria_piu_forte")]
        public CategoriaPrestazione? CategoriaPiuForte { get; set; }

        [JsonPropertyName("categoria_piu_debole")]
        public CategoriaPrestazione? CategoriaPiuDebole { get; set; }
    }

    public class CategoriaPrestazione
    {
        [JsonPropertyName("categoria_id")]
        public int CategoriaId { get; set; }

        [JsonPropertyName("categoria_nome")]
        public string CategoriaNome { get; set; } = "";

        [JsonPropertyName("docente_username")]
        public string DocenteUsername { get; set; } = "";

        [JsonPropertyName("perc_corrette")]
        public double PercCorrette { get; set; }

        [JsonPropertyName("punteggio")]
        public double Punteggio { get; set; }

        [JsonPropertyName("totale_quesiti")]
        public int TotaleQuesiti { get; set; }
    }

    public class PunteggioCategoria
    {
        [JsonPropertyName("categoria_id")]
        public int CategoriaId { get; set; }

        [JsonPropertyName("categoria_nome")]
        public string CategoriaNome { get; set; } = "";

        [JsonPropertyName("docente_username")]
        public string DocenteUsername { get; set; } = "";

        [JsonPropertyName("corrette")]
        public int Corrette { get; set; }

        [JsonPropertyName("sbagliate")]
        public int Sbagliate { get; set; }

        [JsonPropertyName("non_date")]
        public int NonDate { get; set; }

        [JsonPropertyName("totale_quesiti")]
        public int TotaleQuesiti { get; set; }

        [JsonPropertyName("perc_corrette")]
        public double PercCorrette { get; set; }

        [JsonPropertyName("perc_sbagliate")]
        public double PercSbagliate { get; set; }

        [JsonPropertyName("perc_non_date")]
        public double PercNonDate { get; set; }

        [JsonPropertyName("punteggio")]
        public double Punteggio { get; set; }
    }

    public class StatisticheGenerali
    {
        [JsonPropertyName("totale_quesiti")]
        public int TotaleQuesiti { get; set; }

        [JsonPropertyName("totale_corrette")]
        public int TotaleCorrette { get; set; }

        [JsonPropertyName("totale_sbagliate")]
        public int TotaleSbagliate { get; set; }

        [JsonPropertyName("totale_non_date")]
        public int TotaleNonDate { get; set; }

        [JsonPropertyName("perc_corrette")]
        public double PercCorrette { get; set; }

        [JsonPropertyName("perc_sbagliate")]
        public double PercSbagliate { get; set; }

        [JsonPropertyName("perc_non_date")]
        public double PercNonDate { get; set; }
    }
}