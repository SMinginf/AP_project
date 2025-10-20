using QuizClient.Services;
using QuizClient.Utils;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QuizClient
{
    public partial class RegisterPage : Page
    {
        private readonly AuthService _authService;

        public RegisterPage()
        {
            InitializeComponent();
            _authService = new AuthService();
        }

        private async void RegisterUser_ClickAsync(object sender, RoutedEventArgs e)
        {
            string role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
            string username = NicknameBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string name = NameBox.Text.Trim();
            string surname = SurnameBox.Text.Trim();
            string gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            string birthDate = BirthDatePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? string.Empty;

            if (!ValidateInputs(username, email, password, confirmPassword))
                return;

            // Evito doppi click
            RegisterButton.IsEnabled = false;
            BackButton.IsEnabled = false;

            try
            {
                var result = await _authService.RegisterAsync(role, username, email, name, surname, gender, password, birthDate);

                if (result.Success && !string.IsNullOrEmpty(result.Data))
                {
                    ShowInfo(result.Data);
                    NavigationService?.Navigate(new LoginPage());
                }
                else
                {
                    ShowError(result.ErrorMessage ?? "Errore durante la registrazione dell'utente.");
                }
            }
            catch (HttpRequestException)
            {
                ShowError("Impossibile connettersi al server. Controlla la connessione e riprova.");
            }
            catch (Exception ex)
            {
                ShowError($"Errore imprevisto: {ex.Message}");
            }
            finally
            {
                RegisterButton.IsEnabled = true;
                BackButton.IsEnabled = true;
            }
        }

        private bool ValidateInputs(string username, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ShowError("Tutti i campi sono obbligatori.");
                return false;
            }

            if (password != confirmPassword)
            {
                ShowError("Le password non coincidono.");
                return false;
            }

            if (!email.Contains("@"))
            {
                ShowError("Inserisci un indirizzo email valido.");
                return false;
            }

            return true;
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private static void ShowInfo(string message) =>
            MessageBox.Show(message, "Successo", MessageBoxButton.OK, MessageBoxImage.Information);

        private static void ShowError(string message) =>
            MessageBox.Show(message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
