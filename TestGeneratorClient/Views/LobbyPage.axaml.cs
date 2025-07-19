using Avalonia.Controls;
using Avalonia.Interactivity;
using TestGeneratorClient.Utils;

namespace TestGeneratorClient.Views;

public partial class LobbyPage : UserControl
{
    private readonly MainWindow _mainWindow;
    private readonly string _jwtToken;

    public LobbyPage(MainWindow mainWindow, string jwtToken)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        _jwtToken = jwtToken;

        string ruolo;
        try
        {
            ruolo = JwtUtils.GetClaimAsString(_jwtToken, "ruolo");
        }
        catch (Exception ex)
        {
            MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow("Errore JWT", $"Errore nel recupero del ruolo utente dal token JWT: {ex.Message}")
                .ShowDialog(_mainWindow);
            IsEnabled = false;
            return;
        }
        GestisciCategorieButton.IsVisible = ruolo == "Docente";
    }

    private void CreateQuiz_Click(object? sender, RoutedEventArgs e)
    {
        _mainWindow.Navigate(new CreateQuizPage(_mainWindow, _jwtToken));
    }

    private void ManageCategories_Click(object? sender, RoutedEventArgs e)
    {
        _mainWindow.Navigate(new CategoriesPage(_jwtToken, _mainWindow));
    }

    private void ProfileStats_Click(object? sender, RoutedEventArgs e)
    {
        _mainWindow.Navigate(new StudentStats(_jwtToken, _mainWindow));
    }

    private void Logout_Click(object? sender, RoutedEventArgs e)
    {
        _mainWindow.Navigate(new LoginPage(_mainWindow));
    }
}
