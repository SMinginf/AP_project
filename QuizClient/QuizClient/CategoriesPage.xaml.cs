/*
using QuizClient.Models;
using QuizClient.Services;
using QuizClient.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace QuizClient
{
    /// <summary>
    /// Logica di interazione per CategoriesPage.xaml
    /// </summary>
    public partial class CategoriesPage : Page
    {
        // Gestisce la view delle categorie. Viene usata come sorgente dati per la griglia (CategorieGrid) nella UI.
        // Ogni volta che questa collezione viene modificata (aggiunta, rimozione, aggiornamento),
        // la UI si aggiorna automaticamente grazie al binding WPF. Si usa per permettere la ricerca e il filtraggio delle categorie.
        public ObservableCollection<Categoria> Categorie { get; set; } = new ObservableCollection<Categoria>();

        // Contiene tutte le categorie disponibili, senza filtri. Rispecchia il contenuto del database.
        private List<Categoria> TutteLeCategorie = new List<Categoria>();

        private string _jwtToken;
        private CRUDService _crudService;

        public CategoriesPage(string tk)
        {
            _jwtToken = tk; // Salva il token JWT per eventuali chiamate API
            InitializeComponent();
            _crudService = new CRUDService(_jwtToken); // Inizializza il servizio CRUD con il token JWT

            // Controllo ruolo utente dal JWT. FrameworkElement: Puoi semplicemente usare this, perché la tua classe(CategoriesPage) eredita da Page, che a sua volta eredita da FrameworkElement.
            Utils.JwtUtils.ValidateDocenteRole(_jwtToken, this.NavigationService, this);


            CategoryListView.ItemsSource = Categorie;
            _crudService = new CRUDService(_jwtToken); // Inizializza il servizio CRUD con il token JWT
            CaricaCategorie();
        }

        private async void CaricaCategorie()
        {
            var result = await _crudService.GetCategorieByDocenteAsync(); 
            if (result.Success && result.Data != null)
            {
                TutteLeCategorie = result.Data;
            }
            else
            {
                MessageBox.Show(result.ErrorMessage ?? "Errore nel caricamento delle categorie.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            FiltraCategorie(string.Empty);
        }

        //gestore eventi checked
        private void FiltroPubblica_Checked(object sender, RoutedEventArgs e)
        {
            // Se si seleziona "Solo pubbliche", deseleziona "Solo private"
            if (FiltroPubbliche.IsChecked == true)
                FiltroPrivate.IsChecked = false;
            FiltraCategorie(SearchBox?.Text ?? string.Empty);
        }
        private void FiltroPrivata_Checked(object sender, RoutedEventArgs e)
        {
            // Se si seleziona "Solo private", deseleziona "Solo pubbliche"
            if (FiltroPrivate.IsChecked == true)
                FiltroPubbliche.IsChecked = false;
            FiltraCategorie(SearchBox?.Text ?? string.Empty);
        }
        private void FiltraCategorie(string filtro)
        {
            bool soloPubbliche = FiltroPubbliche?.IsChecked == true;
            bool soloPrivate = FiltroPrivate?.IsChecked == true;

            Categorie.Clear();
            foreach (var cat in TutteLeCategorie.Where(c => // string.IsNullOrWhiteSpace(filtro) = Se il testo di ricerca (filtro) è vuoto, nullo o solo spazi, questa condizione è vera per tutte le categorie (nessun filtro per nome).
                (string.IsNullOrWhiteSpace(filtro) || c.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase)) &&
                (
                    (!soloPubbliche && !soloPrivate) ||
                    (soloPubbliche && c.Pubblica) ||
                    (soloPrivate && !c.Pubblica)
                )
            ))
            {
                Categorie.Add(cat);
            }
        }

        // gestore dell'evento TextChanged per il TextBox di ricerca
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var filtro = (sender as TextBox)?.Text ?? string.Empty;
            FiltraCategorie(filtro);
        }

        // gestore dell'evento unchecked per i checkbox di filtro
        private void Filtri_Unchecked(object sender, RoutedEventArgs e)
        {
            FiltraCategorie(SearchBox?.Text ?? string.Empty);
        }

        private async void NuovaCategoria_Click(object sender, RoutedEventArgs e)
        {
            var finestra = new CategoryDialogWindow(_jwtToken);
            if (finestra.ShowDialog() == true)
            {
                
                var result = await _crudService.CreateCategoriaAsync(finestra.CategoriaCreata);
                if (result.Success && result.Data != null )
                {
                    MessageBox.Show("Categoria creata con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);

                    TutteLeCategorie.Add(finestra.CategoriaCreata);
                    FiltraCategorie((SearchBox?.Text) ?? string.Empty);
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage ?? "Errore nella creazione della categoria.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void CategoryListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Verifica che il doppio click sia su una riga e non sull'header
            var listViewItem = ItemsControl.ContainerFromElement(CategoryListView, e.OriginalSource as DependencyObject) as ListViewItem;

            if (listViewItem != null && listViewItem.DataContext is Categoria selezionata)
            {
                var finestra = new CategoryDialogWindow(selezionata, _jwtToken);
                if (finestra.ShowDialog() == true)
                {
                    var result = await _crudService.UpdateCategoriaAsync(finestra.CategoriaCreata);
                    if (result.Success && result.Data != null)
                    {
                        MessageBox.Show("Categoria aggiornata con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                        int index = TutteLeCategorie.IndexOf(selezionata);
                        if (index >= 0)
                            TutteLeCategorie[index] = finestra.CategoriaCreata;
                        FiltraCategorie((SearchBox?.Text) ?? string.Empty);
                    }
                    else
                    {
                        MessageBox.Show(result.ErrorMessage ?? "Errore nell'aggiornamento della categoria.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

       
        private async void EliminaCategorie_Click(object sender, RoutedEventArgs e)
        {
            var selezionate = TutteLeCategorie.Where(c => c.Selezionata).ToList();
            if (selezionate.Count == 0)
            {
                MessageBox.Show("Seleziona almeno una categoria da eliminare.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var conferma = MessageBox.Show(
                $"Vuoi eliminare {selezionate.Count} categorie selezionate?",
                "Conferma eliminazione",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (conferma == MessageBoxResult.Yes)
            {
                // Prepara la lista degli ID da eliminare
                var ids = selezionate.Select(c => c.ID).ToList();

                System.Diagnostics.Debug.WriteLine("Contenuto di ids:" + string.Join(", ", ids));


                // Chiamata unica al servizio per eliminazione multipla
                var result = await _crudService.DeleteCategoriaAsync(ids);

                if (result.Success)
                {
                    MessageBox.Show("Categorie eliminate con successWo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Rimuovi localmente le categorie selezionate
                    foreach (var cat in selezionate)
                    {
                        TutteLeCategorie.Remove(cat);
                        FiltraCategorie((SearchBox?.Text) ?? string.Empty);
                    }
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage ?? "Errore nell'eliminazione delle categorie.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    // In caso di errore, ricarica le categorie per coerenza
                    CaricaCategorie();
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                this.IsEnabled = false;
        }

        private void SelezionaTutteLeCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox headerCheckBox)
            {
                bool seleziona = headerCheckBox.IsChecked == true;

                // Imposta la proprietà Selezionata di tutte le categorie mostrate nella griglia
                foreach (var cat in Categorie)
                {
                    cat.Selezionata = seleziona;
                }
            }
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
using System.Windows.Input;

namespace QuizClient
{
    /// <summary>
    /// Logica di interazione per CategoriesPage.xaml
    /// </summary>
    public partial class CategoriesPage : Page, IDisposable
    {
        // Gestisce la view delle categorie. Viene usata come sorgente dati per la griglia (CategorieGrid) nella UI.
        // Ogni volta che questa collezione viene modificata (aggiunta, rimozione, aggiornamento),
        // la UI si aggiorna automaticamente grazie al binding WPF. Si usa per permettere la ricerca e il filtraggio delle categorie.
        public ObservableCollection<Categoria> Categorie { get; } = new();

        // Contiene tutte le categorie disponibili, senza filtri. Rispecchia il contenuto del database.
        private List<Categoria> _tutteLeCategorie = new();

        private readonly string _jwtToken;
        private readonly CRUDService _crudService;
        private bool _disposed;

        public CategoriesPage(string token)
        {
            InitializeComponent();
            _jwtToken = token;
            _crudService = new CRUDService(_jwtToken); // Inizializza il servizio CRUD con il token JWT

            // Controllo ruolo utente dal JWT.
            JwtUtils.ValidateDocenteRole(_jwtToken, NavigationService, this);

            CategoryListView.ItemsSource = Categorie;
            CaricaCategorie();
        }

        private async void CaricaCategorie()
        {
            var result = await _crudService.GetCategorieByDocenteAsync();

            if (!result.Success || result.Data is null)
            {
                MessageBox.Show(result.ErrorMessage ?? "Errore nel caricamento delle categorie.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _tutteLeCategorie = result.Data;
            FiltraCategorie(string.Empty);
        }

        // Gestore eventi Checked
        private void FiltroPubblica_Checked(object sender, RoutedEventArgs e)
        {
            // Se si seleziona "Solo pubbliche", deseleziona "Solo private"
            if (FiltroPubbliche.IsChecked == true)
                FiltroPrivate.IsChecked = false;

            FiltraCategorie(SearchBox?.Text ?? string.Empty);
        }

        private void FiltroPrivata_Checked(object sender, RoutedEventArgs e)
        {
            // Se si seleziona "Solo private", deseleziona "Solo pubbliche"
            if (FiltroPrivate.IsChecked == true)
                FiltroPubbliche.IsChecked = false;

            FiltraCategorie(SearchBox?.Text ?? string.Empty);
        }

        private void Filtri_Unchecked(object sender, RoutedEventArgs e)
        {
            FiltraCategorie(SearchBox?.Text ?? string.Empty);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FiltraCategorie((sender as TextBox)?.Text ?? string.Empty);
        }

        private void FiltraCategorie(string filtro)
        {
            bool soloPubbliche = FiltroPubbliche?.IsChecked == true;
            bool soloPrivate = FiltroPrivate?.IsChecked == true;

            Categorie.Clear();

            var filtrate = _tutteLeCategorie
                .Where(c =>
                    (string.IsNullOrWhiteSpace(filtro) || c.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase)) &&
                    (
                        (!soloPubbliche && !soloPrivate) ||
                        (soloPubbliche && c.Pubblica) ||
                        (soloPrivate && !c.Pubblica)
                    ))
                .ToList();

            foreach (var categoria in filtrate)
                Categorie.Add(categoria);
        }

        private async void NuovaCategoria_Click(object sender, RoutedEventArgs e)
        {
            var finestra = new CategoryDialogWindow(_jwtToken);
            if (finestra.ShowDialog() != true) return;

            var result = await _crudService.CreateCategoriaAsync(finestra.CategoriaCreata);
            if (result.Success && result.Data != null)
            {
                MessageBox.Show("Categoria creata con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                _tutteLeCategorie.Add(finestra.CategoriaCreata);
                FiltraCategorie(SearchBox?.Text ?? string.Empty);
            }
            else
            {
                MessageBox.Show(result.ErrorMessage ?? "Errore nella creazione della categoria.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CategoryListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Verifica che il doppio click sia su una riga e non sull'header
            if (ItemsControl.ContainerFromElement(CategoryListView, e.OriginalSource as DependencyObject) is not ListViewItem listViewItem)
                return;

            if (listViewItem.DataContext is not Categoria selezionata)
                return;

            var finestra = new CategoryDialogWindow(selezionata, _jwtToken);
            if (finestra.ShowDialog() != true) return;

            var result = await _crudService.UpdateCategoriaAsync(finestra.CategoriaCreata);
            if (result.Success && result.Data != null)
            {
                MessageBox.Show("Categoria aggiornata con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);

                int index = _tutteLeCategorie.IndexOf(selezionata);
                if (index >= 0)
                    _tutteLeCategorie[index] = finestra.CategoriaCreata;

                FiltraCategorie(SearchBox?.Text ?? string.Empty);
            }
            else
            {
                MessageBox.Show(result.ErrorMessage ?? "Errore nell'aggiornamento della categoria.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EliminaCategorie_Click(object sender, RoutedEventArgs e)
        {
            var selezionate = _tutteLeCategorie.Where(c => c.Selezionata).ToList();

            if (selezionate.Count == 0)
            {
                MessageBox.Show("Seleziona almeno una categoria da eliminare.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var conferma = MessageBox.Show(
                $"Vuoi eliminare {selezionate.Count} categorie selezionate?",
                "Conferma eliminazione",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (conferma != MessageBoxResult.Yes)
                return;

            var ids = selezionate.Select(c => c.ID).ToList();

            var result = await _crudService.DeleteCategoriaAsync(ids);
            if (result.Success)
            {
                MessageBox.Show("Categorie eliminate con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                _tutteLeCategorie.RemoveAll(c => selezionate.Contains(c));
                FiltraCategorie(SearchBox?.Text ?? string.Empty);
            }
            else
            {
                MessageBox.Show(result.ErrorMessage ?? "Errore nell'eliminazione delle categorie.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                CaricaCategorie();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
            else
                IsEnabled = false;
        }

        private void SelezionaTutteLeCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox headerCheckBox) return;

            bool seleziona = headerCheckBox.IsChecked == true;

            // Imposta la proprietà Selezionata di tutte le categorie mostrate nella griglia
            foreach (var categoria in Categorie)
                categoria.Selezionata = seleziona;
        }


        public void Dispose()
        {
            // Best practice: controllo con flag per garantire idempotenza
            if (_disposed)
                return;
            _crudService.Dispose();
            _disposed = true;

            // Sopprime la finalizzazione se non è necessaria (buona pratica):
            // permette di saltare l'esecuzione del finalizzatore, poichè le risorse critiche sono già
            // state rilasciate, e pulire la sua memoria nel modo più veloce possibile.
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
