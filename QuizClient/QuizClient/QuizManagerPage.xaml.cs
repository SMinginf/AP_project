using QuizClient;
using QuizClient.Models;
using QuizClient.Services;
using QuizClient.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using static System.Net.Mime.MediaTypeNames;

namespace QuizClient
{
    public partial class QuizManagerPage : Page
    {
        private enum quizMode
        {
            Manuale,
            Random
        }

        private readonly Frame _mainFrame;
        private readonly string _jwtToken;
        private readonly CRUDService _crudService;
        private readonly QuizService _quizService;
        private Mode _mode;
        private quizMode _qm;


        private List<Quesito> TuttiQuesiti = new List<Quesito>();
        private List<int> _indice_quesiti = new List<int>();
        private List<int> _indici_non_sostituiti = new List<int>();
        private List<uint> _id_quesiti = new List<uint>();

        public ObservableCollection<Quesito> QuesitiQuiz { get; set; } = new ObservableCollection<Quesito>();
       
        public QuizManagerPage(Frame mainFrame, string jwtToken, Quiz quiz)
        {
            InitializeComponent();
            _mainFrame = mainFrame;
            _jwtToken = jwtToken;
            _mode = Mode.Default;

            QuesitiQuiz = new ObservableCollection<Quesito>(quiz.Quesiti);
            TuttiQuesiti = quiz.Quesiti; // quesiti presenti nel quiz appena generato
            
            if (TuttiQuesiti.Count == 0)
            {
                _qm = quizMode.Manuale; // Se non ci sono quesiti, il quiz è in modalità manuale
            }
            else _qm = quizMode.Random; // Altrimenti è in modalità manuale

            

            _crudService = new CRUDService(_jwtToken);
            _quizService = new QuizService(_jwtToken);


            //Validazione del token JWT per assicurarsi che l'utente sia un docente
            Utils.JwtUtils.ValidateDocenteRole(_jwtToken, this.NavigationService, this);
            QuesitiQuizGrid.ItemsSource = QuesitiQuiz;
            //Inizializza la lista dei quiz
            CaricaQuesitiQuiz();

        }

        //Metodo per caricare i quesiti del quiz su schermo
        private async void CaricaQuesitiQuiz()
        {
            // Carico tutti i quesiti dal database così che l'utente possa sceglierli
            // per comporre il proprio quiz template
            if (_qm == quizMode.Manuale)
            {
                var result = await _crudService.GetQuesitiByDocenteAsync();
                if (result.Success && result.Data != null)
                {
                    TuttiQuesiti = result.Data;
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage ?? "Errore nel caricamento dei quesiti.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            FiltraQuesiti(string.Empty);
        }
        private void FiltraQuesiti(string filtro)
        {
            var quesitiFiltrati = TuttiQuesiti.Where(q =>
                string.IsNullOrWhiteSpace(filtro) || q.Testo.Contains(filtro, StringComparison.OrdinalIgnoreCase)
            ).ToList();

            // Rimuove dalla collezione gli elementi che non corrispondono al filtro
            foreach (var quesito in QuesitiQuiz.ToList())
            {
                if (!quesitiFiltrati.Contains(quesito))
                {
                    QuesitiQuiz.Remove(quesito);
                }
            }

            // Aggiunge alla collezione gli elementi che corrispondono al filtro ma non sono presenti
            foreach (var quesito in quesitiFiltrati)
            {
                if (!QuesitiQuiz.Contains(quesito))
                {
                    QuesitiQuiz.Add(quesito);
                }
            }
        }

        // Metodo per gestire il cambiamento del testo nella casella di ricerca.
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Tenta di convertire sender in un TextBox. Se ci riesci (e non è null), allora prendi la sua proprietà Text.
            // Se la conversione fallisce o sender è null, allora l'intera espressione restituisce null
            var filtro = (sender as TextBox)?.Text ?? string.Empty;
            FiltraQuesiti(filtro);
        }
        //Metodo per aprire un quesito e modificarlo per il quiz
        private void QuesitiQuizGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Verifica che il doppio click sia su una riga e non sull'header
            var row = ItemsControl.ContainerFromElement(QuesitiQuizGrid, e.OriginalSource as DependencyObject) as DataGridRow;
            if (row == null)
                return;

            if (row.Item is Quesito selezionato)
            {
                var finestra = new QuestionDialogWindow(selezionato, _jwtToken);
                if (finestra.ShowDialog() == true)
                {
                    int i = TuttiQuesiti.IndexOf(selezionato);
                    TuttiQuesiti[i] = finestra.QuesitoCreato;
                    CaricaQuesitiQuiz();
                }
                else
                {
                    MessageBox.Show("Modifiche annullate", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Information);

                }
            }
        }

        // Funzione per richiedere la sostituzione dei quesiti tramite IA o DB
        private void Reroll_Click(object sender, RoutedEventArgs e)
        {
            _indice_quesiti.Clear();
            _id_quesiti.Clear();
            _indici_non_sostituiti.Clear();

            //Crea una nuova finestra che ospita la pagina per la creazione del quiz  
            foreach (var que in QuesitiQuiz)
            {
                // Se il quesito è selezionato
                if (que.Selezionato)
                {
                    _indice_quesiti.Add(QuesitiQuiz.IndexOf(que));
                }
                _id_quesiti.Add(que.ID); //a prescindere che sia selezionato o meno, aggiungo l'id del quesito alla lista
            }

            if (_indice_quesiti.Count == 0)
            {
                MessageBox.Show("Devi selezionare almeno un quesito per il reroll.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Crea una nuova pagina per la creazione del quiz
            // questa è ospitata all'interno di una finestra di dialogo

            //Distinguo in base alla modalità di quiz (Manuale o Random) se il reroll
            // possa essere fatto tramite AI o DB o solo AI
            if (_qm == quizMode.Manuale)
            {
                _mode = Mode.Reroll_AI;
            }
            else if (_qm == quizMode.Random)
            {
                _mode = Mode.Reroll_AI_DB;
            }

            var paginaQuiz = new CreateQuizPage(_mainFrame, _jwtToken, _mode, _id_quesiti, _indice_quesiti.Count());
            var finestraQuiz = new Window
            {
                SizeToContent = SizeToContent.WidthAndHeight
            };
            finestraQuiz.Content = paginaQuiz;

            // Dopo la chiusura della finestra, ricarica i quesiti
            if (finestraQuiz.ShowDialog() == true)
            {

                foreach (Quesito q in paginaQuiz.Quesiti_da_sostituire)
                {
                    foreach (int i in _indice_quesiti)
                    {
                        // Aggiungi il quesito creato alla lista dei quesiti del quiz
                        if (i < QuesitiQuiz.Count) // Verifica che l'indice sia valido
                        {
                            if (!QuesitiQuiz.Any(x => x.ID == q.ID))
                            {
                                QuesitiQuiz[i] = q; // Sostituisci il quesito esistente con quello nuovo 
                            }
                            else
                            {
                                _indici_non_sostituiti.Add(i);
                            }
                        }
                    }
                }
            }
            // Se non sono stati sostituiti quesiti, mostra un messaggio
            if (_indici_non_sostituiti.Count > 0)
            {
                StringBuilder sb = new StringBuilder("I seguenti quesiti non sono stati sostituiti poiché già presenti:\n");
                foreach (int i in _indici_non_sostituiti)
                {
                    sb.AppendLine($"- {QuesitiQuiz[i].Testo}");
                }
                MessageBox.Show(sb.ToString(), "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                MessageBox.Show("Amplia la tua Area Quesiti o genera i quesiti con AI!", "Informazione", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }
        // Metodo per selezionare tutti i quesiti nella griglia
        private void SelezionaTutteLeCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox headerCheckBox)
            {
                bool seleziona = headerCheckBox.IsChecked == true;

                // Imposta la proprietà Selezionato di tutti i quesiti mostrati nella griglia
                foreach (var que in QuesitiQuiz)
                {
                    que.Selezionato = seleziona;
                }

                // Aggiorna la DataGrid (notifica il binding)
                // QuesitiQuizGrid.Items.Refresh();
            }
        }
        // Metodo per tornare indietro
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Perderai tutte le modifiche correnti. Vuoi procedere?", "Conferma", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (NavigationService != null && NavigationService.CanGoBack)
                {
                    if (_qm == quizMode.Manuale)
                    {   
                        // Torna alla pagina di scelta della modalità quiz
                        _mainFrame.Navigate(new ChooseQuizModePage(_mainFrame, _jwtToken)); 
                    }
                    else if (_qm == quizMode.Random)
                    {
                        // Se il quiz è stato generato in modalità Random, torna alla pagina di creazione del quiz
                        _mainFrame.Navigate(new CreateQuizPage(_mainFrame, _jwtToken, Mode.Default));

                    }
                    else  NavigationService.GoBack();

                }

            }
        }
    }
}
