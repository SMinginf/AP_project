using QuizClient.Services;
using QuizClient.Utils;
using System;
using System.Windows;
using System.Windows.Controls;

namespace QuizClient
{
    public partial class RegisterPage : Page
    {
        private readonly AuthService _authService = new AuthService();

        public RegisterPage()
        {
            InitializeComponent();
        }

        private async void RegisterUser_ClickAsync(object sender, RoutedEventArgs e)
        {
            string ruolo = ((ComboBoxItem)RoleComboBox.SelectedItem)?.Content.ToString() ?? string.Empty;

            string username = NicknameBox.Text;
            string email = EmailBox.Text;
            string nome = NameBox.Text;
            string cognome = SurnameBox.Text;
            string genere = ((ComboBoxItem)GenderComboBox.SelectedItem)?.Content.ToString() ?? string.Empty;
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            string data_nascita = BirthDatePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? string.Empty;

            if (password != confirmPassword)
            {
                MessageBox.Show("Le password non coincidono.");
                return;
            }

            try
            {
                var (success, message) = await _authService.RegisterAsync(ruolo, username, email, nome, cognome, genere, password, data_nascita);
                if (success)
                {
                    MessageBox.Show(message, "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService?.Navigate(new LoginPage());
                }
                else
                {
                    MessageBox.Show(message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la registrazione: {ex.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LoginPage());
        }
    }
}
