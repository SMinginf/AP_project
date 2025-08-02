using QuizClient.Models;
using QuizClient.Services;
using QuizClient.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
