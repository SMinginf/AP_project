using QuizClient.Models;
using QuizClient.Services;
using QuizClient.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace QuizClient
{
    public partial class CategorySelectionWindow : Window
    {
        private List<Categoria> _tutteLeCategorie = new();
        private ObservableCollection<Categoria> _categorieFiltrate = new();
        public List<Categoria>? Selezionate { get; private set; }
        public bool Unione { get; set; } = false;

        private readonly CRUDService _CRUDService;
        private readonly string _jwtToken;
        private readonly string _ruolo;

        // Costruttore per creare una nuova categoria
        public CategorySelectionWindow(string jwt, Mode m)
        {
            InitializeComponent();
            switch (m)
            {
                case Mode.DaFinestra:
                    Filters.Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }

            _jwtToken = jwt;
            _CRUDService = new CRUDService(_jwtToken);
            _ruolo = JwtUtils.GetClaimAsString(_jwtToken, "ruolo"); 
            CaricaCategorie();
        }

        // Metodo per caricare le categorie pubbliche dal server
        private async void CaricaCategorie()
        {
            var result = new ServiceResult<List<Categoria>>();
            if (_ruolo == "Studente") { 
                result = await _CRUDService.GetCategoriePubblicheAsync();
            }
            else { result = await _CRUDService.GetCategorieByDocenteAsync();}

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

        // Metodo per filtrare le categorie in base al testo di ricerca
        private void FiltraCategorie(string filtro)
        {
            var categorieDaFiltrare = _tutteLeCategorie.Where(c =>
            (string.IsNullOrWhiteSpace(filtro) || c.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase)) &&
            (_ruolo != "Studente" || c.Pubblica));

            _categorieFiltrate.Clear();
            foreach (var cat in categorieDaFiltrare)
            {
                _categorieFiltrate.Add(cat);
            }


            CategoryListView.ItemsSource = null;
            CategoryListView.ItemsSource = _categorieFiltrate;
        }


        // Gestione degli eventi per la ricerca e il filtro delle categorie
        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => FiltraCategorie(SearchBox.Text);
        private void FilterChanged(object sender, RoutedEventArgs e) => FiltraCategorie(SearchBox.Text);

        // Gestione dei pulsanti di conferma e annullamento
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
