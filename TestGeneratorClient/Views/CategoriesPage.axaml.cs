using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TestGeneratorClient.Views;

public partial class CategoriesPage : UserControl
{
    private readonly string _jwtToken;
    private readonly MainWindow _mainWindow;

    public CategoriesPage(string jwtToken, MainWindow mainWindow)
    {
        InitializeComponent();
        _jwtToken = jwtToken;
        _mainWindow = mainWindow;
    }

    private void BackToLobby_Click(object? sender, RoutedEventArgs e)
    {
        _mainWindow.Navigate(new LobbyPage(_mainWindow, _jwtToken));
    }
}
