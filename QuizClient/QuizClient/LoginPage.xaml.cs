using QuizClient.Services;
using QuizClient.Utils;
using System.Windows;
using System.Windows.Controls;

namespace QuizClient
{
    public partial class LoginPage : Page
    {
        private readonly AuthService _authService = new AuthService();

        public LoginPage()
        {
            InitializeComponent();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailBox.Text;
            string password = PasswordBox.Password;

            var token = await _authService.LoginAsync(email, password);
            if (token != null)
            {
                SessionManager.AccessToken = token;

                MessageBox.Show($"Login riuscito!\nToken: {token}", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService?.Navigate(new LobbyPage(token));
            }
            else
            {
                MessageBox.Show("Credenziali non valide.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegisterPage());
        }
    }
}
