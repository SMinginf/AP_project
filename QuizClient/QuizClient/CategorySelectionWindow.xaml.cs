using QuizClient.Models;
using QuizClient.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using QuizClient.Utils;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace QuizClient
{
    public partial class CategorySelectionWindow : Window
    {
        private List<Categoria> _tutteLeCategorie = new();
        private ObservableCollection<Categoria> _categorieFiltrate = new();
        public List<Categoria>? Selezionate { get; private set; }
        public bool Unione { get; set; } = false;

        private readonly CRUDService _CRUDService;
        private string _jwtToken;

        public CategorySelectionWindow(string jwt)
        {
            InitializeComponent();
            _jwtToken = jwt;
            _CRUDService = new CRUDService(_jwtToken);
            CaricaCategorie();
        }


        private async void CaricaCategorie()
        {
            var result = await _CRUDService.GetCategorieByDocenteAsync();
            if (result.Success && result.Data != null)
            {
                _tutteLeCategorie = result.Data;
                FiltraCategorie(SearchBox.Text);
            }
            else
            {
                MessageBox.Show(result.ErrorMessage ?? "Errore nel caricamento delle categorie.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void FiltraCategorie(string filtro)
        {
            _categorieFiltrate.Clear();
            foreach (var cat in _tutteLeCategorie.Where(c =>
                c.Pubblica && (string.IsNullOrWhiteSpace(filtro) || c.Nome.Contains(filtro, System.StringComparison.OrdinalIgnoreCase))))
            {
                _categorieFiltrate.Add(cat);
            }
            CategoryListView.ItemsSource = null;
            CategoryListView.ItemsSource = _categorieFiltrate;
        }



        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => FiltraCategorie(SearchBox.Text);
        private void FilterChanged(object sender, RoutedEventArgs e) => FiltraCategorie(SearchBox.Text);

        private void Annulla_Click(object sender, RoutedEventArgs e) => Close();

        private void Conferma_Click(object sender, RoutedEventArgs e)
        {
            Unione = UnionOption.IsChecked == true;
            Selezionate = _categorieFiltrate.Where(c => c.Selezionata).ToList();
            DialogResult = true;
            Close();
        }
    }
}
