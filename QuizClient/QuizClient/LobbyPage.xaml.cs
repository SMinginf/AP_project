using QuizClient;
using QuizClient.Services;
using QuizClient.Utils;
using System.Windows;
using System.Windows.Controls;

namespace QuizClient
{
    public partial class LobbyPage : Page
    {
        private Frame _mainFrame;
        private string _jwtToken;

        public LobbyPage(Frame mainFrame, string jwtToken)
        {
            InitializeComponent();
            _mainFrame = mainFrame;
            _jwtToken = jwtToken;
            // Recupera il ruolo dal token JWT
            string ruolo;
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
            GestisciCategorieButton.Visibility = ruolo == "Docente" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CreateQuiz_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Navigate(new CreateQuizPage(_mainFrame, _jwtToken));
        }

        private void ManageCategories_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Navigate(new CategoriesPage(_jwtToken));
        }

        private void JoinSession_Click(object sender, RoutedEventArgs e)
        {
            // Naviga a una pagina per unirsi a sessione (da implementare)
            //MessageBox.Show("Naviga a Unisciti alla Sessione (token disponibile)");
        }

        private void ProfileStats_Click(object sender, RoutedEventArgs e)
        {
            // Naviga a pagina statistiche (da implementare)
            _mainFrame.Navigate(new StudentStats(_jwtToken));
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Torna alla pagina di login
            _mainFrame.Navigate(new LoginPage(_mainFrame));
        }
    }
}
