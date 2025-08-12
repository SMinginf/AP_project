using QuizClient.Services;
using QuizClient.Utils;
<<<<<<< HEAD
=======
using System;
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
using System.Windows;
using System.Windows.Controls;

namespace QuizClient
{
    public partial class RegisterPage : Page
    {
<<<<<<< HEAD
        private Frame _mainFrame;
        private readonly AuthService _authService = new AuthService();

        public RegisterPage(Frame mainFrame)
        {
            InitializeComponent();
            _mainFrame = mainFrame;
=======
        private readonly AuthService _authService = new AuthService();

        public RegisterPage()
        {
            InitializeComponent();
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
        }

        private async void RegisterUser_ClickAsync(object sender, RoutedEventArgs e)
        {
            string ruolo = ((ComboBoxItem)RoleComboBox.SelectedItem)?.Content.ToString() ?? string.Empty;

            string username = NicknameBox.Text;
            string email = EmailBox.Text;
            string nome = NameBox.Text;
            string cognome = SurnameBox.Text;
<<<<<<< HEAD

            /*
            La proprietà Content di un oggetto ComboBoxItem potrebbe essere null. Quando si tenta di chiamare il metodo ToString() 
            su un valore null, si genera un errore. Ho aggiunto l'operatore di coalescenza nulla (??) per fornire un valore predefinito 
            (string.Empty) nel caso in cui Content o il risultato di ToString() siano null. 
            Questo garantisce che la variabile genere non contenga mai un valore null.

            la proprietà SelectedItem di una ComboBox restituisce un oggetto di tipo object.
            In questo caso gli elementi della ComboBox sono di tipo ComboBoxItem, quindi faccio il casting per accedere alla proprietà Content.

            ? = operatore di accesso condizionale -> se il risultato del cast non è null, allora accedo alla proprietà Content, altrimenti restituisci null.

             */
=======
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
            string genere = ((ComboBoxItem)GenderComboBox.SelectedItem)?.Content.ToString() ?? string.Empty;
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            string data_nascita = BirthDatePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? string.Empty;

            if (password != confirmPassword)
            {
                MessageBox.Show("Le password non coincidono.");
                return;
            }

<<<<<<< HEAD
            // Qui andrebbe inviata la richiesta al servizio di registrazione (da aggiungere)
=======
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
            try
            {
                var (success, message) = await _authService.RegisterAsync(ruolo, username, email, nome, cognome, genere, password, data_nascita);
                if (success)
                {
                    MessageBox.Show(message, "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
<<<<<<< HEAD
                    _mainFrame.Navigate(new LoginPage(_mainFrame));


=======
                    NavigationService?.Navigate(new LoginPage());
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
                }
                else
                {
                    MessageBox.Show(message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
<<<<<<< HEAD

=======
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la registrazione: {ex.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
<<<<<<< HEAD
            _mainFrame.Navigate(new LoginPage(_mainFrame));
=======
            NavigationService?.Navigate(new LoginPage());
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
        }
    }
}
