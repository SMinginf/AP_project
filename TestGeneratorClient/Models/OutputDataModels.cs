using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TestGeneratorClient.Models
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
        [JsonPropertyName("studente")]
        public Utente Studente { get; set; } = new();

        [JsonPropertyName("stats_per_categoria_difficolta")]
        public List<StatsPerCategoriaDifficolta> StatsPerCategoriaDifficolta { get; set; } = new();

        [JsonPropertyName("andamento_temporale")]
        public List<TimelineData> AndamentoTemporale { get; set; } = new();
    }



    public class StatsPerCategoriaDifficolta
    {
        [JsonPropertyName("categoria")]
        public string Categoria { get; set; } = "";
        [JsonPropertyName("difficolta")]
        public string Difficolta { get; set; } = "";
        [JsonPropertyName("corrette")]
        public int Corrette { get; set; }
        [JsonPropertyName("sbagliate")]
        public int Sbagliate { get; set; }
        [JsonPropertyName("non_date")]
        public int NonDate { get; set; }
    }

    public class TimelineData
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
        [JsonPropertyName("corrette")]
        public int Corrette { get; set; }
        [JsonPropertyName("sbagliate")]
        public int Sbagliate { get; set; }
        [JsonPropertyName("non_date")]
        public int NonDate { get; set; }
    }
}