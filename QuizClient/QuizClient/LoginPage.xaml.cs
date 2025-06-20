using System.Windows.Controls;

namespace QuizApp
{
    public partial class LoginPage : Page
    {
        private Frame _mainFrame;

        public LoginPage(Frame mainFrame)
        {
            InitializeComponent();
            _mainFrame = mainFrame;
        }

        private void Login_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Logica di login futura
        }

        private void Register_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _mainFrame.Navigate(new RegisterPage(_mainFrame));
        }
    }
}