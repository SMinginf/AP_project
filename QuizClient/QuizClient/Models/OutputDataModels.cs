using System.ComponentModel;
using System.Text.Json.Serialization;

namespace QuizClient.Models
{

    public enum Mode
    {   
        Default,
        DaFinestra,
        Reroll_AI,
        Reroll_AI_DB
    }

    // Classi che rispecchiano le strutture dei JSON restituiti dal server: C# client <-- JSON -- Server
    public class Quiz
    {
        [JsonPropertyName("titolo")]
        public string Titolo { get; set; } = "";

        [JsonPropertyName("categoria")]
        public string Categoria { get; set; } = "";
        
        [JsonPropertyName("difficolta")]
        public string Difficolta { get; set; } = "";

        [JsonPropertyName("quantita")]
        public int Quantita { get; set; } = 0 ;

        [JsonPropertyName("quesiti")]
        public List<Quesito> Quesiti { get; set; } = new();
        
        [JsonPropertyName("AIGenerated")]
        public bool AiGenerated { get; set; }

    }

    public class Quesito : INotifyPropertyChanged
    {
        private bool _selezionato;

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
        public Utente? Docente { get; set; } //= new Utente(); //ho messo il ? per evitare null reference exception

        [JsonPropertyName("categorie")]
        public List<Categoria> Categorie { get; set; } = new();

        //Gestione della selezione del quesito nella UI
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool Selezionato { 
            get => _selezionato;
            set {
                if (_selezionato != value) // Controlla se il valore è cambiato
                {
                    _selezionato = value;
                    OnPropertyChanged(nameof(Selezionato)); // Notifica la UI
                }
            } 
        } // Per bind alla checkbox
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

    public class Categoria : INotifyPropertyChanged
    {
        
        private bool _selezionata;

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

        //Gestione della selezione del quesito nella UI
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool Selezionata
        {
            get => _selezionata;
            set
            {
                if (_selezionata != value) // Controlla se il valore è cambiato
                {
                    _selezionata = value;
                    OnPropertyChanged(nameof(Selezionata)); // Notifica la UI
                }
            }
        } // Per bind alla checkbox
    }

}