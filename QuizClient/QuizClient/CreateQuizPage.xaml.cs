using QuizClient;
using QuizClient.Models;
using QuizClient.Services;
using QuizClient.Utils;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace QuizClient
{
    public partial class CreateQuizPage : Page
    {
        private readonly Frame _mainFrame;
        private readonly string _jwtToken;
        private string _ruolo = "";
        private Mode _mode = Mode.Default;
        private bool _unione = false; // Indica se le categorie selezionate devono essere unite o intersecate
      
        
        private List<Categoria> _categorieSelezionate = new List<Categoria>();
        private List<uint> _id_quesiti = new List<uint>();
        
        private readonly QuizService _quizService;
        public int Num_domande { get; set; }
        public List<Quesito> Quesiti_da_sostituire { get; private set;}

        public CreateQuizPage(Frame mainFrame, string jwtToken, Mode m, List<uint> id_quesiti, int num_domande)
        {
            InitializeComponent();
            _mainFrame = mainFrame;
            _jwtToken = jwtToken;
            _id_quesiti = id_quesiti;
            _mode = m;
            this.Num_domande = num_domande;
            this.Quesiti_da_sostituire = new List<Quesito>();
            _quizService = new QuizService(jwtToken);

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
       
        //Overload del costruttore per la modalità di creazione quiz senza ID quesiti
        public CreateQuizPage(Frame mainFrame, string jwtToken, Mode oc) : this(mainFrame, jwtToken, oc, new List<uint>(), 0) { }
        private async void CreateQuiz_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                List<int> idCategorieSelezionate = [.. _categorieSelezionate
                    .Where(c => c != null)
                    .Select(c => (int)c.ID)];

                int quantita = int.TryParse(NumQuestionsBox.Text, out int q) ? q : 0;
                string difficolta = ((ComboBoxItem)DifficultyBox.SelectedItem)?.Content.ToString() ?? "Qualsiasi";
                bool aiGenerated = AIGeneratedYes.IsChecked == true;
                string aiCategoria = aiGenerated ? AICategoryText.Text : string.Empty;
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

                // Gestione result 
                if (result != null && result.Success && result.Data != null)
                {
                    if (_ruolo == "Studente")
                    {
                        MessageBox.Show("Quiz creato con successo!");
                        _mainFrame.Navigate(new QuizPage(result.Data, _jwtToken));
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
                        else _mainFrame.Navigate(new QuizManagerPage(_mainFrame, _jwtToken, result.Data));
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

            if (CategorieSelezionateLabel == null || CategoryBox == null) {
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
            if (_mode == Mode.Reroll_AI || _mode == Mode.Reroll_AI_DB)
            {
                Window parentWindow = Window.GetWindow(this);
                parentWindow.DialogResult = false;
            }
            else _mainFrame.GoBack();
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