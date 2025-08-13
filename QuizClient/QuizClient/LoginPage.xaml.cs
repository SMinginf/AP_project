using QuizClient.Services;
using QuizClient.Utils;
using System.Windows;
using System.Windows.Controls;

namespace QuizClient
{
    public partial class LoginPage : Page
    {
        private Frame _mainFrame;
        private readonly AuthService _authService = new AuthService();

        public LoginPage(Frame mainFrame)
        {
            InitializeComponent();
            _mainFrame = mainFrame;
        }

        private async void Login_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            
            string email = EmailBox.Text; 
            string password = PasswordBox.Password; 

            var token = await _authService.LoginAsync(email, password);
            if (token != null)
            {
                // Salva il token in SessionManager
                SessionManager.AccessToken = token;

                MessageBox.Show($"Login riuscito!\n Token: {token}", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Naviga alla prossima pagina (es. lobby), eventualmente passando il token
                _mainFrame.Navigate(new LobbyPage(_mainFrame, token));
            }
            else
            {
                MessageBox.Show("Credenziali non valide.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Register_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _mainFrame.Navigate(new RegisterPage(_mainFrame));
        }
    }
}