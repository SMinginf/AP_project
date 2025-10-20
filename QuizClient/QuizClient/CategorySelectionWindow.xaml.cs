/*
using QuizClient.Models;
using QuizClient.Services;
using QuizClient.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        
        private string _ruolo="";
        public CategorySelectionWindow(string jwt, Mode m)
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


            Personalizza();

            if (m == Mode.Default)
            {
                CategoryModePanel.Visibility = Visibility.Collapsed;
            }
           

            CaricaCategorie(m);
        }


        private void Personalizza()
        {
            CategoryModePanel.Visibility = Visibility.Collapsed;

            if (_ruolo == "Docente") //aggiungo la colonna Visibilità solo se la finestra è stata aperta da un utente col ruolo di docente
            {
                var gridView = CategoryListView.View as GridView;
                if (gridView != null)
                {
                    // Rimuovi la colonna con l'username del docente
                    gridView.Columns.Remove(CreatoreColumn);

                    // Aggiungi la colonna Visibilità
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

        private async void CaricaCategorie( Mode m)
        {
            ServiceResult<List<Categoria>> result = new();
            switch (m) {

                case Mode.StatsPage:
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
                break;

                case Mode.DaFinestra:
                    result = await _CRUDService.GetCategorieByDocenteAsync();
                break;


                default:
                    result = await _CRUDService.GetCategoriePubblicheAsync();
                break;
            }

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
             (_ruolo != "Studente" || c.Pubblica) && (string.IsNullOrWhiteSpace(filtro) || c.Nome.Contains(filtro, System.StringComparison.OrdinalIgnoreCase))))
            {
                _categorieFiltrate.Add(cat);
            }

            // SQ
            //var categorieDaFiltrare = _tutteLeCategorie.Where(c =>
            //(string.IsNullOrWhiteSpace(filtro) || c.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase)) &&
            //(_ruolo != "Studente" || c.Pubblica));

            //_categorieFiltrate.Clear();
            //foreach (var cat in categorieDaFiltrare)
            //{
            //    _categorieFiltrate.Add(cat);
            //}


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
*/

using QuizClient.Models;
using QuizClient.Services;
using QuizClient.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QuizClient
{
    public partial class CategorySelectionWindow : Window, IDisposable
    {
        private readonly CRUDService _crudService;
        private bool _disposed;
        private readonly string _jwtToken;
        private readonly string _ruolo="";

        private List<Categoria> _tutteLeCategorie = new();
        private readonly ObservableCollection<Categoria> _categorieFiltrate = new();

        public List<Categoria>? Selezionate { get; private set; }
        public bool Unione { get; private set; }

        // --------------------------- //
        //         COSTRUTTORE         //
        // --------------------------- //

        public CategorySelectionWindow(string jwtToken, Mode mode)
        {
            InitializeComponent();

            _jwtToken = jwtToken;
            _crudService = new CRUDService(_jwtToken);

            try
            {
                // Ricava il ruolo dell'utente dal token JWT
                _ruolo = JwtUtils.GetClaimAsString(_jwtToken, "ruolo");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel recupero del ruolo utente dal token JWT:\n{ex.Message}",
                    "Errore JWT", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            Personalizza();
            if (mode == Mode.Default)
                CategoryModePanel.Visibility = Visibility.Collapsed;

            CaricaCategorie(mode);
        }

        // --------------------------- //
        //     CONFIGURAZIONE VIEW     //
        // --------------------------- //

        private void Personalizza()
        {
            CategoryModePanel.Visibility = Visibility.Collapsed;

            // Aggiunge la colonna “Visibilità” solo se l'utente è un docente
            if (_ruolo != "Docente")
                return;

            if (CategoryListView.View is not GridView gridView)
                return;

            // Rimuove la colonna con l'username del creatore
            gridView.Columns.Remove(CreatoreColumn);

            // Aggiunge la colonna “Visibilità” con il template appropriato
            var visibilitaColumn = new GridViewColumn
            {
                Header = "Visibilità",
                Width = 60,
                CellTemplate = (DataTemplate)FindResource("VisibilitaCellTemplate")
            };

            gridView.Columns.Add(visibilitaColumn);
        }

        // --------------------------- //
        //     CARICAMENTO DATI        //
        // --------------------------- //

        private async void CaricaCategorie(Mode mode)
        {
            ServiceResult<List<Categoria>> result;

            switch (mode)
            {
                case Mode.StatsPage:
                    result = _ruolo switch
                    {
                        "Studente" => await _crudService.GetCategorieByStudenteAsync(),
                        "Docente" => await _crudService.GetCategorieByDocenteAsync(),
                        _ => new ServiceResult<List<Categoria>> { ErrorMessage = "Ruolo utente non riconosciuto." }
                    };
                    break;

                case Mode.DaFinestra:
                    result = await _crudService.GetCategorieByDocenteAsync();
                    break;

                default:
                    result = await _crudService.GetCategoriePubblicheAsync();
                    break;
            }

            if (!result.Success || result.Data is null)
            {
                MessageBox.Show(result.ErrorMessage ?? "Errore nel caricamento delle categorie.",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            _tutteLeCategorie = result.Data;
            FiltraCategorie(SearchBox.Text);
        }

        private void FiltraCategorie(string filtro)
        {
            _categorieFiltrate.Clear();

            var categorieDaMostrare = _tutteLeCategorie
                .Where(c =>
                    // Mostra solo categorie pubbliche per gli studenti
                    (_ruolo != "Studente" || c.Pubblica) &&
                    // Applica il filtro testuale
                    (string.IsNullOrWhiteSpace(filtro) ||
                     c.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var categoria in categorieDaMostrare)
                _categorieFiltrate.Add(categoria);

            CategoryListView.ItemsSource = _categorieFiltrate;
        }

        // --------------------------- //
        //     EVENTI INTERFACCIA      //
        // --------------------------- //

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
            => FiltraCategorie(SearchBox.Text);

        private void FilterChanged(object sender, RoutedEventArgs e)
            => FiltraCategorie(SearchBox.Text);

        private void Annulla_Click(object sender, RoutedEventArgs e)
            => Close();

        private void Conferma_Click(object sender, RoutedEventArgs e)
        {
            Unione = UnionOption.IsChecked == true;
            Selezionate = _categorieFiltrate.Where(c => c.Selezionata).ToList();

            DialogResult = true;
            Close();
        }

        // --------------------------- //
        //           DISPOSE           //
        // --------------------------- //
        public void Dispose()
        {
            if (_disposed) return;

            _crudService.Dispose();

            _disposed = true;
            GC.SuppressFinalize(this);
        }


        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            // Chiama il metodo Dispose() quando la finestra non è più visualizzata
            (this as IDisposable)?.Dispose();
        }
    }
}
