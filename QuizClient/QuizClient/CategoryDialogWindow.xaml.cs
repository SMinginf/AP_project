/*
using QuizClient.Models;
using QuizClient.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuizClient
{
    /// <summary>
    /// Logica di interazione per CategoryDialogWindow.xaml
    /// </summary>
    public partial class CategoryDialogWindow : Window
    {
        public Categoria CategoriaCreata { get; private set; }

        private string _jwtToken;

        public CategoryDialogWindow(string tk)
        {
            _jwtToken = tk; 
            InitializeComponent();
            CategoriaCreata = new Categoria(); // Inizializza la categoria creata

            // Controllo ruolo utente dal JWT
            Utils.JwtUtils.ValidateDocenteRole(_jwtToken, null, this);
        }

        // Costruttore chiamato per modificare una categoria esistente
        public CategoryDialogWindow(Categoria esistente, string tk) : this(tk)
        {
            NomeBox.Text = esistente.Nome;
            TipoBox.Text = esistente.Tipo;
            DescrizioneBox.Text = esistente.Descrizione;
            PubblicaRadio.IsChecked = esistente.Pubblica;
            PrivataRadio.IsChecked = !esistente.Pubblica;

            // Crea una copia per evitare modifiche all'oggetto originale
            CategoriaCreata = new Categoria
            {
                ID = esistente.ID,
                Nome = esistente.Nome,
                Tipo = esistente.Tipo,
                Descrizione = esistente.Descrizione,
                Pubblica = esistente.Pubblica,
                IDDocente = esistente.IDDocente,
                Docente = esistente.Docente,
                Quesiti = esistente.Quesiti
            };
        }


        private void Salva_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NomeBox.Text))
            {
                MessageBox.Show("Il nome è obbligatorio.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(DescrizioneBox.Text))
            {
                MessageBox.Show("La descrizione è obbligatoria.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (PubblicaRadio.IsChecked == false && PrivataRadio.IsChecked == false)
            {
                MessageBox.Show("Devi specificare se la categoria è pubblica o privata.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            uint idDocente;
            try
            {
                idDocente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel recupero dell'ID docente dal token JWT: {ex.Message}", "Errore JWT", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CategoriaCreata.Nome = NomeBox.Text.Trim();
            CategoriaCreata.Tipo = TipoBox.Text.Trim();
            CategoriaCreata.Descrizione = DescrizioneBox.Text.Trim();
            CategoriaCreata.Pubblica = PubblicaRadio.IsChecked == true;
            CategoriaCreata.IDDocente = idDocente;

            
            DialogResult = true;
        }

        private void Annulla_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
*/

using QuizClient.Models;
using QuizClient.Utils;
using System;
using System.Windows;

namespace QuizClient
{
    public partial class CategoryDialogWindow : Window
    {
        public Categoria CategoriaCreata { get; private set; }

        private readonly string _jwtToken;

        // --------------------------- //
        //         COSTRUTTORI         //
        // --------------------------- //

        public CategoryDialogWindow(string jwtToken)
        {
            InitializeComponent();

            _jwtToken = jwtToken;
            CategoriaCreata = new Categoria();

            // Controllo ruolo utente dal JWT (solo docente può aprire)
            JwtUtils.ValidateDocenteRole(_jwtToken, null, this);
        }

        // Costruttore chiamato per modificare una categoria esistente
        public CategoryDialogWindow(Categoria esistente, string jwtToken) : this(jwtToken)
        {
            // Popola i campi della finestra con i dati esistenti
            NomeBox.Text = esistente.Nome;
            TipoBox.Text = esistente.Tipo;
            DescrizioneBox.Text = esistente.Descrizione;
            PubblicaRadio.IsChecked = esistente.Pubblica;
            PrivataRadio.IsChecked = !esistente.Pubblica;

            // Crea una copia per evitare modifiche all'oggetto originale
            CategoriaCreata = new Categoria
            {
                ID = esistente.ID,
                Nome = esistente.Nome,
                Tipo = esistente.Tipo,
                Descrizione = esistente.Descrizione,
                Pubblica = esistente.Pubblica,
                IDDocente = esistente.IDDocente,
                Docente = esistente.Docente,
                Quesiti = esistente.Quesiti
            };
        }

        // --------------------------- //
        //     EVENTI BUTTON CLICK     //
        // --------------------------- //

        private void Salva_Click(object sender, RoutedEventArgs e)
        {
            // Validazioni base
            if (string.IsNullOrWhiteSpace(NomeBox.Text))
            {
                MessageBox.Show("Il nome è obbligatorio.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(DescrizioneBox.Text))
            {
                MessageBox.Show("La descrizione è obbligatoria.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PubblicaRadio.IsChecked == false && PrivataRadio.IsChecked == false)
            {
                MessageBox.Show("Devi specificare se la categoria è pubblica o privata.",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Recupera l’ID del docente dal JWT
            uint idDocente;
            try
            {
                idDocente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel recupero dell'ID docente dal token JWT:\n{ex.Message}",
                    "Errore JWT", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Popola la categoria creata
            CategoriaCreata.Nome = NomeBox.Text.Trim();
            CategoriaCreata.Tipo = TipoBox.Text.Trim();
            CategoriaCreata.Descrizione = DescrizioneBox.Text.Trim();
            CategoriaCreata.Pubblica = PubblicaRadio.IsChecked == true;
            CategoriaCreata.IDDocente = idDocente;

            DialogResult = true;
        }

        private void Annulla_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
