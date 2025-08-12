using QuizClient;
using QuizClient.Services;
using QuizClient.Utils;
using System.Windows;
using System.Windows.Controls;
using QuizClient.Models;

namespace QuizClient
{
    public partial class LobbyPage : Page
    {
        private readonly Frame _mainFrame;
        private readonly string _jwtToken;
        private readonly string ruolo = "";

        public LobbyPage(Frame mainFrame, string jwtToken)
        {
            InitializeComponent();
            _mainFrame = mainFrame;
            _jwtToken = jwtToken;

            // Recupera il ruolo dal token JWT
            try
            {
                ruolo = JwtUtils.GetClaimAsString(_jwtToken, "ruolo");
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

            // Mostra/nascondi i pulsanti in base al ruolo
            QuizButton.Content = ruolo == "Docente" ? "Crea nuovo Quiz" : "Svolgi un Quiz";
            GestisciCategorieButton.Visibility = ruolo == "Docente" ? Visibility.Visible : Visibility.Collapsed;
            GestisciQuesitiButton.Visibility = ruolo == "Docente" ? Visibility.Visible : Visibility.Collapsed;
        }
        private void CreateQuiz_Click(object sender, RoutedEventArgs e)
        {
            if (ruolo == "Studente") {
                _mainFrame.Navigate(new CreateQuizPage(_mainFrame, _jwtToken, Mode.Default));
                }
            else if(ruolo == "Docente")
            {
               _mainFrame.Navigate(new ChooseQuizModePage(_mainFrame, _jwtToken));
            }
        }
        private void ManageCategories_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Navigate(new CategoriesPage(_jwtToken));
        }
        private void ManageQuestions_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Navigate(new QuestionsPage(_jwtToken));
        }  
        private void ProfileStats_Click(object sender, RoutedEventArgs e)
        {
            // Naviga a pagina statistiche (da implementare)
            MessageBox.Show("Naviga a Statistiche Profilo (token disponibile)");
        }
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Torna alla pagina di login
            _mainFrame.Navigate(new LoginPage(_mainFrame));
        }
    }
}
