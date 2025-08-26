using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using QuizClient.Models;
using QuizClient.Services;
using QuizClient.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuizClient
{
    /// <summary>
    /// Logica di interazione per TeacherStats.xaml
    /// </summary>
    public partial class TeacherStats : Page
    {

        private readonly AnalyticsService analyticsService;
        private readonly CRUDService crudService;

        private readonly string _jwtToken; // Token JWT per autenticazione
        private TeacherCategoryStatsResponse _stats = new();
        private TeacherGeneralStats? _generalStats;
        private List<Categoria> _categorieSelezionate = new List<Categoria>();

        public TeacherStats(string jwttoken)
        {
            InitializeComponent();

            analyticsService = new AnalyticsService(jwttoken);
            crudService = new CRUDService(jwttoken);
            _jwtToken = jwttoken;

            LoadPersonalInfo();
            LoadGeneralStats();
        }

        private async void LoadPersonalInfo()
        {
            // Ottieni info personali dell'utente
            var result = await crudService.GetUserAsync();
            if (result == null || !result.Success || result.Data == null)
            {
                MessageBox.Show("Errore nel caricamento dei dati del docente: " + result?.ErrorMessage);
                return;
            }
            var user_info = result.Data;

            // Popola info personali
            NameText.Text = user_info.Nome;
            CognomeText.Text = user_info.Cognome;
            EmailText.Text = user_info.Email;
            UsernameText.Text = user_info.Username;
            BirthDateText.Text = user_info.DataNascita.ToShortDateString();
            GenderText.Text = user_info.Genere;

        }


        private async void LoadGeneralStats()
        {
            var result = await analyticsService.GetTeacherGeneralStatsAsync();
            if (result == null || !result.Success || result.Data == null)
            {
                MessageBox.Show("Errore nel caricamento delle statistiche generali del docente: " + result?.ErrorMessage);
                return;
            }
            _generalStats = result.Data;

            // Popola le statistiche generali 
            ShowGeneralStatsChart(_generalStats.StatsPerCategoria);

            GeneralStatsPanel.DataContext = _generalStats;
        }

        private async void LoadStatsPerCategory()
        {
            var categoria_ids = _categorieSelezionate.Select(c => c.ID).ToList();
            var stats_result = await analyticsService.GetTeacherStatsPerCategoryAsync(categoria_ids);
            if (stats_result == null || !stats_result.Success || stats_result.Data == null)
            {
                MessageBox.Show("Errore nel caricamento delle statistiche: " + stats_result?.ErrorMessage);
                return;
            }

            _stats = stats_result.Data;

            // Mostra grafici
            ShowStatsPerDifficoltaChart(_stats.StatsPerDifficolta);
            ShowTop10Studenti(_stats.Top10Studenti);

            QuesitiSpeciali.Visibility=Visibility.Visible;
            QuesitiSpeciali.DataContext = _stats;

        }

         private void ChooseCategories_Click(object sender, RoutedEventArgs e) {
            var selettore = new CategorySelectionWindow(_jwtToken, Mode.StatsPage);
            if (selettore.ShowDialog() == true && selettore.Selezionate != null)
            {
                _categorieSelezionate = selettore.Selezionate;
                SelectedCategoriesBox.Text = string.Join(", ", _categorieSelezionate.Select(c => c.Nome));
                LoadStatsPerCategory(); // Ritira le statistiche per le categorie selezionate
            }
            else
            {
                SelectedCategoriesBox.Text = string.Empty;
            }
        }

        private void ShowGeneralStatsChart(List<TeacherCategoriaStats>? statsPerCategoria)
        {
            if (statsPerCategoria == null || statsPerCategoria.Count == 0)
                return;

            var model = new PlotModel { Title = "Risposte per Categoria" };

            // Asse Y: etichette delle categorie
            var categoryLabels = statsPerCategoria.Select(s => $"{s.CategoriaNome}\n({s.Visibilita})").ToList();

            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left, // ← obbligatorio per BarSeries
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

            var corretteSeries = new BarSeries
            {
                Title = "Corrette",
                FillColor = OxyColors.SeaGreen,
                IsStacked = true,
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = "{0}"
            };

            var sbagliateSeries = new BarSeries
            {
                Title = "Sbagliate",
                FillColor = OxyColors.IndianRed,
                IsStacked = true,
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = "{0}"
            };

            var nonDateSeries = new BarSeries
            {
                Title = "Non Date",
                FillColor = OxyColors.SteelBlue,
                IsStacked = true,
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = "{0}"
            };

            for (int i = 0; i < statsPerCategoria.Count; i++)
            {
                var stat = statsPerCategoria[i];
                corretteSeries.Items.Add(new BarItem { Value = stat.Corrette });
                sbagliateSeries.Items.Add(new BarItem { Value = stat.Sbagliate });
                nonDateSeries.Items.Add(new BarItem { Value = stat.NonDate });
            }

            model.Series.Add(corretteSeries);
            model.Series.Add(sbagliateSeries);
            model.Series.Add(nonDateSeries);

            TeacherGeneralStatsBarChart.Model = model;
        }

        private void ShowStatsPerDifficoltaChart(List<DifficoltaStats> statsPerDifficolta)
        {
            if (statsPerDifficolta == null || !statsPerDifficolta.Any())
                return;

            var model = new PlotModel { Title = "Risposte per Difficoltà" };

            var difficoltaLabels = statsPerDifficolta.Select(s => s.Difficolta).ToList();

            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left, // ← asse categorie per BarSeries
                ItemsSource = difficoltaLabels,
                Title = "Difficoltà"
            });

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom, // ← asse dei numeri
                Title = "Numero Risposte",
                Minimum = 0,
                MajorStep = 1,
                MinorStep = 1,
                StringFormat = "0"
            });

            var corretteSeries = new BarSeries
            {
                Title = "Corrette",
                FillColor = OxyColors.SeaGreen,
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = "{0}"
            };

            var sbagliateSeries = new BarSeries
            {
                Title = "Sbagliate",
                FillColor = OxyColors.IndianRed,
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = "{0}"
            };

            var nonDateSeries = new BarSeries
            {
                Title = "Non Date",
                FillColor = OxyColors.SteelBlue,
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = "{0}"
            };

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

        private void ShowTop10Studenti(List<StudentRanking> top10Studenti)
        {

            if (top10Studenti == null || top10Studenti.Count == 0)
            {
                Top10StudentiListView.ItemsSource = null;
                return;
            }

            ClassificaTitleText.Visibility= Visibility.Visible;
            Top10StudentiListView.Visibility = Visibility.Visible;
            Top10StudentiListView.ItemsSource = top10Studenti;
        }

  

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
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                // Opzionale: gestisci il caso in cui non sia possibile tornare indietro
                MessageBox.Show("Nessuna pagina precedente disponibile.");
            }
        }
    
    
    }


}
