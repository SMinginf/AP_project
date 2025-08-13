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
            if (listViewItem == null)
                return;

            if (listViewItem.DataContext is Categoria selezionata)
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
