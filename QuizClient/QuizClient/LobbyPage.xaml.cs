using QuizClient;
using QuizClient.Services;
using QuizClient.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace QuizClient
{
    public partial class LobbyPage : Page
    {
        private string _jwtToken;
        private string _ruolo="";

        public LobbyPage(string jwtToken)
        {
            InitializeComponent();
            _jwtToken = jwtToken;
            string username;

            try
            {
                _ruolo = JwtUtils.GetClaimAsString(_jwtToken, "ruolo");
                username = JwtUtils.GetClaimAsString(jwtToken, "username");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel recupero dati dal token JWT: {ex.Message}", "Errore JWT", MessageBoxButton.OK, MessageBoxImage.Error);
                if (NavigationService != null && NavigationService.CanGoBack)
                    NavigationService.GoBack();
                else
                    this.IsEnabled = false;
                return;
            }

            TitoloTextBox.Text = $"Benvenuto, {username}";
            GestisciCategorieButton.Visibility = _ruolo == "Docente" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CreateQuiz_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CreateQuizPage(_jwtToken));
        }

        private void ManageCategories_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CategoriesPage(_jwtToken));
        }

        private void ProfileStats_Click(object sender, RoutedEventArgs e)
        {
            if (_ruolo == "Docente")
                NavigationService?.Navigate(new TeacherStats(_jwtToken));
            else if (_ruolo == "Studente")
                NavigationService?.Navigate(new StudentStats(_jwtToken));
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LoginPage());
        }
    }
}
