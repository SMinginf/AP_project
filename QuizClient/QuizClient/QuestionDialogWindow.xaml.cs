/*using Microsoft.Win32;
using QuizClient.Models;
using QuizClient.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QuizClient
{
    public partial class QuestionDialogWindow : Window
    {
        private readonly string _jwtToken;
        private readonly Mode _mode;
        private List<Categoria> _categorieSelezionate = new List<Categoria>();
        public Quesito QuesitoCreato { get; private set; }



        //Costruttore della finestra di dialogo per creare un nuovo quesito
        public QuestionDialogWindow(string tk, Mode m)
        {
            _jwtToken = tk;
            _mode = m;
            InitializeComponent();
            QuesitoCreato = new Quesito(); // Inizializza il quesito creato
            // Controllo ruolo utente dal JWT
            Utils.JwtUtils.ValidateDocenteRole(_jwtToken, null, this);
        }
        // Costruttore chiamato per modificare un quesito esistente
        public QuestionDialogWindow(Quesito esistente, string tk, Mode mode) : this(tk, mode)
        {
            // Mostra i dati del quesito esistente nei campi di input
            DifficoltaBox.SelectedValue = esistente.Difficolta;

            TestoBox.Text = esistente.Testo;
            OpzioneABox.Text = esistente.OpzioneA;
            OpzioneBBox.Text = esistente.OpzioneB;
            OpzioneCBox.Text = esistente.OpzioneC;
            OpzioneDBox.Text = esistente.OpzioneD;
             
            OpCorrettaBox.SelectedIndex = esistente.OpCorretta - 1; // 1, 2, 3, 4 in 0, 1, 2 o 3
            CategoryBox.Text = string.Join(", ", esistente.Categorie.Select(c => c.Nome));

            // Crea una copia per evitare modifiche all'oggetto originale
            QuesitoCreato = new Quesito
            {
                ID = esistente.ID,
                Difficolta = esistente.Difficolta,
                OpzioneA = esistente.OpzioneA,
                OpzioneB = esistente.OpzioneB,
                OpzioneC = esistente.OpzioneC,
                OpzioneD = esistente.OpzioneD,
                OpCorretta = esistente.OpCorretta,
                Testo = esistente.Testo,
                IDDocente = esistente.IDDocente,
                Docente = esistente.Docente,
                Categorie = esistente.Categorie
            };

            // Inizializza le categorie selezionate con quelle del quesito esistente
            _categorieSelezionate = esistente.Categorie;

            GestisciVisibilita(); // Gestisce la visibilità degli elementi in base al contesto

        }



        private void GestisciVisibilita()
        {
            if (_mode == Mode.FromQuizManagerPage)
            {
                // Imposta la visibilità su Collapsed per nascondere gli elementi
                DifficoltaLabel.Visibility = Visibility.Collapsed;
                DifficoltaBox.Visibility = Visibility.Collapsed;
                CorrettaStackPanel.Visibility = Visibility.Collapsed;
                CategoryLabel.Visibility = Visibility.Collapsed;
                CategoryDockPanel.Visibility = Visibility.Collapsed;
            }
        }

        // Metodo per salvare il quesito creato o modificato
        private void Salva_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DifficoltaBox.Text))
            {
                MessageBox.Show("Specificare la difficoltà è obbligatorio.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(TestoBox.Text))
            {
                MessageBox.Show("Il testo è obbligatorio.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(CategoryBox.Text))
            {
                MessageBox.Show("Necessario aggiungere il quesito a delle categorie.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            uint idDocente;
            try
            {
                idDocente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel recupero dell'ID docente dal token JWT: {ex.Message}", "Errore JWT", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Imposta i valori del quesito creato da input dell'utente
            QuesitoCreato.IDDocente = idDocente;
            QuesitoCreato.Difficolta = DifficoltaBox.Text.Trim();
            QuesitoCreato.Testo = TestoBox.Text.Trim();
            QuesitoCreato.OpzioneA = OpzioneABox.Text.Trim();
            QuesitoCreato.OpzioneB = OpzioneBBox.Text.Trim();
            QuesitoCreato.OpzioneC = OpzioneCBox.Text.Trim();
            QuesitoCreato.OpzioneD = OpzioneDBox.Text.Trim();
            QuesitoCreato.OpCorretta = OpCorrettaBox.SelectedIndex + 1; // 0, 1, 2 o 3 in 1, 2, 3 o 4

            
            // Assegna le categorie selezionate al quesito creato: 
            // o riassegna le categorie precedenti o sovrascrive con le nuove selezionate
            QuesitoCreato.Categorie = _categorieSelezionate;

            //Controllo che l'opzione corretta sia selezionata
            if (QuesitoCreato.OpCorretta < 1 || QuesitoCreato.OpCorretta > 4)
            {
                MessageBox.Show("Devi selezionare un'opzione corretta.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        // Metodo per annullare l'operazione e chiudere la finestra senza salvare
        private void Annulla_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        // Metodo per aprire la finestra di selezione delle categorie 
        private void ApriSelezioneCategorie_Click(object sender, RoutedEventArgs e)
        {
            //OrigineClick è un enum che indica da dove è stata aperta la finestra di selezione categorie
            var selettore = new CategorySelectionWindow(_jwtToken, Mode.DaFinestra);
            if (selettore.ShowDialog() == true && selettore.Selezionate != null)
            {
                _categorieSelezionate = selettore.Selezionate;
                
            }
            CategoryBox.Text = string.Join(", ", _categorieSelezionate.Select(c => c.Nome));
        }


    }
}
*/

using Microsoft.Win32;
using QuizClient.Models;
using QuizClient.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace QuizClient
{
    public partial class QuestionDialogWindow : Window
    {
        private readonly string _jwtToken;
        private readonly Mode _mode;
        private List<Categoria> _categorieSelezionate = new();

        public Quesito QuesitoCreato { get; private set; }

        // --------------------------- //
        //     COSTRUTTORI FINESTRA    //
        // --------------------------- //

        // Costruttore per la creazione di un nuovo quesito
        public QuestionDialogWindow(string jwtToken, Mode mode)
        {
            InitializeComponent();

            _jwtToken = jwtToken;
            _mode = mode;
            QuesitoCreato = new Quesito();

            // Controllo del ruolo utente dal JWT
            JwtUtils.ValidateDocenteRole(_jwtToken, null, this);
        }

        // Costruttore chiamato per modificare un quesito esistente
        public QuestionDialogWindow(Quesito esistente, string jwtToken, Mode mode) : this(jwtToken, mode)
        {
            // Popola i campi di input con i dati del quesito esistente
            DifficoltaBox.SelectedValue = esistente.Difficolta;
            TestoBox.Text = esistente.Testo;
            OpzioneABox.Text = esistente.OpzioneA;
            OpzioneBBox.Text = esistente.OpzioneB;
            OpzioneCBox.Text = esistente.OpzioneC;
            OpzioneDBox.Text = esistente.OpzioneD;
            OpCorrettaBox.SelectedIndex = esistente.OpCorretta - 1;
            CategoryBox.Text = string.Join(", ", esistente.Categorie.Select(c => c.Nome));

            // Copia dell'oggetto per evitare modifiche all'originale
            QuesitoCreato = new Quesito
            {
                ID = esistente.ID,
                Difficolta = esistente.Difficolta,
                OpzioneA = esistente.OpzioneA,
                OpzioneB = esistente.OpzioneB,
                OpzioneC = esistente.OpzioneC,
                OpzioneD = esistente.OpzioneD,
                OpCorretta = esistente.OpCorretta,
                Testo = esistente.Testo,
                IDDocente = esistente.IDDocente,
                Docente = esistente.Docente,
                Categorie = new List<Categoria>(esistente.Categorie)
            };

            _categorieSelezionate = new List<Categoria>(esistente.Categorie);

            GestisciVisibilita();
        }

        // --------------------------- //
        //     GESTIONE INTERFACCIA    //
        // --------------------------- //

        private void GestisciVisibilita()
        {
            if (_mode != Mode.FromQuizManagerPage)
                return;

            // Nasconde gli elementi non rilevanti per la creazione da QuizManager
            DifficoltaLabel.Visibility = Visibility.Collapsed;
            DifficoltaBox.Visibility = Visibility.Collapsed;
            CorrettaStackPanel.Visibility = Visibility.Collapsed;
            CategoryLabel.Visibility = Visibility.Collapsed;
            CategoryDockPanel.Visibility = Visibility.Collapsed;
        }

        // --------------------------- //
        //     EVENTI BUTTON CLICK     //
        // --------------------------- //

        // Salva il quesito creato o modificato
        private void Salva_Click(object sender, RoutedEventArgs e)
        {
            // Validazioni base
            if (string.IsNullOrWhiteSpace(DifficoltaBox.Text))
            {
                MessageBox.Show("Specificare la difficoltà è obbligatorio.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TestoBox.Text))
            {
                MessageBox.Show("Il testo è obbligatorio.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(CategoryBox.Text))
            {
                MessageBox.Show("È necessario aggiungere il quesito a una o più categorie.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Estrae ID docente dal JWT
            uint idDocente;
            try
            {
                idDocente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel recupero dell'ID docente dal token JWT:\n{ex.Message}",
                    "Errore JWT", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Imposta i valori del quesito creato
            QuesitoCreato.IDDocente = idDocente;
            QuesitoCreato.Difficolta = DifficoltaBox.Text.Trim();
            QuesitoCreato.Testo = TestoBox.Text.Trim();
            QuesitoCreato.OpzioneA = OpzioneABox.Text.Trim();
            QuesitoCreato.OpzioneB = OpzioneBBox.Text.Trim();
            QuesitoCreato.OpzioneC = OpzioneCBox.Text.Trim();
            QuesitoCreato.OpzioneD = OpzioneDBox.Text.Trim();
            QuesitoCreato.OpCorretta = OpCorrettaBox.SelectedIndex + 1; // 0–3 → 1–4
            QuesitoCreato.Categorie = new List<Categoria>(_categorieSelezionate);

            // Verifica selezione risposta corretta
            if (QuesitoCreato.OpCorretta is < 1 or > 4)
            {
                MessageBox.Show("Devi selezionare un'opzione corretta.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        // Annulla l'operazione e chiude la finestra
        private void Annulla_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        // Apre la finestra di selezione delle categorie
        private void ApriSelezioneCategorie_Click(object sender, RoutedEventArgs e)
        {
            // OrigineClick è un enum che indica da dove è stata aperta la finestra
            var selettore = new CategorySelectionWindow(_jwtToken, Mode.DaFinestra);

            if (selettore.ShowDialog() == true && selettore.Selezionate is not null)
                _categorieSelezionate = selettore.Selezionate;

            CategoryBox.Text = string.Join(", ", _categorieSelezionate.Select(c => c.Nome));
        }
    }
}
