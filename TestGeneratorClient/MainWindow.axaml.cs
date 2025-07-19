using Avalonia.Controls;

namespace TestGeneratorClient;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MainContent.Content = new Views.LoginPage(this);
    }

    public void Navigate(Control control)
    {
        MainContent.Content = control;
    }
}
