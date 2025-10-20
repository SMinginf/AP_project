using QuizClient.Models;
using QuizClient.Services;
using QuizClient.Utils;
using System;
using System.Windows;
using System.Windows.Controls;

namespace QuizClient
{
    public partial class LobbyPage : Page
    {
        private readonly string _jwtToken;
        private readonly string _role="";
        private readonly string _username="";

        public LobbyPage(string jwtToken)
        {
            InitializeComponent();
            _jwtToken = jwtToken;

            try
            {
                _role = JwtUtils.GetClaimAsString(_jwtToken, "ruolo");
                _username = JwtUtils.GetClaimAsString(_jwtToken, "username");
            }
            catch (Exception ex)
            {
                ShowError($"Errore nel recupero dati dal token JWT: {ex.Message}");
                DisableAndGoBack();
                return;
            }

            InitializeUI();
        }

        private void InitializeUI()
        {
            TitoloTextBox.Text = $"Benvenuto, {_username}";

            bool isTeacher = _role.Equals("Docente", StringComparison.OrdinalIgnoreCase);

            QuizButton.Content = isTeacher ? "Crea nuovo Quiz" : "Svolgi un Quiz";

            GestisciCategorieButton.Visibility = isTeacher ? Visibility.Visible : Visibility.Collapsed;
            GestisciQuesitiButton.Visibility = isTeacher ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CreateQuiz_Click(object sender, RoutedEventArgs e)
        {
            if (_role.Equals("Studente", StringComparison.OrdinalIgnoreCase))
                NavigationService.Navigate(new CreateQuizPage(_jwtToken, Mode.Default));
            else
                NavigationService.Navigate(new ChooseQuizModePage(_jwtToken));
        }

        private void ManageCategories_Click(object sender, RoutedEventArgs e) =>
            NavigationService.Navigate(new CategoriesPage(_jwtToken));

        private void ManageQuestions_Click(object sender, RoutedEventArgs e) =>
            NavigationService.Navigate(new QuestionsPage(_jwtToken));

        private void ProfileStats_Click(object sender, RoutedEventArgs e)
        {
            if (_role.Equals("Docente", StringComparison.OrdinalIgnoreCase))
                NavigationService.Navigate(new TeacherStats(_jwtToken));
            else
                NavigationService.Navigate(new StudentStats(_jwtToken));
        }

        private void Logout_Click(object sender, RoutedEventArgs e) {
            SessionManager.Clear();
            NavigationService.Navigate(new LoginPage());
        }
            

        // --- Helper methods ---
        private void DisableAndGoBack()
        {
            if (NavigationService.CanGoBack == true)
                NavigationService.GoBack();
            else
                IsEnabled = false;
        }

        private static void ShowError(string message) =>
            MessageBox.Show(message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
