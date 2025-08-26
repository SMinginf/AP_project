using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using QuizClient.Models;
using QuizClient.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
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
    public partial class StudentStats : Page
    {
        private readonly AnalyticsService analyticsService;
        private readonly CRUDService crudService;
        
        private readonly string _jwtToken; // Token JWT per autenticazione
        private StudentStatsResponse? _stats;
        private List<Categoria> _categorieSelezionate = new List<Categoria>();

        public StudentStats(string jwtToken)
        {
            InitializeComponent();

            _jwtToken = jwtToken;
            analyticsService = new AnalyticsService(_jwtToken); // Inizializza il servizio con il token JWT
            crudService = new CRUDService(_jwtToken); // Inizializza il servizio CRUD con il token JWT

            LoadPersonalInfo();
            LoadGeneralStats();

        }

        private async void LoadPersonalInfo() {
            // Ottieni info personali dell'utente
            var result = await crudService.GetUserAsync();
            if (result == null || !result.Success || result.Data == null)
            {
                MessageBox.Show("Errore nel caricamento dei dati dell'utente: " + result?.ErrorMessage);
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
            var result = await analyticsService.GetStudentGeneralStatsAsync();
            if (result == null || !result.Success || result.Data == null)
            {
                MessageBox.Show("Errore nel caricamento delle statistiche generali dello studente: " + result?.ErrorMessage);
                return;
            }
            var generalStats = result.Data;


            GeneralStatsPanel.DataContext= generalStats;

            ShowPercentualiTotaliChart(generalStats.StatisticheGenerali.PercCorrette, generalStats.StatisticheGenerali.PercSbagliate, generalStats.StatisticheGenerali.PercNonDate, GeneralStatsPieChart);
            
            ShowUserCategoriesChart(generalStats.PunteggiPerCategoria);
            
        }



        private async void LoadStatsPerCategory()
        {
            var categoria_ids = _categorieSelezionate.Select(c => c.ID).ToList();
            var stats_result = await analyticsService.GetStudentStatsPerCategoryAsync(categoria_ids);
            if (stats_result == null || !stats_result.Success || stats_result.Data == null)
            {
                MessageBox.Show("Errore nel caricamento delle statistiche dello studente: " + stats_result?.ErrorMessage);
                return;
            }

            _stats = stats_result.Data;

            // Mostra quiz e quesiti totali
            StatsPanel.DataContext = _stats.QuizEQuesitiTotali;
            StatCardsPerCategoriaGrid.Visibility = Visibility.Visible;

            // Mostra grafici
            ShowStatsPerCategoriaChart();
            ShowPercentualiTotaliChart(_stats.PercentualiTotali.PercCorrette, _stats.PercentualiTotali.PercSbagliate, _stats.PercentualiTotali.PercNonDate, PercentualiTotaliPieChart);
            ShowAndamentoTemporaleChart();
        }
        private void ShowStatsPerCategoriaChart()
        {
            if (_stats == null || _stats.StatsPerCategoriaDifficolta == null || !_stats.StatsPerCategoriaDifficolta.Any())
                return;

            var difficulties = _stats.StatsPerCategoriaDifficolta
                .Select(s => s.Difficolta)
                .Distinct()
                .ToList();

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

            foreach (var diff in difficulties)
            {
                var stat = _stats.StatsPerCategoriaDifficolta.FirstOrDefault(s => s.Difficolta == diff);
                corretteSeries.Items.Add(new BarItem { Value = stat?.Corrette ?? 0 });
                sbagliateSeries.Items.Add(new BarItem { Value = stat?.Sbagliate ?? 0 });
                nonDateSeries.Items.Add(new BarItem { Value = stat?.NonDate ?? 0 });
            }

            var model = new PlotModel { Title = "Risposte per Difficoltà" };

            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left, // ← invertito per BarSeries
                ItemsSource = difficulties
            });

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Numero Risposte",
                Minimum = 0,
                MajorStep = 2,
                MinorStep = 1,
                StringFormat = "0"
            });

            model.Series.Add(corretteSeries);
            model.Series.Add(sbagliateSeries);
            model.Series.Add(nonDateSeries);

            StatsPerCategoriaDifficoltaChart.Model = model;
        }

        private void ShowPercentualiTotaliChart(double perc_corrette, double perc_sbagliate, double perc_non_date, OxyPlot.Wpf.PlotView plotView)
        {
            var pieModel = new PlotModel { Title = "Percentuali Totali Risposte" };

            var pieSeries = new PieSeries
            {
                StrokeThickness = 1.0,
                InsideLabelPosition = 0.8,
                AngleSpan = 360,
                StartAngle = 0
            };

            pieSeries.Slices.Add(new PieSlice("Corrette", perc_corrette)
            {
                Fill = OxyColors.SeaGreen
            });
            pieSeries.Slices.Add(new PieSlice("Sbagliate", perc_sbagliate)
            {
                Fill = OxyColors.IndianRed
            });
            pieSeries.Slices.Add(new PieSlice("Non Date", perc_non_date)
            {
                Fill = OxyColors.SteelBlue
            });

            pieModel.Series.Add(pieSeries);

            plotView.Model = pieModel;
        }

        private void ShowAndamentoTemporaleChart()
        {
            // ANDAMENTO PER DIFFICOLTA'
            if (_stats?.AndamentoTemporalePerDifficolta == null || _stats.AndamentoTemporalePerDifficolta.Count == 0)
                return;

            var model = new PlotModel { Title = "Andamento temporale per difficoltà" };

            // Asse X: Date
            model.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd/MM/yyyy",
                Title = "Data Quiz",
                IntervalType = DateTimeIntervalType.Days,
                MinorIntervalType = DateTimeIntervalType.Days,
                IsZoomEnabled = true,
                IsPanEnabled = true
            });

            // Asse Y: Percentuale corrette
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Percentuale Risposte Corrette",
                Minimum = 0,
                MajorStep = 10,
                Maximum = 110,
                StringFormat = "0'%'"
            });

            // Colori per difficoltà
            var colorMap = new Dictionary<string, OxyColor>
            {
                { "Facile", OxyColors.SeaGreen },
                { "Intermedia", OxyColors.SteelBlue },
                { "Difficile", OxyColors.IndianRed }
            };

            var difficoltaPresenti = _stats.AndamentoTemporalePerDifficolta
                .Select(t => t.Difficolta)
                .Distinct();

            foreach (var difficolta in difficoltaPresenti)
            {
                var punti = _stats.AndamentoTemporalePerDifficolta
                    .Where(t => t.Difficolta == difficolta && t.TotaleQuesiti > 0)
                    .OrderBy(t => t.DataQuiz)
                    .ToList();

                if (!punti.Any())
                    continue;

                if (punti.Count == 1)
                {
                    // Usa ScatterSeries se solo 1 punto
                    var scatter = new ScatterSeries
                    {
                        Title = difficolta,
                        MarkerType = MarkerType.Circle,
                        MarkerSize = 6,
                        MarkerFill = colorMap.ContainsKey(difficolta) ? colorMap[difficolta] : OxyColors.Gray
                    };

                    var p = punti.First();
                    double y = Math.Round((p.Corrette * 100.0) / p.TotaleQuesiti,2);
                    scatter.Points.Add(new ScatterPoint(DateTimeAxis.ToDouble(p.DataQuiz.ToLocalTime()), y));

                    model.Series.Add(scatter);
                }
                else
                {
                    // Usa LineSeries se più punti
                    var lineSeries = new LineSeries
                    {
                        Title = difficolta,
                        Color = colorMap.ContainsKey(difficolta) ? colorMap[difficolta] : OxyColors.Gray,
                        StrokeThickness = 2,
                        MarkerType = MarkerType.Circle,
                        MarkerSize = 5,
                        CanTrackerInterpolatePoints = false,
                        MarkerStroke = OxyColors.Black,
                        MarkerFill = colorMap.ContainsKey(difficolta) ? colorMap[difficolta] : OxyColors.Gray,
                        LineStyle = LineStyle.Solid
                    };

                    foreach (var punto in punti)
                    {
                        double percentualeCorrette = Math.Round((punto.Corrette * 100.0) / punto.TotaleQuesiti,2);
                        var dataX = DateTimeAxis.ToDouble(punto.DataQuiz.ToLocalTime());
                        lineSeries.Points.Add(new DataPoint(dataX, percentualeCorrette));
                    }

                    model.Series.Add(lineSeries);
                }
            }


            // ANDAMENTO TOTALE
            if (_stats.AndamentoTemporaleTotale != null && _stats.AndamentoTemporaleTotale.Count != 0)
            {
                var puntiTotali = _stats.AndamentoTemporaleTotale
                    .Where(t => t.TotaleQuesiti > 0)
                    .OrderBy(t => t.DataQuiz)
                    .ToList();

                if (puntiTotali.Count == 1)
                {
                    var scatter = new ScatterSeries
                    {
                        Title = "Totale",
                        MarkerType = MarkerType.Square,
                        MarkerSize = 6,
                        MarkerFill = OxyColors.Black
                    };

                    var p = puntiTotali.First();
                    double y = Math.Round((p.Corrette * 100.0) / p.TotaleQuesiti, 2);
                    scatter.Points.Add(new ScatterPoint(DateTimeAxis.ToDouble(p.DataQuiz.ToLocalTime()), y));

                    model.Series.Add(scatter);
                }
                else
                {
                    var lineSeries = new LineSeries
                    {
                        Title = "Totale",
                        Color = OxyColors.Black,
                        StrokeThickness = 2,
                        MarkerType = MarkerType.Square,
                        MarkerSize = 5,
                        CanTrackerInterpolatePoints = false,
                        MarkerStroke = OxyColors.Black,
                        MarkerFill = OxyColors.Black,
                        LineStyle = LineStyle.Dash
                    };

                    foreach (var punto in puntiTotali)
                    {
                        double percentualeCorrette = Math.Round((punto.Corrette * 100.0) / punto.TotaleQuesiti, 2);
                        var dataX = DateTimeAxis.ToDouble(punto.DataQuiz.ToLocalTime());
                        lineSeries.Points.Add(new DataPoint(dataX, percentualeCorrette));
                    }

                    model.Series.Add(lineSeries);
                }
            }



            StudentTimelineChart.Model = model;
        }

        private void ShowUserCategoriesChart(List<PunteggioCategoria>? punteggiPerCategoria)
        {
            if (punteggiPerCategoria == null || !punteggiPerCategoria.Any())
                return;

            var model = new PlotModel { Title = "Risposte per Categoria" };

            var categoryLabels = punteggiPerCategoria
                .Select(p => $"{p.CategoriaNome} ({p.DocenteUsername})")
                .ToList();

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

            foreach (var p in punteggiPerCategoria)
            {
                corretteSeries.Items.Add(new BarItem { Value = p.Corrette });
                sbagliateSeries.Items.Add(new BarItem { Value = p.Sbagliate });
                nonDateSeries.Items.Add(new BarItem { Value = p.NonDate });
            }

            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left, // ← categorie sul lato sinistro per BarSeries
                ItemsSource = categoryLabels,
                GapWidth = 0.5
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

            model.Series.Add(corretteSeries);
            model.Series.Add(sbagliateSeries);
            model.Series.Add(nonDateSeries);

            PunteggiPerCategoriaChart.Model = model;
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
