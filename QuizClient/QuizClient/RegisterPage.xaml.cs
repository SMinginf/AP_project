using System.Windows;
using System.Windows.Controls;

namespace QuizApp
{
    public partial class RegisterPage : Page
    {
        private Frame _mainFrame;

        public RegisterPage(Frame mainFrame)
        {
            InitializeComponent();
            _mainFrame = mainFrame;
        }

        private void RegisterUser_Click(object sender, RoutedEventArgs e)
        {
            string nickname = NicknameBox.Text;
            string email = EmailBox.Text;
            string nome = NameBox.Text;
            string cognome = SurnameBox.Text;
            
            /*
            La proprietà Content di un oggetto ComboBoxItem potrebbe essere null. Quando si tenta di chiamare il metodo ToString() 
            su un valore null, si genera un errore. Ho aggiunto l'operatore di coalescenza nulla (??) per fornire un valore predefinito 
            (string.Empty) nel caso in cui Content o il risultato di ToString() siano null. 
            Questo garantisce che la variabile sesso non contenga mai un valore null.

             */
            string sesso = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? string.Empty; 
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            string birthDate = BirthDatePicker.SelectedDate?.ToShortDateString() ?? string.Empty;

            if (password != confirmPassword)
            {
                MessageBox.Show("Le password non coincidono.");
                return;
            }

            // Qui andrebbe inviata la richiesta al servizio di registrazione (da aggiungere)
            MessageBox.Show("Registrazione completata per: " + nickname);

            _mainFrame.Navigate(new LoginPage(_mainFrame));
        }
    }
}
