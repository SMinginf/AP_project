using OxyPlot;
using OxyPlot.Series;
using QuizClient.Services;
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
        private string _jwtToken; // Token JWT per autenticazione
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

            var studentStats = result.Data; // Ottieni i dati delle statistiche dello studente

            // Popola il DataGrid con le statistiche per categoria e difficoltà
            StudentStatsDataGrid.ItemsSource = studentStats.StatsPerCategoriaDifficolta;

            // Imposta i dati per il grafico temporale
            var timelineData = studentStats.AndamentoTemporale;
            var plotModel = new PlotModel { Title = "Andamento Temporale" };

            // Aggiungiamo le serie per "Corrette", "Sbagliate" e "Non Date"
            var correctSeries = new BarSeries
            {
                Title = "Corrette",
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0}"
            };

            var wrongSeries = new BarSeries
            {
                Title = "Sbagliate",
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0}"
            };

            var notAnsweredSeries = new BarSeries
            {
                Title = "Non Date",
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0}"
            };

            foreach (var record in timelineData)
            {
                correctSeries.Items.Add(new BarItem { Value = record.Corrette });
                wrongSeries.Items.Add(new BarItem { Value = record.Sbagliate });
                notAnsweredSeries.Items.Add(new BarItem { Value = record.NonDate });
            }

            // Aggiungiamo le serie al grafico
            plotModel.Series.Add(correctSeries);
            plotModel.Series.Add(wrongSeries);
            plotModel.Series.Add(notAnsweredSeries);

            // Impostiamo il grafico
            StudentTimelineChart.Model = plotModel;
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
