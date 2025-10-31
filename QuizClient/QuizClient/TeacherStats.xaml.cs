
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using QuizClient.Models;
using QuizClient.Services;
using QuizClient.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QuizClient
{
    /// <summary>
    /// Logica di interazione per TeacherStats.xaml
    /// </summary>
    public partial class TeacherStats : Page, IDisposable
    {
        private readonly AnalyticsService _analyticsService;
        private readonly CRUDService _crudService;
        private readonly string _jwtToken; // Token JWT per autenticazione
        private bool _disposed;
        private bool _isEditing = false;

        private TeacherCategoryStatsResponse _stats = new();
        private TeacherGeneralStats? _generalStats;
        private List<Categoria> _categorieSelezionate = new();

        public TeacherStats(string jwtToken)
        {
            InitializeComponent();

            _analyticsService = new AnalyticsService(jwtToken);
            _crudService = new CRUDService(jwtToken);
            _jwtToken = jwtToken;

            LoadPersonalInfo();
            LoadGeneralStats();
        }

        /// <summary>
        /// Carica le informazioni personali del docente.
        /// </summary>
        private async void LoadPersonalInfo()
        {
            var result = await _crudService.GetUserAsync();
            if (result == null || !result.Success || result.Data == null)
            {
                MessageBox.Show($"Errore nel caricamento dei dati del docente: {result?.ErrorMessage}");
                return;
            }

            var user = result.Data;
            NameText.Text = user.Nome;
            CognomeText.Text = user.Cognome;
            EmailText.Text = user.Email;
            UsernameText.Text = user.Username;
            BirthDatePicker.SelectedDate = user.DataNascita;
            GenderText.Text = user.Genere;
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            _isEditing = true;
            SetEditingMode(true);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var updatedUser = new Utente
            {
                Nome = NameText.Text,
                Cognome = CognomeText.Text,
                Email = EmailText.Text,
                Username = UsernameText.Text,
                DataNascita = (BirthDatePicker.SelectedDate ?? DateTime.MinValue).ToUniversalTime(),
                Genere = GenderText.Text
            };

            var result = await _crudService.UpdateUserAsync(updatedUser);
            if (result?.Success == true)
            {
                MessageBox.Show("Dati aggiornati con successo!");
                _isEditing = false;
                SetEditingMode(false);
                LoadPersonalInfo();
            }
            else
            {
                MessageBox.Show($"Errore durante l'aggiornamento: {result?.ErrorMessage}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _isEditing = false;
            SetEditingMode(false);
            LoadPersonalInfo();
        }

        private void SetEditingMode(bool isEditing)
        {
            NameText.IsReadOnly = !isEditing;
            CognomeText.IsReadOnly = !isEditing;
            EmailText.IsReadOnly = !isEditing;
            UsernameText.IsReadOnly = !isEditing;
            BirthDatePicker.IsEnabled = isEditing;
            GenderText.IsReadOnly = !isEditing;

            SaveButton.Visibility = isEditing ? Visibility.Visible : Visibility.Collapsed;
            CancelButton.Visibility = isEditing ? Visibility.Visible : Visibility.Collapsed;
            EditButton.Visibility = !isEditing ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Carica le statistiche generali del docente.
        /// </summary>
        private async void LoadGeneralStats()
        {
            var result = await _analyticsService.GetTeacherGeneralStatsAsync();
            if (!result.Success || result.Data == null)
            {
                MessageBox.Show($"Errore nel caricamento delle statistiche generali del docente: {result?.ErrorMessage}");
                return;
            }

            _generalStats = result.Data;

            // Mostra il grafico e collega i dati al pannello
            ShowGeneralStatsChart(_generalStats.StatsPerCategoria);
            GeneralStatsPanel.DataContext = _generalStats;
        }

        /// <summary>
        /// Carica le statistiche per le categorie selezionate.
        /// </summary>
        private async void LoadStatsPerCategory()
        {
            var categoriaIds = _categorieSelezionate.Select(c => c.ID).ToList();
            var result = await _analyticsService.GetTeacherStatsPerCategoryAsync(categoriaIds);
            if (result == null || !result.Success || result.Data == null)
            {
                MessageBox.Show($"Errore nel caricamento delle statistiche: {result?.ErrorMessage}");
                return;
            }

            _stats = result.Data;

            // Mostra i grafici relativi
            ShowStatsPerDifficoltaChart(_stats.StatsPerDifficolta);
            ShowTop10Studenti(_stats.Top10Studenti);

            QuesitiSpeciali.Visibility = Visibility.Visible;
            QuesitiSpeciali.DataContext = _stats;
        }

        /// <summary>
        /// Apre la finestra di selezione categorie per filtrare le statistiche.
        /// </summary>
        private void ChooseCategories_Click(object sender, RoutedEventArgs e)
        {
            var selettore = new CategorySelectionWindow(_jwtToken, Mode.StatsPage);

            if (selettore.ShowDialog() == true && selettore.Selezionate != null)
            {
                _categorieSelezionate = selettore.Selezionate;
                SelectedCategoriesBox.Text = string.Join(", ", _categorieSelezionate.Select(c => c.Nome));
                LoadStatsPerCategory();
            }
            else
            {
                SelectedCategoriesBox.Text = string.Empty;
            }
        }

        /// <summary>
        /// Mostra il grafico delle risposte per categoria.
        /// </summary>
        private void ShowGeneralStatsChart(List<TeacherCategoriaStats>? statsPerCategoria)
        {
            if (statsPerCategoria == null || statsPerCategoria.Count == 0)
                return;

            var model = new PlotModel { Title = "Risposte per Categoria" };
            var categoryLabels = statsPerCategoria.Select(s => $"{s.CategoriaNome}\n({s.Visibilita})").ToList();

            // Asse Y: categorie
            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                ItemsSource = categoryLabels,
                Title = "Categoria",
                GapWidth = 0.5
            });

            // Asse X: numero risposte
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Numero Risposte",
                Minimum = 0,
                MajorStep = 1,
                MinorStep = 1,
                StringFormat = "0"
            });

            // Serie di dati
            var corretteSeries = CreateBarSeries("Corrette", OxyColors.SeaGreen, true);
            var sbagliateSeries = CreateBarSeries("Sbagliate", OxyColors.IndianRed, true);
            var nonDateSeries = CreateBarSeries("Non Date", OxyColors.SteelBlue, true);

            foreach (var stat in statsPerCategoria)
            {
                corretteSeries.Items.Add(new BarItem { Value = stat.Corrette });
                sbagliateSeries.Items.Add(new BarItem { Value = stat.Sbagliate });
                nonDateSeries.Items.Add(new BarItem { Value = stat.NonDate });
            }

            model.Series.Add(corretteSeries);
            model.Series.Add(sbagliateSeries);
            model.Series.Add(nonDateSeries);

            TeacherGeneralStatsBarChart.Model = model;
        }

        /// <summary>
        /// Mostra il grafico delle risposte per difficoltà.
        /// </summary>
        private void ShowStatsPerDifficoltaChart(List<DifficoltaStats> statsPerDifficolta)
        {
            if (statsPerDifficolta == null || !statsPerDifficolta.Any())
                return;

            var model = new PlotModel { Title = "Risposte per Difficoltà" };
            var difficoltaLabels = statsPerDifficolta.Select(s => s.Difficolta).ToList();

            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                ItemsSource = difficoltaLabels,
                Title = "Difficoltà"
            });

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Numero Risposte",
                Minimum = 0,
                MajorStep = 1,
                MinorStep = 1,
                StringFormat = "0"
            });

            var corretteSeries = CreateBarSeries("Corrette", OxyColors.SeaGreen);
            var sbagliateSeries = CreateBarSeries("Sbagliate", OxyColors.IndianRed);
            var nonDateSeries = CreateBarSeries("Non Date", OxyColors.SteelBlue);

            foreach (var stat in statsPerDifficolta)
            {
                corretteSeries.Items.Add(new BarItem { Value = stat.Corrette });
                sbagliateSeries.Items.Add(new BarItem { Value = stat.Sbagliate });
                nonDateSeries.Items.Add(new BarItem { Value = stat.NonDate });
            }

            model.Series.Add(corretteSeries);
            model.Series.Add(sbagliateSeries);
            model.Series.Add(nonDateSeries);

            StatsPerDifficoltaChart.Model = model;
        }

        /// <summary>
        /// Mostra la classifica dei migliori dieci studenti.
        /// </summary>
        private void ShowTop10Studenti(List<StudentRanking> top10Studenti)
        {
            if (top10Studenti == null || top10Studenti.Count == 0)
            {
                Top10StudentiListView.ItemsSource = null;
                return;
            }

            ClassificaTitleText.Visibility = Visibility.Visible;
            Top10StudentiListView.Visibility = Visibility.Visible;
            Top10StudentiListView.ItemsSource = top10Studenti;
        }

        /// <summary>
        /// Crea una serie a barre con stile coerente.
        /// </summary>
        private static BarSeries CreateBarSeries(string title, OxyColor color, bool stacked = false)
        {
            return new BarSeries
            {
                Title = title,
                FillColor = color,
                IsStacked = stacked,
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = "{0}"
            };
        }

        // ————— Navigazione tra pannelli —————

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            InfoPanel.Visibility = Visibility.Visible;
            StatsPanel.Visibility = Visibility.Collapsed;
            GeneralStatsPanel.Visibility = Visibility.Collapsed;
        }

        private void StatsPerCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            InfoPanel.Visibility = Visibility.Collapsed;
            StatsPanel.Visibility = Visibility.Visible;
            GeneralStatsPanel.Visibility = Visibility.Collapsed;
        }

        private void GeneralStatsButton_Click(object sender, RoutedEventArgs e)
        {
            InfoPanel.Visibility = Visibility.Collapsed;
            StatsPanel.Visibility = Visibility.Collapsed;
            GeneralStatsPanel.Visibility = Visibility.Visible;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
            else
                MessageBox.Show("Nessuna pagina precedente disponibile.");
        }


        // ----- Dispose() -----
        public void Dispose()
        {
            // Best practice: controllo con flag per garantire idempotenza
            if (_disposed)
                return;
            _crudService.Dispose();
            _analyticsService.Dispose();
            _disposed = true;

            // Sopprime la finalizzazione se non è necessaria (buona pratica):
            // permette di saltare l'esecuzione del finalizzatore, poichè le risorse critiche sono già
            // state rilasciate, e pulire la sua memoria nel modo più veloce possibile.
            GC.SuppressFinalize(this);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Chiama il metodo Dispose() quando la pagina non è più visualizzata
            // Questa è la chiave per il rilascio di _quizService.HttpClient.
            (this as IDisposable)?.Dispose();
        }
    }
}
