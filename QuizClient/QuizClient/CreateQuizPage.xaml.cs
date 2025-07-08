using QuizClient.Services;
using System.Windows;
using System.Windows.Controls;
using QuizClient.Models;
using QuizClient;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Globalization;
using QuizClient.Utils;

namespace QuizClient
{
    public partial class CreateQuizPage : Page
    {
        private Frame _mainFrame;
        private string _jwtToken;

        private List<Categoria> _categorieSelezionate = new List<Categoria>();
        private bool _unione = false; // Indica se le categorie selezionate devono essere unite o intersecate

        private readonly QuizService _quizService;


        public CreateQuizPage(Frame mainFrame, string jwtToken)
        {
            InitializeComponent();
            _mainFrame = mainFrame;
            _jwtToken = jwtToken;
            _quizService = new QuizService(jwtToken);
            
        }

        private async void CreateQuiz_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<int> idCategorieSelezionate = [.. _categorieSelezionate
                    .Where(c => c != null)
                    .Select(c => (int)c.ID)];

                int quantita = int.TryParse(NumQuestionsBox.Text, out int q) ? q : 0;
                string difficolta = ((ComboBoxItem)DifficultyBox.SelectedItem)?.Content.ToString() ?? "Qualsiasi";
                bool aiGenerated = AIGeneratedYes.IsChecked == true;
                string aiCategoria = aiGenerated ? AICategoryText.Text : string.Empty;

                ServiceResult<Quiz> result = await _quizService.CreateQuizAsync(
                    aiGenerated,
                    aiCategoria,
                    idCategorieSelezionate,
                    _unione,
                    difficolta,
                    quantita
                );

                if (result != null && result.Success && result.Data != null)
                {
                    MessageBox.Show("Quiz creato con successo!");
                    _mainFrame.Navigate(new QuizPage(result.Data));
                }
                else
                {
                    MessageBox.Show(result?.ErrorMessage ?? "Errore nella creazione del quiz.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AIGenerated_Checked(object sender, RoutedEventArgs e)
        {
            bool isAI = AIGeneratedYes.IsChecked == true;

            if (CategorieSelezionateLabel == null || CategoryBox == null) {
                return;
            }

            // Mostra/nasconde la label delle categorie
            CategorieSelezionateLabel.Visibility = isAI ? Visibility.Collapsed : Visibility.Visible;

            // Mostra/nasconde la textbox delle categorie
            CategoryBox.Visibility = isAI ? Visibility.Collapsed : Visibility.Visible;

            // Mostra/nasconde il pulsante "Scegli" (cerca tra i figli del DockPanel)
            if (CategoryBox.Parent is DockPanel dockPanel)
            {
                foreach (var child in dockPanel.Children)
                {
                    if (child is Button btn && btn.Content?.ToString() == "Scegli")
                        btn.Visibility = isAI ? Visibility.Collapsed : Visibility.Visible;
                }
            }

            // Mostra/nasconde il pannello per la categoria AI
            AICategoryPanel.Visibility = isAI ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.GoBack();
        }

        private void ApriSelezioneCategorie_Click(object sender, RoutedEventArgs e)
        {
            var selettore = new CategorySelectionWindow(_jwtToken);
            if (selettore.ShowDialog() == true && selettore.Selezionate != null)
            {
                _categorieSelezionate = selettore.Selezionate;
                _unione = selettore.Unione;
                CategoryBox.Text = string.Join(", ", _categorieSelezionate.Select(c => c.Nome));
            }
            else
            {
                CategoryBox.Text = string.Empty;
            }
        }
    }
}