using QuizClient.Services;
using QuizClient;
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
        }

        private void CreateQuiz_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Navigate(new CreateQuizPage(_mainFrame, _jwtToken));
        }

        private void ViewSessions_Click(object sender, RoutedEventArgs e)
        {
            //_mainFrame.Navigate(new ViewSessionsPage(_mainFrame, _jwtToken));
        }

        private void JoinSession_Click(object sender, RoutedEventArgs e)
        {
            // Naviga a una pagina per unirsi a sessione (da implementare)
            //MessageBox.Show("Naviga a Unisciti alla Sessione (token disponibile)");
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
