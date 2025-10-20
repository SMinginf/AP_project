/*
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using QuizClient.Models;
using QuizClient.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;



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
*/

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using QuizClient.Models;
using QuizClient.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QuizClient
{
    public partial class StudentStats : Page, IDisposable
    {
        private readonly AnalyticsService _analyticsService;
        private readonly CRUDService _crudService;
        private readonly string _jwtToken;

        private StudentStatsResponse? _stats;
        private List<Categoria> _categorieSelezionate = new();

        private bool _disposed;

        public StudentStats(string jwtToken)
        {
            InitializeComponent();

            _jwtToken = jwtToken;
            _analyticsService = new AnalyticsService(_jwtToken);
            _crudService = new CRUDService(_jwtToken);

            // Carica i dati iniziali
            LoadPersonalInfo();
            LoadGeneralStats();
        }

        // --------------------- //
        //    SEZIONE DATI       //
        // --------------------- //

        private async void LoadPersonalInfo()
        {
            var result = await _crudService.GetUserAsync();
            if (result?.Success != true || result.Data is null)
            {
                MessageBox.Show($"Errore nel caricamento dei dati dell'utente: {result?.ErrorMessage}");
                return;
            }

            var user = result.Data;

            // Popola le informazioni personali
            NameText.Text = user.Nome;
            CognomeText.Text = user.Cognome;
            EmailText.Text = user.Email;
            UsernameText.Text = user.Username;
            BirthDateText.Text = user.DataNascita.ToShortDateString();
            GenderText.Text = user.Genere;
        }

        private async void LoadGeneralStats()
        {
            var result = await _analyticsService.GetStudentGeneralStatsAsync();
            if (result?.Success != true || result.Data is null)
            {
                MessageBox.Show($"Errore nel caricamento delle statistiche generali: {result?.ErrorMessage}");
                return;
            }

            var generalStats = result.Data;
            GeneralStatsPanel.DataContext = generalStats;

            ShowPercentualiTotaliChart(
                generalStats.StatisticheGenerali.PercCorrette,
                generalStats.StatisticheGenerali.PercSbagliate,
                generalStats.StatisticheGenerali.PercNonDate,
                GeneralStatsPieChart);

            ShowUserCategoriesChart(generalStats.PunteggiPerCategoria);
        }

        private async void LoadStatsPerCategory()
        {
            var categoryIds = _categorieSelezionate.Select(c => c.ID).ToList();
            var result = await _analyticsService.GetStudentStatsPerCategoryAsync(categoryIds);

            if (result?.Success != true || result.Data is null)
            {
                MessageBox.Show($"Errore nel caricamento delle statistiche per categoria: {result?.ErrorMessage}");
                return;
            }

            _stats = result.Data;
            StatsPanel.DataContext = _stats.QuizEQuesitiTotali;
            StatCardsPerCategoriaGrid.Visibility = Visibility.Visible;

            // Mostra grafici
            ShowStatsPerCategoriaChart();
            ShowPercentualiTotaliChart(_stats.PercentualiTotali.PercCorrette,
                                       _stats.PercentualiTotali.PercSbagliate,
                                       _stats.PercentualiTotali.PercNonDate,
                                       PercentualiTotaliPieChart);
            ShowAndamentoTemporaleChart();
        }

        // --------------------- //
        //    GRAFICI OXYPLOT    //
        // --------------------- //

        private void ShowStatsPerCategoriaChart()
        {
            if (_stats?.StatsPerCategoriaDifficolta == null || !_stats.StatsPerCategoriaDifficolta.Any())
                return;

            var difficulties = _stats.StatsPerCategoriaDifficolta
                .Select(s => s.Difficolta)
                .Distinct()
                .ToList();

            var model = new PlotModel { Title = "Risposte per Difficoltà" };

            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
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

            // Serie
            var seriesData = new[]
            {
                new { Title = "Corrette", Color = OxyColors.SeaGreen, Selector = new Func<StatsPerCategoriaDifficolta, int>(s => s.Corrette) },
                new { Title = "Sbagliate", Color = OxyColors.IndianRed, Selector = new Func<StatsPerCategoriaDifficolta, int>(s => s.Sbagliate) },
                new { Title = "Non Date", Color = OxyColors.SteelBlue, Selector = new Func<StatsPerCategoriaDifficolta, int>(s => s.NonDate) }
            };

            foreach (var s in seriesData)
            {
                var barSeries = new BarSeries
                {
                    Title = s.Title,
                    FillColor = s.Color,
                    LabelPlacement = LabelPlacement.Middle,
                    LabelFormatString = "{0}"
                };

                foreach (var diff in difficulties)
                {
                    var stat = _stats.StatsPerCategoriaDifficolta.FirstOrDefault(x => x.Difficolta == diff);
                    barSeries.Items.Add(new BarItem { Value = stat != null ? s.Selector(stat) : 0 });
                }

                model.Series.Add(barSeries);
            }

            StatsPerCategoriaDifficoltaChart.Model = model;
        }

        private void ShowPercentualiTotaliChart(double corrette, double sbagliate, double nonDate, OxyPlot.Wpf.PlotView plotView)
        {
            var pieModel = new PlotModel { Title = "Percentuali Totali Risposte" };

            var pieSeries = new PieSeries
            {
                StrokeThickness = 1.0,
                InsideLabelPosition = 0.8,
                AngleSpan = 360,
                StartAngle = 0
            };

            pieSeries.Slices.Add(new PieSlice("Corrette", corrette) { Fill = OxyColors.SeaGreen });
            pieSeries.Slices.Add(new PieSlice("Sbagliate", sbagliate) { Fill = OxyColors.IndianRed });
            pieSeries.Slices.Add(new PieSlice("Non Date", nonDate) { Fill = OxyColors.SteelBlue });

            pieModel.Series.Add(pieSeries);
            plotView.Model = pieModel;
        }

        private void ShowUserCategoriesChart(List<PunteggioCategoria>? punteggi)
        {
            if (punteggi == null || !punteggi.Any())
                return;

            var model = new PlotModel { Title = "Risposte per Categoria" };

            var categoryLabels = punteggi.Select(p => $"{p.CategoriaNome} ({p.DocenteUsername})").ToList();

            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                ItemsSource = categoryLabels,
                GapWidth = 0.5
            });

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Numero Risposte",
                Minimum = 0,
                MajorStep = 1,
                MinorStep = 1
            });

            var barSets = new[]
            {
                new { Title = "Corrette", Color = OxyColors.SeaGreen, Values = punteggi.Select(p => (double)p.Corrette) },
                new { Title = "Sbagliate", Color = OxyColors.IndianRed, Values = punteggi.Select(p => (double)p.Sbagliate) },
                new { Title = "Non Date", Color = OxyColors.SteelBlue, Values = punteggi.Select(p => (double)p.NonDate) }
            };

            foreach (var set in barSets)
            {
                var series = new BarSeries
                {
                    Title = set.Title,
                    FillColor = set.Color,
                    LabelPlacement = LabelPlacement.Middle,
                    LabelFormatString = "{0}"
                };

                foreach (var v in set.Values)
                    series.Items.Add(new BarItem { Value = v });

                model.Series.Add(series);
            }

            PunteggiPerCategoriaChart.Model = model;
        }

        private void ShowAndamentoTemporaleChart()
        {
            if (_stats?.AndamentoTemporalePerDifficolta == null || !_stats.AndamentoTemporalePerDifficolta.Any())
                return;

            var model = new PlotModel { Title = "Andamento temporale per difficoltà" };

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

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Percentuale Risposte Corrette",
                Minimum = 0,
                Maximum = 110,
                MajorStep = 10,
                StringFormat = "0'%'"
            });

            var colorMap = new Dictionary<string, OxyColor>
            {
                { "Facile", OxyColors.SeaGreen },
                { "Intermedia", OxyColors.SteelBlue },
                { "Difficile", OxyColors.IndianRed }
            };

            foreach (var gruppo in _stats.AndamentoTemporalePerDifficolta.GroupBy(x => x.Difficolta))
            {
                var punti = gruppo.OrderBy(p => p.DataQuiz).Where(p => p.TotaleQuesiti > 0).ToList();
                if (!punti.Any()) continue;

                if (punti.Count == 1)
                {
                    var unico = punti.First();
                    var scatter = new ScatterSeries
                    {
                        Title = gruppo.Key,
                        MarkerType = MarkerType.Circle,
                        MarkerSize = 6,
                        MarkerFill = colorMap.GetValueOrDefault(gruppo.Key, OxyColors.Gray)
                    };

                    var y = Math.Round((unico.Corrette * 100.0) / unico.TotaleQuesiti, 2);
                    scatter.Points.Add(new ScatterPoint(DateTimeAxis.ToDouble(unico.DataQuiz.ToLocalTime()), y));

                    model.Series.Add(scatter);
                }
                else
                {
                    var line = new LineSeries
                    {
                        Title = gruppo.Key,
                        Color = colorMap.GetValueOrDefault(gruppo.Key, OxyColors.Gray),
                        StrokeThickness = 2,
                        MarkerType = MarkerType.Circle,
                        MarkerSize = 5,
                        MarkerFill = colorMap.GetValueOrDefault(gruppo.Key, OxyColors.Gray)
                    };

                    foreach (var p in punti)
                    {
                        var y = Math.Round((p.Corrette * 100.0) / p.TotaleQuesiti, 2);
                        line.Points.Add(new DataPoint(DateTimeAxis.ToDouble(p.DataQuiz.ToLocalTime()), y));
                    }

                    model.Series.Add(line);
                }
            }

            StudentTimelineChart.Model = model;
        }

        // --------------------- //
        //     EVENTI UI         //
        // --------------------- //

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            InfoPanel.Visibility = Visibility.Visible;
            StatsPanel.Visibility = GeneralStatsPanel.Visibility = Visibility.Collapsed;
        }

        private void StatsPerCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            StatsPanel.Visibility = Visibility.Visible;
            InfoPanel.Visibility = GeneralStatsPanel.Visibility = Visibility.Collapsed;
        }

        private void GeneralStatsButton_Click(object sender, RoutedEventArgs e)
        {
            GeneralStatsPanel.Visibility = Visibility.Visible;
            InfoPanel.Visibility = StatsPanel.Visibility = Visibility.Collapsed;
        }

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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
            else
                MessageBox.Show("Nessuna pagina precedente disponibile.");
        }

        // --------------------- //
        //   DISPOSING SERVIZI   //
        // --------------------- //

        public void Dispose()
        {
            if (_disposed) 
                return;
            _analyticsService.Dispose();
            _crudService.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Chiama il metodo Dispose() quando la pagina non è più visualizzata
            // Questa è la chiave per il rilascio di _crudService.HttpClient.
            (this as IDisposable)?.Dispose();
        }
    }
}

