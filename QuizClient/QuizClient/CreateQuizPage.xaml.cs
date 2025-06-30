using QuizClient.Services;
using System.Windows;
using System.Windows.Controls;
using QuizClient.Models;
using QuizClient;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Globalization;

namespace QuizClient
{
    public partial class CreateQuizPage : Page
    {
        private Frame _mainFrame;
        private string _jwtToken;

        private List<Categoria> _categorieSelezionate = new List<Categoria>();
        private bool _unione = false; // Indica se le categorie selezionate devono essere unite o intersecate

        private readonly QuizService _sessionService;


        public CreateQuizPage(Frame mainFrame, string jwtToken)
        {
            InitializeComponent();
            _mainFrame = mainFrame;
            _jwtToken = jwtToken;
            _sessionService = new QuizService(jwtToken);
            
        }

        private async void CreateQuiz_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Estrai la lista degli ID delle categorie selezionate
                // L’espressione di raccolta [.. sequence] in C# 12 (e .NET 8) è una collection expression.
                // Serve per creare una nuova collezione (ad esempio una List<T>, un array, ecc.) a partire da un’altra sequenza.
                List<int> idCategorieSelezionate = [.. _categorieSelezionate
                    .Where(c => c != null)
                    .Select(c => (int)c.ID)];

                int quantita = int.TryParse(NumQuestionsBox.Text, out int q) ? q : 0;
                string difficolta = ((ComboBoxItem)DifficultyBox.SelectedItem)?.Content.ToString() ?? "Qualsiasi";
                bool aiGenerated = AIGeneratedYes.IsChecked == true;
                string aiCategoria = aiGenerated ? AICategoryText.Text : string.Empty;

                

                Quiz? nuovoQuiz = await _sessionService.CreateQuizAsync(
                    aiGenerated,
                    aiCategoria,
                    idCategorieSelezionate,
                    _unione,
                    difficolta,
                    quantita
                );

                if (nuovoQuiz != null)
                {
                    MessageBox.Show("Quiz creato con successo !");
                    _mainFrame.Navigate(new QuizPage(nuovoQuiz));
                }
                else
                {
                    MessageBox.Show("Errore nella creazione della sessione.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore: {ex.Message}");
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