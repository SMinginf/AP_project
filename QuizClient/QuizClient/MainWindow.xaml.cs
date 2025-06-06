using System.Windows;

namespace QuizApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new LoginPage(MainFrame));
        }
    }
}