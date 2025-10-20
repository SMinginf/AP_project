/*
using QuizClient.Services;
using System.Windows;
using System.Windows.Controls;
using QuizClient.Models;
using System.Collections.Generic;
using System.Linq;
using QuizClient.Utils;

namespace QuizClient
{
    public partial class CreateQuizPage : Page
    {
        private readonly string _jwtToken;
        private readonly QuizService _quizService;

        private List<Categoria> _categorieSelezionate = new();
        private bool _unione = false; // Indica se le categorie selezionate devono essere unite o intersecate

        // Aggiunto SQ
        private string _ruolo = "";
        private Mode _mode = Mode.Default;
        private List<uint> _id_quesiti = new List<uint>(); // quesiti per il reroll
        public int Num_domande { get; set; }
        public List<Quesito> Quesiti_da_sostituire { get; private set; }



        public CreateQuizPage(string jwtToken, Mode m, List<uint> id_quesiti, int num_domande)
        {
            InitializeComponent();
            _jwtToken = jwtToken;
            _quizService = new QuizService(jwtToken);

            // SQ
            _id_quesiti = id_quesiti;
            _mode = m;
            Num_domande = num_domande;
            Quesiti_da_sostituire = new List<Quesito>();
            // Recupera il ruolo dal token JWT
            try
            {
                _ruolo = JwtUtils.GetClaimAsString(_jwtToken, "ruolo");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel recupero del ruolo utente dal token JWT: {ex.Message}", "Errore JWT", MessageBoxButton.OK, MessageBoxImage.Error);

                // Torna indietro o chiudi la pagina
                if (NavigationService != null && NavigationService.CanGoBack)
                    NavigationService.GoBack();
                else
                    this.IsEnabled = false;
                return;
            }

            // Modifica l'interfaccia e il comportamento in base all provenienza della pagina
            switch (_mode)
            {
                case Mode.Reroll_AI_DB:
                    CreaButton.Content = "Sostituisci";
                    NumQuestionsBox.Text = Num_domande.ToString();
                    NumQuestionsBox.IsReadOnly = true; // Disabilita la modifica del numero di domande
                    TitleTextBlock.Text = "Sostituisci quesiti";
                    DescriptionTextBlock.Text = "Definisci i parametri dei nuovi quesiti:";
                    AITextBlock.Text = "Vuoi che siano generati con l'AI?\n(altrimenti verranno presi random dall'Area Quesiti): ";
                    break;
                case Mode.Reroll_AI:
                    CreaButton.Content = "Sostituisci";
                    NumQuestionsBox.Text = Num_domande.ToString();
                    NumQuestionsBox.IsReadOnly = true;
                    TitleTextBlock.Text = "Sostituisci quesiti";
                    DescriptionTextBlock.Text = "Definisci i parametri dei nuovi quesiti";
                    AITextBlock.Text = "I quesiti verranno generati con AI";
                    AIGeneratedNo.Visibility = Visibility.Collapsed;
                    AIGeneratedYes.Visibility = Visibility.Collapsed;
                    AIGeneratedYes.IsChecked = true;
                    // da nascondere l'opzione di reroll da DB solo se siamo in QuizManagerPage e veniamo da manuale
                    // si risolve con enum
                    break;
                default:
                    CreaButton.Content = "Crea Quiz";
                    TitleTextBlock.Text = "Crea quiz";
                    DescriptionTextBlock.Text = "Compila i campi per generare un nuovo quiz:";
                    AITextBlock.Text = "Vuoi che il quiz sia generato dall'AI?";
                    break;
            }

        }

        //Overload del costruttore per la modalità di creazione quiz senza ID quesiti (senza reroll)
        public CreateQuizPage(string jwtToken, Mode m) : this (jwtToken, m, new List<uint>(), 0) { }

        private async void CreateQuiz_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<uint> idCategorieSelezionate = [.. _categorieSelezionate
                    .Where(c => c != null)
                    .Select(c => c.ID)];

                int quantita = int.TryParse(NumQuestionsBox.Text, out int q) ? q : 0;
                string difficolta = ((ComboBoxItem)DifficultyBox.SelectedItem)?.Content.ToString() ?? "Qualsiasi";
                bool aiGenerated = AIGeneratedYes.IsChecked == true;
                string aiCategoria = aiGenerated ? AICategoryBox.Text : string.Empty;
                List<uint> idQuesitiSelezionati = _id_quesiti; //se siamo in modalità reroll


                ServiceResult<Quiz> result = await _quizService.CreateQuizAsync(
                    aiGenerated,
                    aiCategoria,
                    idCategorieSelezionate,
                    _unione,
                    difficolta,
                    quantita,
                    idQuesitiSelezionati
                );

                //    if (result != null && result.Success && result.Data != null)
                //    {
                //        MessageBox.Show("Quiz creato con successo!");
                //        NavigationService?.Navigate(new QuizPage(result.Data, _jwtToken));
                //    }
                //    else
                //    {
                //        MessageBox.Show(result?.ErrorMessage ?? "Errore nella creazione del quiz.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                //    }

                // Gestione result 
                if (result != null && result.Success && result.Data != null)
                {
                    if (_ruolo == "Studente")
                    {
                        MessageBox.Show("Quiz creato con successo!");
                        NavigationService.Navigate(new QuizPage(result.Data, _jwtToken));
                    }
                    else //Docente
                    {
                        if (_mode == Mode.Reroll_AI || _mode == Mode.Reroll_AI_DB)
                        {
                            Window parentWindow = Window.GetWindow(this);
                            // Se siamo nella pagina di gestione quiz,
                            // sostituiamo i quesiti accessibili all'esterno 
                            Quesiti_da_sostituire = result.Data.Quesiti;

                            if (Quesiti_da_sostituire.Count == 0)
                            {
                                MessageBox.Show("Nessun quesito creato. Assicurati di aver selezionato almeno una categoria o di aver impostato l'AI.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                                parentWindow.DialogResult = false;
                            }
                            else
                            {
                                parentWindow.DialogResult = true;
                            }
                        }

                        //Comportamento base se vengo da selezione random
                        else NavigationService.Navigate(new QuizManagerPage(_jwtToken, result.Data));
                    }
                }
                else
                {
                    MessageBox.Show(result?.ErrorMessage ?? "Errore nella creazione del quiz.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        private void AIGenerated_Checked(object sender, RoutedEventArgs e)
        {
            bool isAI = AIGeneratedYes.IsChecked == true;

            if (CategorieSelezionateLabel == null || CategoryBox == null)
            {
                return;
            }

            // Mostra/nasconde la label delle categorie
            CategorieSelezionateLabel.Visibility = isAI ? Visibility.Collapsed : Visibility.Visible;

            // Mostra/nasconde la textbox delle categorie
            CategoryBox.Visibility = isAI ? Visibility.Collapsed : Visibility.Visible;

            // Mostra/nasconde il pulsante "Scegli" (cerca tra i figli del DockPanel)
            if (CategoryBox.Parent is DockPanel dockPanel)
            {
                foreach (var child in dockPanel.Children)
                {
                    if (child is Button btn && btn.Content?.ToString() == "Scegli")
                        btn.Visibility = isAI ? Visibility.Collapsed : Visibility.Visible;
                }
            }

            // Mostra/nasconde il pannello per la categoria AI
            AICategoryPanel.Visibility = isAI ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            //SQ
            // Se siamo in modalità Reroll_AI o Reroll_AI_DB, chiudiamo la finestra
            // altrimenti torniamo indietro nella navigazione
            if (_mode == Mode.Reroll_AI || _mode == Mode.Reroll_AI_DB)
            {
                Window parentWindow = Window.GetWindow(this);
                parentWindow.DialogResult = false;
            }
            else 
                NavigationService.Navigate(new LobbyPage(_jwtToken));
        }

        private void ApriSelezioneCategorie_Click(object sender, RoutedEventArgs e)
        {
            var selettore = new CategorySelectionWindow(_jwtToken, Mode.Default);
            if (selettore.ShowDialog() == true && selettore.Selezionate != null)
            {
                _categorieSelezionate = selettore.Selezionate;
                _unione = selettore.Unione;
                CategoryBox.Text = string.Join(", ", _categorieSelezionate.Select(c => c.Nome));
            }
            else
            {
                CategoryBox.Text = string.Empty;
            }
        }
    }
}
*/

using QuizClient.Services;
using QuizClient.Models;
using QuizClient.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QuizClient
{
    public partial class CreateQuizPage : Page, IDisposable
    {
        private readonly string _jwtToken;
        private readonly QuizService _quizService;
        private readonly string _ruolo = "";
        private readonly Mode _mode;
        private readonly List<uint> _idQuesiti; // quesiti per il reroll

        private List<Categoria> _categorieSelezionate = new();
        private bool _unione = false; // Indica se le categorie selezionate devono essere unite o intersecate
        private bool _disposed;

        // Aggiunto SQ
        public int Num_domande { get; }
        public List<Quesito> Quesiti_da_sostituire { get; private set; }

        public CreateQuizPage(string jwtToken, Mode mode, List<uint> idQuesiti, int numDomande)
        {
            InitializeComponent();
            _jwtToken = jwtToken;
            _quizService = new QuizService(jwtToken);
            _idQuesiti = idQuesiti ?? new List<uint>();
            _mode = mode;
            Num_domande = numDomande;
            Quesiti_da_sostituire = new List<Quesito>();

            // Recupera il ruolo dal token JWT
            try
            {
                _ruolo = JwtUtils.GetClaimAsString(_jwtToken, "ruolo");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel recupero del ruolo utente dal token JWT: {ex.Message}", "Errore JWT", MessageBoxButton.OK, MessageBoxImage.Error);

                // Torna indietro o chiudi la pagina
                if (NavigationService?.CanGoBack == true)
                    NavigationService.GoBack();
                else
                    IsEnabled = false;
                return;
            }

            // Modifica l'interfaccia e il comportamento in base alla provenienza della pagina
            switch (_mode)
            {
                case Mode.Reroll_AI_DB:
                    CreaButton.Content = "Sostituisci";
                    NumQuestionsBox.Text = Num_domande.ToString();
                    NumQuestionsBox.IsReadOnly = true; // Disabilita la modifica del numero di domande
                    TitleTextBlock.Text = "Sostituisci quesiti";
                    DescriptionTextBlock.Text = "Definisci i parametri dei nuovi quesiti:";
                    AITextBlock.Text = "Vuoi che siano generati con l'AI?\n(altrimenti verranno presi random dall'Area Quesiti): ";
                    break;

                case Mode.Reroll_AI:
                    CreaButton.Content = "Sostituisci";
                    NumQuestionsBox.Text = Num_domande.ToString();
                    NumQuestionsBox.IsReadOnly = true;
                    TitleTextBlock.Text = "Sostituisci quesiti";
                    DescriptionTextBlock.Text = "Definisci i parametri dei nuovi quesiti";
                    AITextBlock.Text = "I quesiti verranno generati con AI";
                    AIGeneratedNo.Visibility = Visibility.Collapsed;
                    AIGeneratedYes.Visibility = Visibility.Collapsed;
                    AIGeneratedYes.IsChecked = true;
                    // da nascondere l'opzione di reroll da DB solo se siamo in QuizManagerPage e veniamo da manuale
                    // si risolve con enum
                    break;

                default:
                    CreaButton.Content = "Crea Quiz";
                    TitleTextBlock.Text = "Crea quiz";
                    DescriptionTextBlock.Text = "Compila i campi per generare un nuovo quiz:";
                    AITextBlock.Text = "Vuoi che il quiz sia generato dall'AI?";
                    break;
            }
        }

        // Overload del costruttore per la modalità di creazione quiz senza ID quesiti (senza reroll)
        public CreateQuizPage(string jwtToken, Mode mode) : this(jwtToken, mode, new List<uint>(), 0) { }

        private async void CreateQuiz_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var idCategorieSelezionate = _categorieSelezionate
                    .Where(c => c != null)
                    .Select(c => c.ID)
                    .ToList();

                if (!int.TryParse(NumQuestionsBox.Text, out int quantita))
                    quantita = 0;

                string difficolta = (DifficultyBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Qualsiasi";
                bool aiGenerated = AIGeneratedYes.IsChecked == true;
                string aiCategoria = aiGenerated ? AICategoryBox.Text : string.Empty;

                // se siamo in modalità reroll
                var idQuesitiSelezionati = _idQuesiti;

                var result = await _quizService.CreateQuizAsync(
                    aiGenerated,
                    aiCategoria,
                    idCategorieSelezionate,
                    _unione,
                    difficolta,
                    quantita,
                    idQuesitiSelezionati
                );

                // Gestione result 
                if (result is { Success: true, Data: not null })
                {
                    if (_ruolo == "Studente")
                    {
                        MessageBox.Show("Quiz creato con successo!");
                        NavigationService?.Navigate(new QuizPage(result.Data, _jwtToken));
                    }
                    else // Docente
                    {
                        if (_mode == Mode.Reroll_AI || _mode == Mode.Reroll_AI_DB)
                        {
                            var parentWindow = Window.GetWindow(this);
                            // Se siamo nella pagina di gestione quiz,
                            // sostituiamo i quesiti accessibili all'esterno 
                            Quesiti_da_sostituire = result.Data.Quesiti ?? new List<Quesito>();

                            if (Quesiti_da_sostituire.Count == 0)
                            {
                                MessageBox.Show("Nessun quesito creato. Assicurati di aver selezionato almeno una categoria o di aver impostato l'AI.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                                if (parentWindow != null) parentWindow.DialogResult = false;
                            }
                            else
                            {
                                if (parentWindow != null) parentWindow.DialogResult = true;
                            }
                        }
                        else
                        {
                            // Comportamento base se vengo da selezione random
                            NavigationService?.Navigate(new QuizManagerPage(_jwtToken, result.Data));
                        }
                    }
                }
                else
                {
                    MessageBox.Show(result?.ErrorMessage ?? "Errore nella creazione del quiz.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AIGenerated_Checked(object sender, RoutedEventArgs e)
        {
            bool isAI = AIGeneratedYes.IsChecked == true;

            if (CategorieSelezionateLabel == null || CategoryBox == null)
                return;

            // Mostra/nasconde la label delle categorie
            CategorieSelezionateLabel.Visibility = isAI ? Visibility.Collapsed : Visibility.Visible;

            // Mostra/nasconde la textbox delle categorie
            CategoryBox.Visibility = isAI ? Visibility.Collapsed : Visibility.Visible;

            // Mostra/nasconde il pulsante "Scegli" (cerca tra i figli del DockPanel)
            if (CategoryBox.Parent is DockPanel dockPanel)
            {
                foreach (var child in dockPanel.Children.OfType<Button>())
                {
                    if (child.Content?.ToString() == "Scegli")
                        child.Visibility = isAI ? Visibility.Collapsed : Visibility.Visible;
                }
            }

            // Mostra/nasconde il pannello per la categoria AI
            AICategoryPanel.Visibility = isAI ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // SQ
            // Se siamo in modalità Reroll_AI o Reroll_AI_DB, chiudiamo la finestra
            // altrimenti torniamo indietro nella navigazione
            if (_mode == Mode.Reroll_AI || _mode == Mode.Reroll_AI_DB)
            {
                var parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                    parentWindow.DialogResult = false;
            }
            else
            {
                NavigationService?.Navigate(new LobbyPage(_jwtToken));
            }
        }

        private void ApriSelezioneCategorie_Click(object sender, RoutedEventArgs e)
        {
            var selettore = new CategorySelectionWindow(_jwtToken, Mode.Default);
            if (selettore.ShowDialog() == true && selettore.Selezionate != null)
            {
                _categorieSelezionate = selettore.Selezionate;
                _unione = selettore.Unione;
                CategoryBox.Text = string.Join(", ", _categorieSelezionate.Select(c => c.Nome));
            }
            else
            {
                CategoryBox.Text = string.Empty;
            }
        }

        public void Dispose()
        {
            // Best practice: controllo con flag per garantire idempotenza
            if (_disposed)
                return;
            _quizService.Dispose();
            _disposed = true;

            // Sopprime la finalizzazione se non è necessaria (buona pratica):
            // permette di saltare l'esecuzione del finalizzatore, poichè le risorse critiche sono già
            // state rilasciate, e pulire la sua memoria nel modo più veloce possibile.
            GC.SuppressFinalize(this);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Chiama il metodo Dispose() quando la pagina non è più visualizzata
            // Questa è la chiave per il rilascio di _quizService.HttpClient.
            (this as IDisposable)?.Dispose();
        }
    }
}
