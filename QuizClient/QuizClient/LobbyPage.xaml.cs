using System.Windows;
using System.Windows.Controls;

namespace QuizApp
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

        private void CreateSession_Click(object sender, RoutedEventArgs e)
        {
            // Naviga a una pagina per creare sessione (da implementare)
            MessageBox.Show("Naviga a Crea Sessione (token disponibile)");
        }

        private void JoinSession_Click(object sender, RoutedEventArgs e)
        {
            // Naviga a una pagina per unirsi a sessione (da implementare)
            MessageBox.Show("Naviga a Unisciti alla Sessione (token disponibile)");
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
