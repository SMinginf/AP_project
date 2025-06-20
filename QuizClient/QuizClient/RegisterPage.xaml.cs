using QuizApp.Services;
using QuizApp.Utils;
using System.Windows;
using System.Windows.Controls;

namespace QuizApp
{
    public partial class RegisterPage : Page
    {
        private Frame _mainFrame;
        private readonly AuthService _authService = new AuthService();

        public RegisterPage(Frame mainFrame)
        {
            InitializeComponent();
            _mainFrame = mainFrame;
        }

        private async void RegisterUser_ClickAsync(object sender, RoutedEventArgs e)
        {
            string username = NicknameBox.Text;
            string email = EmailBox.Text;
            string nome = NameBox.Text;
            string cognome = SurnameBox.Text;

            /*
            La proprietà Content di un oggetto ComboBoxItem potrebbe essere null. Quando si tenta di chiamare il metodo ToString() 
            su un valore null, si genera un errore. Ho aggiunto l'operatore di coalescenza nulla (??) per fornire un valore predefinito 
            (string.Empty) nel caso in cui Content o il risultato di ToString() siano null. 
            Questo garantisce che la variabile sesso non contenga mai un valore null.

             */
            string genere = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? string.Empty;
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            string data_nascita = BirthDatePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? string.Empty;

            if (password != confirmPassword)
            {
                MessageBox.Show("Le password non coincidono.");
                return;
            }

            // Qui andrebbe inviata la richiesta al servizio di registrazione (da aggiungere)
            try
            {
                var (success, message) = await _authService.RegisterAsync(username, email, nome, cognome, genere, password, data_nascita);
                if (success)
                {
                    MessageBox.Show(message, "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    _mainFrame.Navigate(new LoginPage(_mainFrame));


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
            _mainFrame.Navigate(new LoginPage(_mainFrame));
        }
    }
}
