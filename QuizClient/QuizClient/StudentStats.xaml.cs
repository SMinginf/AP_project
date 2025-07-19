using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using QuizClient.Services;
using QuizClient.Models;
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
    public partial class StudentStats : Page
    {
        private readonly AnalyticsService analyticsService;
        private readonly string _jwtToken; // Token JWT per autenticazione
        private StudentStatsResponse? _stats;

        public StudentStats(string jwtToken)
        {
            InitializeComponent();

            _jwtToken = jwtToken;
            analyticsService = new AnalyticsService(_jwtToken); // Inizializza il servizio con il token JWT
            LoadStudentStats();
            
        }

        private async void LoadStudentStats()
        {
            var result = await analyticsService.GetStudentStatsAsync(); // Metodo per ottenere le statistiche dal server

            if (result == null || !result.Success || result.Data == null)
            {
                MessageBox.Show("Errore nel caricamento delle statistiche dello studente: " + result?.ErrorMessage);
                return;
            }

            _stats = result.Data; // Salva i dati delle statistiche dello studente

            // Popola info personali
            var studente = _stats.Studente;
            NameText.Text = $"{studente.Nome} {studente.Cognome}";
            EmailText.Text = studente.Email;
            BirthDateText.Text = studente.DataNascita.ToShortDateString();
            GenderText.Text = studente.Genere;

            // Grafico quiz per categoria
            var categoryModel = new PlotModel { Title = "Quiz per Categoria" };
            var catAxis = new CategoryAxis { Position = AxisPosition.Bottom };
            var catValueAxis = new LinearAxis { Position = AxisPosition.Left, MinimumPadding = 0, AbsoluteMinimum = 0 };
            categoryModel.Axes.Add(catAxis);
            categoryModel.Axes.Add(catValueAxis);

            var catSeries = new ColumnSeries { Title = "Quiz" };

            var byCategory = _stats.StatsPerCategoriaDifficolta
                .GroupBy(s => s.Categoria)
                .Select(g => new { Categoria = g.Key, Totale = g.Sum(s => s.Corrette + s.Sbagliate + s.NonDate) });

            foreach (var stat in byCategory)
            {
                catAxis.Labels.Add(stat.Categoria);
                catSeries.Items.Add(new ColumnItem(stat.Totale));
            }

            categoryModel.Series.Add(catSeries);
            CategoryChart.Model = categoryModel;

            // Grafico prestazioni per categoria/difficoltà
            var performanceModel = new PlotModel { Title = "Prestazioni per Categoria/Difficoltà" };
            var categoryAxis = new CategoryAxis { Position = AxisPosition.Bottom };
            var valueAxis = new LinearAxis { Position = AxisPosition.Left, MinimumPadding = 0, AbsoluteMinimum = 0 };
            performanceModel.Axes.Add(categoryAxis);
            performanceModel.Axes.Add(valueAxis);

            var correctSeriesCat = new ColumnSeries { Title = "Corrette" };
            var wrongSeriesCat = new ColumnSeries { Title = "Sbagliate" };
            var notAnsweredSeriesCat = new ColumnSeries { Title = "Non Date" };

            foreach (var stat in _stats.StatsPerCategoriaDifficolta)
            {
                var label = $"{stat.Categoria} ({stat.Difficolta})";
                categoryAxis.Labels.Add(label);
                correctSeriesCat.Items.Add(new ColumnItem(stat.Corrette));
                wrongSeriesCat.Items.Add(new ColumnItem(stat.Sbagliate));
                notAnsweredSeriesCat.Items.Add(new ColumnItem(stat.NonDate));
            }

            performanceModel.Series.Add(correctSeriesCat);
            performanceModel.Series.Add(wrongSeriesCat);
            performanceModel.Series.Add(notAnsweredSeriesCat);
            PerformanceChart.Model = performanceModel;

            // Grafico percentuale corrette per difficoltà
            var diffModel = new PlotModel { Title = "Percentuale Corrette per Difficoltà" };
            var diffAxis = new CategoryAxis { Position = AxisPosition.Bottom };
            var diffValueAxis = new LinearAxis { Position = AxisPosition.Left, Minimum = 0, Maximum = 100 };
            diffModel.Axes.Add(diffAxis);
            diffModel.Axes.Add(diffValueAxis);

            var diffSeries = new ColumnSeries { Title = "% Corrette" };

            var byDiff = _stats.StatsPerCategoriaDifficolta
                .GroupBy(s => s.Difficolta)
                .Select(g => new
                {
                    Difficolta = g.Key,
                    Corrette = g.Sum(s => s.Corrette),
                    Totali = g.Sum(s => s.Corrette + s.Sbagliate + s.NonDate)
                });

            foreach (var stat in byDiff)
            {
                diffAxis.Labels.Add(stat.Difficolta);
                double perc = stat.Totali > 0 ? (double)stat.Corrette / stat.Totali * 100.0 : 0;
                diffSeries.Items.Add(new ColumnItem(perc));
            }

            diffModel.Series.Add(diffSeries);
            DifficultyChart.Model = diffModel;

            // Imposta i dati per il grafico temporale
            var timelineData = _stats.AndamentoTemporale;
            var plotModel = new PlotModel { Title = "Andamento Temporale" };

            var dateAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd/MM",
                IntervalType = DateTimeIntervalType.Days,
                MinorIntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            };

            var valueAxisTime = new LinearAxis
            {
                Position = AxisPosition.Left,
                MinimumPadding = 0,
                AbsoluteMinimum = 0
            };

            plotModel.Axes.Add(dateAxis);
            plotModel.Axes.Add(valueAxisTime);

            var correctSeries = new LineSeries { Title = "Corrette", MarkerType = MarkerType.Circle };
            var wrongSeries = new LineSeries { Title = "Sbagliate", MarkerType = MarkerType.Circle };
            var notAnsweredSeries = new LineSeries { Title = "Non Date", MarkerType = MarkerType.Circle };

            foreach (var record in timelineData)
            {
                var x = DateTimeAxis.ToDouble(record.Date);
                correctSeries.Points.Add(new DataPoint(x, record.Corrette));
                wrongSeries.Points.Add(new DataPoint(x, record.Sbagliate));
                notAnsweredSeries.Points.Add(new DataPoint(x, record.NonDate));
            }

            plotModel.Series.Add(correctSeries);
            plotModel.Series.Add(wrongSeries);
            plotModel.Series.Add(notAnsweredSeries);

            StudentTimelineChart.Model = plotModel;
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            InfoPanel.Visibility = Visibility.Visible;
            PerformancePanel.Visibility = Visibility.Collapsed;
            TimelinePanel.Visibility = Visibility.Collapsed;
        }

        private void PerformanceButton_Click(object sender, RoutedEventArgs e)
        {
            InfoPanel.Visibility = Visibility.Collapsed;
            PerformancePanel.Visibility = Visibility.Visible;
            TimelinePanel.Visibility = Visibility.Collapsed;
        }

        private void TimelineButton_Click(object sender, RoutedEventArgs e)
        {
            InfoPanel.Visibility = Visibility.Collapsed;
            PerformancePanel.Visibility = Visibility.Collapsed;
            TimelinePanel.Visibility = Visibility.Visible;
        }
    }

    public class TimelineData
    {
        public DateTime Date { get; set; }
        public int Corrette { get; set; }
        public int Sbagliate { get; set; }
        public int NonDate { get; set; }
    }
}
