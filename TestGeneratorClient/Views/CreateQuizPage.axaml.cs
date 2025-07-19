using Avalonia.Controls;
using Avalonia.Interactivity;
using TestGeneratorClient.Models;
using TestGeneratorClient.Services;
using TestGeneratorClient.Utils;

namespace TestGeneratorClient.Views;

public partial class CreateQuizPage : UserControl
{
    private readonly MainWindow _mainWindow;
    private readonly string _jwtToken;
    private readonly QuizService _quizService;
    private List<Categoria> _categorieSelezionate = new();
    private bool _unione;

    public CreateQuizPage(MainWindow mainWindow, string jwtToken)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        _jwtToken = jwtToken;
        _quizService = new QuizService(jwtToken);
    }

    private async void CreateQuiz_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var idCategorie = _categorieSelezionate.Where(c => c != null).Select(c => (int)c.ID).ToList();
            int quantita = int.TryParse(NumQuestionsBox.Text, out int q) ? q : 0;
            string difficolta = (DifficultyBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Qualsiasi";
            bool aiGenerated = AIGeneratedYes.IsChecked == true;
            string aiCategoria = aiGenerated ? AICategoryBox.Text : string.Empty;

            var result = await _quizService.CreateQuizAsync(aiGenerated, aiCategoria, idCategorie, _unione, difficolta, quantita);
            if (result != null && result.Success && result.Data != null)
            {
                await MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Successo", "Quiz creato con successo!").ShowDialog(_mainWindow);
                _mainWindow.Navigate(new QuizPage(result.Data, _jwtToken, _mainWindow));
            }
            else
            {
                await MessageBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandardWindow("Errore", result?.ErrorMessage ?? "Errore nella creazione del quiz.")
                    .ShowDialog(_mainWindow);
            }
        }
        catch (Exception ex)
        {
            await MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Errore", $"Errore: {ex.Message}").ShowDialog(_mainWindow);
        }
    }

    private void AIGenerated_Checked(object? sender, RoutedEventArgs e)
    {
        bool isAI = AIGeneratedYes.IsChecked == true;
        CategorieSelezionateLabel.IsVisible = !isAI;
        CategoryBox.IsVisible = !isAI;
        if (CategoryBox.Parent is DockPanel dp)
        {
            foreach (var child in dp.Children)
            {
                if (child is Button btn && (string?)btn.Content == "Scegli")
                    btn.IsVisible = !isAI;
            }
        }
        AICategoryPanel.IsVisible = isAI;
    }

    private void Back_Click(object? sender, RoutedEventArgs e)
    {
        _mainWindow.Navigate(new LobbyPage(_mainWindow, _jwtToken));
    }

    private void ApriSelezioneCategorie_Click(object? sender, RoutedEventArgs e)
    {
        var selector = new CategorySelectionWindow(_jwtToken);
        selector.ShowDialog(_mainWindow).ContinueWith(t =>
        {
            if (selector.DialogResult == true && selector.Selezionate != null)
            {
                _categorieSelezionate = selector.Selezionate;
                _unione = selector.Unione;
                CategoryBox.Text = string.Join(", ", _categorieSelezionate.Select(c => c.Nome));
            }
            else
            {
                CategoryBox.Text = string.Empty;
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }
}
