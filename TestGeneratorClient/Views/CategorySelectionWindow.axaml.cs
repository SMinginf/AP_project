using Avalonia.Controls;
using Avalonia.Interactivity;
using TestGeneratorClient.Models;
using TestGeneratorClient.Services;
using TestGeneratorClient.Utils;
using System.Collections.ObjectModel;

namespace TestGeneratorClient.Views;

public partial class CategorySelectionWindow : Window
{
    private List<Categoria> _tutteLeCategorie = new();
    private ObservableCollection<Categoria> _categorieFiltrate = new();
    public List<Categoria>? Selezionate { get; private set; }
    public bool Unione { get; set; }
    private readonly CRUDService _CRUDService;
    private readonly string _jwtToken;

    public CategorySelectionWindow(string jwt)
    {
        InitializeComponent();
        _jwtToken = jwt;
        _CRUDService = new CRUDService(_jwtToken);
        CaricaCategorie();
    }

    private async void CaricaCategorie()
    {
        var result = await _CRUDService.GetCategoriePubblicheAsync();
        if (result.Success && result.Data != null)
        {
            _tutteLeCategorie = result.Data;
            FiltraCategorie(SearchBox.Text);
        }
        else
        {
            await MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow("Errore", result.ErrorMessage ?? "Errore nel caricamento delle categorie.")
                .ShowDialog(this);
            Close();
        }
    }

    private void FiltraCategorie(string filtro)
    {
        _categorieFiltrate.Clear();
        foreach (var cat in _tutteLeCategorie.Where(c => c.Pubblica && (string.IsNullOrWhiteSpace(filtro) || c.Nome.Contains(filtro, System.StringComparison.OrdinalIgnoreCase))))
        {
            _categorieFiltrate.Add(cat);
        }
        CategoryListView.ItemsSource = null;
        CategoryListView.ItemsSource = _categorieFiltrate;
    }

    private void SearchBox_TextChanged(object? sender, RoutedEventArgs e) => FiltraCategorie(SearchBox.Text ?? string.Empty);
    private void Annulla_Click(object? sender, RoutedEventArgs e) => Close();

    private void Conferma_Click(object? sender, RoutedEventArgs e)
    {
        Unione = UnionOption.IsChecked == true;
        Selezionate = _categorieFiltrate.Where(c => c.Selezionata).ToList();
        DialogResult = true;
        Close();
    }
}
