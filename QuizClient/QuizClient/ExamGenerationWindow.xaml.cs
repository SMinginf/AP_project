/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using QuizClient.Models;
using QuizClient.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuizClient
{
    public partial class ExamGenerationWindow : Window { 
           
        public int Copie {get; set;}
        
            public ExamGenerationWindow()
        {
            InitializeComponent();
        }

        // Gestisce il click sul pulsante "Ok" inserendo il numero di copie specificato
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(CopieBox.Text, out int copie) && copie > 0)
            {
                Copie = copie;
                DialogResult = true; // Imposta il risultato del dialogo su true
                this.Close(); // Chiude la finestra
            }
            else
            {
                MessageBox.Show("Per favore, inserisci un numero valido di copie (intero positivo).", "Input non valido", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Imposta il risultato del dialogo su false
            this.Close(); // Chiude la finestra
        }
    }
}
*/

using System;
using System.Windows;

namespace QuizClient
{
    public partial class ExamGenerationWindow : Window
    {
        // Numero di copie inserito dall'utente
        public int Copie { get; private set; }

        public ExamGenerationWindow()
        {
            InitializeComponent();
        }

        // --------------------------- //
        //     EVENTI BUTTON CLICK     //
        // --------------------------- //

        // Gestisce il click sul pulsante "Ok"
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            var input = CopieBox.Text.Trim();

            // Convalida che l'input sia un intero positivo
            if (int.TryParse(input, out int copie) && copie > 0)
            {
                Copie = copie;
                DialogResult = true;  // Imposta il risultato del dialogo su true
                Close();              // Chiude la finestra
            }
            else
            {
                MessageBox.Show(
                    "Inserisci un numero valido di copie (intero positivo).",
                    "Input non valido",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        // Gestisce il click su "Annulla"
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
