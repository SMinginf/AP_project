using QuizClient.Services;
using QuizClient.Utils;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QuizClient
{
    public partial class LoginPage : Page, IDisposable
    {
        private readonly AuthService _authService;
        private bool _disposed;
        public LoginPage()
        {
            InitializeComponent();
            _authService = new AuthService();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Inserisci email e password.");
                return;
            }

            // Per evitare doppi click
            LoginButton.IsEnabled = false;
            RegisterButton.IsEnabled = false;

            try
            {
                var result = await _authService.LoginAsync(email, password);

                if (result.Success && !string.IsNullOrWhiteSpace(result.Data))
                {
                    SessionManager.SetToken(result.Data);
                    ShowInfo("Login riuscito!");
                    NavigationService?.Navigate(new LobbyPage(result.Data));
                }
                else
                {
                    ShowError(result.ErrorMessage ?? "Credenziali non valide.");
                }
            }
            catch (HttpRequestException)
            {
                ShowError("Impossibile connettersi al server. Riprova più tardi.");
            }
            catch (Exception ex)
            {
                ShowError($"Errore imprevisto: {ex.Message}");
            }
            finally
            {
                LoginButton.IsEnabled = true;
                RegisterButton.IsEnabled = true;
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegisterPage());
        }

        private static void ShowInfo(string message) => MessageBox.Show(message, "Successo", MessageBoxButton.OK, MessageBoxImage.Information);

        private static void ShowError(string message) => MessageBox.Show(message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error);


        public void Dispose()
        {
            // Best practice: controllo con flag per garantire idempotenza
            if (_disposed)
                return;
            _authService.Dispose();
            _disposed = true;

            // Sopprime la finalizzazione se non è necessaria (buona pratica):
            // permette di saltare l'esecuzione del finalizzatore, poichè le risorse critiche sono già
            // state rilasciate, e pulire la sua memoria nel modo più veloce possibile.
            GC.SuppressFinalize(this);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Chiama il metodo Dispose() quando la pagina non è più visualizzata
            // Questa è la chiave per il rilascio di _authService.HttpClient.
            (this as IDisposable)?.Dispose();
        }
    }
}
