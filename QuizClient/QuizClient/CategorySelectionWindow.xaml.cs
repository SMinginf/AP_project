using QuizClient.Models;
using QuizClient.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace QuizClient
{
    public partial class CategorySelectionWindow : Window
    {
        private List<Categoria>? _categorieDisponibili;
        public List<Categoria>? Selezionate { get; private set; }
        public bool Unione { get; set; } = false; // Indica se le categorie selezionate devono essere unite o meno

        private readonly CatService _catService;
        private string _jwtToken;

        public CategorySelectionWindow(string jwt)
        {
            InitializeComponent();
            _jwtToken = jwt;
            _catService = new CatService(_jwtToken);
            Loaded += CategorySelectionWindow_Loaded;
        }

        // Prima, il caricamento delle categorie veniva effettuato nel costruttore della finestra usando il metodo sincrono .Result su una chiamata asincrona.
        // Questo approccio causava un blocco (deadlock) dell'interfaccia grafica in WPF, impedendo la visualizzazione della finestra CategorySelectionWindow.
        // Ora, il caricamento delle categorie è stato spostato nell'evento Loaded della finestra e viene eseguito in modo asincrono tramite await.
        // In questo modo, la finestra viene visualizzata immediatamente e le categorie vengono caricate senza bloccare l'UI, risolvendo il problema di mancata visualizzazione.
        private async void CategorySelectionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CaricaCategorieAsync();
            AggiornaLista();
        }

        
        private async Task CaricaCategorieAsync()
        {
            _categorieDisponibili = await _catService.GetCategorieAsync() ?? new List<Categoria>();
            if (!_categorieDisponibili.Any())
            {
                MessageBox.Show("Nessuna categoria disponibile.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
            }
        }


        private void AggiornaLista()
        {
            if (_categorieDisponibili == null)
            {
                CategoryListView.ItemsSource = null;
                return;
            }

            var filtro = SearchBox.Text.ToLower();

            // Filtra le categorie in base al nome
            var filtrate = _categorieDisponibili.Where(cat =>
                cat.Pubblica && cat.Nome.ToLower().Contains(filtro)).ToList();

            CategoryListView.ItemsSource = filtrate;
        }
        /*
         ItemsSource è una proprietà usata nei controlli WPF come ListView, ComboBox, DataGrid ecc. Serve per collegare (bindare) una collezione di oggetti al controllo, così che ogni elemento della collezione venga visualizzato come una riga o voce nell’interfaccia.
        Come funziona il binding agli oggetti:
        •	Quando imposti CategoryListView.ItemsSource = filtrate;, ogni oggetto della lista filtrate (ad esempio, ogni Categoria) diventa il DataContext di una riga della ListView.
        •	Nel file XAML puoi poi usare {Binding Nome} o {Binding Docente.Username} per mostrare le proprietà di ogni oggetto. 
         */
        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => AggiornaLista();
        private void FilterChanged(object sender, RoutedEventArgs e) => AggiornaLista();

        private void Annulla_Click(object sender, RoutedEventArgs e) => Close();

        private void Conferma_Click(object sender, RoutedEventArgs e)
        {
            Unione = UnionOption.IsChecked== true;
            Selezionate = _categorieDisponibili?.Where(c => c.Selezionata).ToList() ?? new List<Categoria>();

            DialogResult = true;
            Close();
        }
    }
}
