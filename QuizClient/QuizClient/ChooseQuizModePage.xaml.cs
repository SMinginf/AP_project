using System.Windows;
using System.Windows.Controls;
using QuizClient.Models;
using QuizClient.Utils;

namespace QuizClient
{
    public partial class ChooseQuizModePage : Page
    {
        private readonly string _jwtToken;

        public ChooseQuizModePage(string jwtToken)
        {
            InitializeComponent();
            _jwtToken = jwtToken;
            JwtUtils.ValidateDocenteRole(_jwtToken, NavigationService, this);
        }

        private void ManualeButton_Click(object sender, RoutedEventArgs e)
        {
            // Naviga alla pagina di gestione dei quiz (quiz inizialmente vuoto)
            NavigationService?.Navigate(new QuizManagerPage(_jwtToken, new Quiz()));
        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            // Naviga alla pagina di creazione del quiz
            NavigationService?.Navigate(new CreateQuizPage(_jwtToken, Mode.Default));
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LobbyPage(_jwtToken));
        }
    }
}
