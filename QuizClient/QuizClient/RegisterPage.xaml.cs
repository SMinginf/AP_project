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
            /*
               La proprietà Content di un oggetto ComboBoxItem potrebbe essere null. Quando si tenta di chiamare il metodo ToString() 
               su un valore null, si genera un errore. Ho aggiunto l'operatore di coalescenza nulla (??) per fornire un valore predefinito 
               (string.Empty) nel caso in cui Content o il risultato di ToString() siano null. 
               Questo garantisce che la variabile genere non contenga mai un valore null.

               la proprietà SelectedItem di una ComboBox restituisce un oggetto di tipo object.
               In questo caso gli elementi della ComboBox sono di tipo ComboBoxItem, quindi faccio il casting per accedere alla proprietà Content.

               ? = operatore di accesso condizionale -> se il risultato del cast non è null, allora accedo alla proprietà Content, altrimenti restituisci null.

            */
            if (password != confirmPassword)
            {
                MessageBox.Show("Le password non coincidono.");
                return;
            }

           
            var result = await _authService.RegisterAsync(ruolo, username, email, nome, cognome, genere, password, data_nascita);
            if (result.Success && result.Data != null)
            {
                  MessageBox.Show(result.Data, "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                  NavigationService.Navigate(new LoginPage());
            }
            else
            {
                  MessageBox.Show(result.ErrorMessage ?? "Errore durante la registrazione dell'utente", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
            
 

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
