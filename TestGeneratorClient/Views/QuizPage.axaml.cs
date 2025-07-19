using Avalonia.Controls;
using Avalonia.Interactivity;
using TestGeneratorClient.Models;

namespace TestGeneratorClient.Views;

public partial class QuizPage : UserControl
{
    private readonly MainWindow _mainWindow;
    private readonly string _jwt;
    private readonly Quiz _quiz;

    public QuizPage(Quiz quiz, string jwt, MainWindow mainWindow)
    {
        InitializeComponent();
        _quiz = quiz;
        _jwt = jwt;
        _mainWindow = mainWindow;
        TitleText.Text = quiz.Titolo;
    }

    private void Back_Click(object? sender, RoutedEventArgs e)
    {
        _mainWindow.Navigate(new LobbyPage(_mainWindow, _jwt));
    }
}
