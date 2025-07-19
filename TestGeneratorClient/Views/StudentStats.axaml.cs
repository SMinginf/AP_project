using Avalonia.Controls;
using Avalonia.Interactivity;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using TestGeneratorClient.Services;
using TestGeneratorClient.Models;

namespace TestGeneratorClient.Views;

public partial class StudentStats : UserControl
{
    private readonly AnalyticsService analyticsService;
    private readonly string _jwtToken;
    private StudentStatsResponse? _stats;
    private readonly MainWindow _mainWindow;

    public StudentStats(string jwtToken, MainWindow mainWindow)
    {
        InitializeComponent();
        _jwtToken = jwtToken;
        _mainWindow = mainWindow;
        analyticsService = new AnalyticsService(_jwtToken);
        LoadStudentStats();
    }

    private async void LoadStudentStats()
    {
        var result = await analyticsService.GetStudentStatsAsync();
        if (result == null || !result.Success || result.Data == null)
        {
            await MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow("Errore", "Errore nel caricamento delle statistiche dello studente: " + result?.ErrorMessage)
                .ShowDialog(_mainWindow);
            return;
        }
        _stats = result.Data;
        var studente = _stats.Studente;
        NameText.Text = $"{studente.Nome} {studente.Cognome}";
        EmailText.Text = studente.Email;
        BirthDateText.Text = studente.DataNascita.ToShortDateString();
        GenderText.Text = studente.Genere;

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

    private void InfoButton_Click(object? sender, RoutedEventArgs e)
    {
        InfoPanel.IsVisible = true;
        PerformancePanel.IsVisible = false;
        TimelinePanel.IsVisible = false;
    }

    private void PerformanceButton_Click(object? sender, RoutedEventArgs e)
    {
        InfoPanel.IsVisible = false;
        PerformancePanel.IsVisible = true;
        TimelinePanel.IsVisible = false;
    }

    private void TimelineButton_Click(object? sender, RoutedEventArgs e)
    {
        InfoPanel.IsVisible = false;
        PerformancePanel.IsVisible = false;
        TimelinePanel.IsVisible = true;
    }
}
