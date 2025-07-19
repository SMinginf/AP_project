using Avalonia.Controls;
using Avalonia.Interactivity;
using TestGeneratorClient.Services;

namespace TestGeneratorClient.Views;

public partial class LoginPage : UserControl
{
    private readonly MainWindow _mainWindow;
    private readonly AuthService _authService = new();

    public LoginPage(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
    }

    private async void Login_Click(object? sender, RoutedEventArgs e)
    {
        string email = EmailBox.Text ?? string.Empty;
        string password = PasswordBox.Text ?? string.Empty;

        var token = await _authService.LoginAsync(email, password);
        if (token != null)
        {
            Utils.SessionManager.AccessToken = token;
            _mainWindow.Navigate(new LobbyPage(_mainWindow, token));
        }
        else
        {
            await MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow("Errore", "Credenziali non valide.")
                .ShowDialog(_mainWindow);
        }
    }

    private void Register_Click(object? sender, RoutedEventArgs e)
    {
        _mainWindow.Navigate(new RegisterPage(_mainWindow));
    }
}
