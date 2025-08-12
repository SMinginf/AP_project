using QuizClient.Models;
using QuizClient.Services;
using QuizClient.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
<<<<<<< HEAD
using System.Windows.Navigation;
=======
using System.Windows.Controls;
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba

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
<<<<<<< HEAD
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
=======
        
        private bool IsFromStatsPage = false; // Indica se la finestra è stata aperta dalla pagina delle statistiche. Mi serve per riutilizzare la pagina cambiando qualcosina.
        private string _ruolo="";
        public CategorySelectionWindow(string jwt, bool isFromStatsPage = false)
        {
            InitializeComponent();
            _jwtToken = jwt;
            _CRUDService = new CRUDService(_jwtToken);

            try
            {
                //ricavo il ruolo dell'utente dal token JWT
                _ruolo = JwtUtils.GetClaimAsString(_jwtToken, "ruolo");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel recupero del ruolo utente dal token JWT: {ex.Message}", "Errore JWT", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            IsFromStatsPage = isFromStatsPage;

            Personalizza();
            CaricaCategorie();
        }


        private void Personalizza()
        {
            if (IsFromStatsPage)
            {
                CategoryModePanel.Visibility = Visibility.Collapsed;

                if (_ruolo == "Docente") //aggiungo la colonna Visibilità solo se la finestra è stata aperta dalla pagina delle statistiche docente
                {
                    var gridView = CategoryListView.View as GridView;
                    if (gridView != null)
                    {
                        var visibilitaColumn = new GridViewColumn
                        {
                            Header = "Visibilità",
                            Width = 60,
                            CellTemplate = (DataTemplate)FindResource("VisibilitaCellTemplate"),
                        };
                        gridView.Columns.Add(visibilitaColumn);
                    }
                }

            }


        }
        private async void CaricaCategorie()
        {
            ServiceResult<List<Categoria>> result = new();

            if (IsFromStatsPage)
            {        
                if (_ruolo == "Studente")
                {
                    // Se la finestra è stata aperta dalla pagina delle statistiche studente, mostro le categorie affrontate dallo studente
                    result = await _CRUDService.GetCategorieByStudenteAsync();
                }
                else if (_ruolo == "Docente")
                {
                    // Se la finestra è stata aperta dalla pagina delle statistiche docente, mostro tutte le categorie create dal docente
                    result = await _CRUDService.GetCategorieByDocenteAsync(); //devo fare il get di tutte le categorie di un docente
                }
            }
            else
                result = await _CRUDService.GetCategoriePubblicheAsync();
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba

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

<<<<<<< HEAD
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


=======
        private void FiltraCategorie(string filtro)
        {
            _categorieFiltrate.Clear();
            foreach (var cat in _tutteLeCategorie.Where(c =>
                c.Pubblica && (string.IsNullOrWhiteSpace(filtro) || c.Nome.Contains(filtro, System.StringComparison.OrdinalIgnoreCase))))
            {
                _categorieFiltrate.Add(cat);
            }
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
            CategoryListView.ItemsSource = null;
            CategoryListView.ItemsSource = _categorieFiltrate;
        }


<<<<<<< HEAD
        // Gestione degli eventi per la ricerca e il filtro delle categorie
        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => FiltraCategorie(SearchBox.Text);
        private void FilterChanged(object sender, RoutedEventArgs e) => FiltraCategorie(SearchBox.Text);

        // Gestione dei pulsanti di conferma e annullamento
        private void Annulla_Click(object sender, RoutedEventArgs e) => Close();
=======

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => FiltraCategorie(SearchBox.Text);
        private void FilterChanged(object sender, RoutedEventArgs e) => FiltraCategorie(SearchBox.Text);

        private void Annulla_Click(object sender, RoutedEventArgs e) => Close();

>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
        private void Conferma_Click(object sender, RoutedEventArgs e)
        {
            Unione = UnionOption.IsChecked == true;
            Selezionate = _categorieFiltrate.Where(c => c.Selezionata).ToList();
            DialogResult = true;
            Close();
        }
    }
}
