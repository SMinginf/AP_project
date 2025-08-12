using Microsoft.Win32;
using QuizClient.Models;
using QuizClient.Services;
using QuizClient.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QuizClient
{
    public partial class QuestionsPage : Page
    {   
        // Gestisce la view dei Quesiti. Viene usata come sorgente dati per la griglia (QuesitiGrid) nella UI.
        // Ogni volta che questa collezione viene modificata (aggiunta, rimozione, aggiornamento),
        // la UI si aggiorna automaticamente grazie al binding WPF. Si usa per permettere la ricerca e
        // il filtraggio dei quesiti.
        public ObservableCollection<Quesito> Quesiti { get; set; } = new ObservableCollection<Quesito>();
        // Contiene tutte i quesiti disponibili, senza filtri. Rispecchia il contenuto del database.
        private List<Quesito> TuttiQuesiti= new List<Quesito>();
        private readonly string _jwtToken;
        private readonly CRUDService _crudService;
        private int count = 0; // Contatore per il numero di quesiti caricati dal file

        //Costruttore della pagina QuestionsPage
        public QuestionsPage(string tk)
        {
            _jwtToken = tk; // Salva il token JWT per eventuali chiamate API
            InitializeComponent();
            _crudService = new CRUDService(_jwtToken); // Inizializza il servizio CRUD con il token JWT

            // Controllo ruolo utente dal JWT.
            // FrameworkElement: Puoi semplicemente usare this, perché la tua classe (QuestionsPage) eredita da Page,
            // che a sua volta eredita da FrameworkElement.
            Utils.JwtUtils.ValidateDocenteRole(_jwtToken, this.NavigationService, this);

            QuesitiGrid.ItemsSource = Quesiti;
            CaricaQuesiti();
        }
        //Metodo per caricare i quesiti dal database rispetto al docente loggato.
        private async void CaricaQuesiti()
        {
            var result = await _crudService.GetQuesitiByDocenteAsync();
            if (result.Success && result.Data != null)
            {
                TuttiQuesiti = result.Data;
            }
            else
            {
                MessageBox.Show(result.ErrorMessage ?? "Errore nel caricamento dei quesiti.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            FiltraQuesiti(string.Empty);
        }
        //Metodo per filtrare i quesiti in base al testo di ricerca e ai filtri selezionati e caricare la view.
        private void FiltraQuesiti(string filtro)
        {
            Quesiti.Clear();
            // Filtra tutti quesiti per Docente loggato 
            foreach (var que in TuttiQuesiti.Where(q =>
                // string.IsNullOrWhiteSpace(filtro) = Se il testo di ricerca (filtro) è vuoto,
                // nullo o solo spazi, questa condizione è vera per tutti i quesiti (nessun filtro per il testo).
                (string.IsNullOrWhiteSpace(filtro) || q.Testo.Contains(filtro, StringComparison.OrdinalIgnoreCase))
            ))
            {
                Quesiti.Add(que);
            }
        }


        // Metodo per gestire il cambiamento del testo nella casella di ricerca.
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Tenta di convertire sender in un TextBox. Se ci riesci (e non è null), allora prendi la sua proprietà Text.
            // Se la conversione fallisce o sender è null, allora l'intera espressione restituisce null
            var filtro = (sender as TextBox)?.Text ?? string.Empty;
            FiltraQuesiti(filtro);
        }


       
        // Metodo per gestire il doppio click su una riga della griglia dei quesiti.
        private async void QuesitiGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Verifica che il doppio click sia su una riga e non sull'header
            var row = ItemsControl.ContainerFromElement(QuesitiGrid, e.OriginalSource as DependencyObject) as DataGridRow;
            if (row == null)
                return;

            if (row.Item is Quesito selezionato)
            {
                var finestra = new QuestionDialogWindow (selezionato, _jwtToken);
                if (finestra.ShowDialog() == true)
                {
                    var result = await _crudService.UpdateQuesitoAsync(finestra.QuesitoCreato);
                    if (result.Success && result.Data != null)
                    {
                        MessageBox.Show("Quesito aggiornato con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                        int index = TuttiQuesiti.IndexOf(selezionato);
                        if (index >= 0)
                            TuttiQuesiti[index] = finestra.QuesitoCreato;
                        FiltraQuesiti((SearchBox?.Text) ?? string.Empty);
                    }
                    else
                    {
                        MessageBox.Show(result.ErrorMessage ?? "Errore nell'aggiornamento del quesito.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        // Metodo per aprire la finestra di dialogo per creare un nuovo quesito.
        // Viene chiamato quando l'utente clicca sul pulsante "Nuovo Quesito".
        private async void NuovoQuesito_Click(object sender, RoutedEventArgs e)
        {
            var finestra = new QuestionDialogWindow(_jwtToken);
            if (finestra.ShowDialog() == true)
            {

                var result = await _crudService.CreateQuesitoAsync(finestra.QuesitoCreato);
                if (result.Success && result.Data != null)
                {
                    MessageBox.Show("Quesito creato con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);

                    TuttiQuesiti.Add(finestra.QuesitoCreato);
                    FiltraQuesiti((SearchBox?.Text) ?? string.Empty);
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage ?? "Errore nella creazione del quesito.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        // Metodo per eliminare i quesiti selezionati.
        private async void EliminaQuesito_Click(object sender, RoutedEventArgs e)
        {
            var selezionati = TuttiQuesiti.Where(c => c.Selezionato).ToList();
            if (selezionati.Count == 0)
            {
                MessageBox.Show("Seleziona almeno un quesito da eliminare.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var conferma = MessageBox.Show(
                $"Vuoi eliminare {selezionati.Count} quesiti selezionati?",
                "Conferma eliminazione",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (conferma == MessageBoxResult.Yes)
            {
                // Prepara la lista degli ID da eliminare
                var ids = selezionati.Select(c => c.ID).ToList();

                System.Diagnostics.Debug.WriteLine("Contenuto di ids:" + string.Join(", ", ids));


                // Chiamata unica al servizio per eliminazione multipla
                var result = await _crudService.DeleteCategoriaAsync(ids);

                if (result.Success)
                {
                    MessageBox.Show("Quesiti eliminati con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Rimuovi localmente le categorie selezionate
                    foreach (var cat in selezionati)
                    {
                        TuttiQuesiti.Remove(cat);
                        FiltraQuesiti((SearchBox?.Text) ?? string.Empty);
                    }
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage ?? "Errore nell'eliminazione dei quesiti.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    // In caso di errore, ricarica le categorie per coerenza
                    CaricaQuesiti();
                }
            }
        }


        // Metodo per caricare un quesito da un file 
        private async void CaricaDaFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // Formato: "Descrizione File|*.estensione1;*.estensione2|Descrizione Altro Tipo|*.estensione3"
            openFileDialog.Filter = "File di Testo (*.txt)|*.txt|File JSON (*.json)|*.json|Tutti i file (*.*)|*.*";
            // Imposta il titolo della finestra di dialogo
            openFileDialog.Title = "Seleziona il file da cui caricare i quesiti";
            // Imposta la directory iniziale 
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            bool? result = openFileDialog.ShowDialog();
            // Elabora il risultato
            if (result == true)
            {
                // Ottieni il percorso completo del file selezionato
                string filePath = openFileDialog.FileName;
                try
                {
                    string fileContent = System.IO.File.ReadAllText(filePath);
                    MessageBox.Show($"File selezionato: {filePath}\nContenuto (primi 100 caratteri):\n{fileContent.Substring(0, Math.Min(fileContent.Length, 100))}");
                    // passo la stringa con il contenuto del file al metodo JSONtoQuesiti
                    List<Quesito> QuesitiFile = JSONLoader.JSONtoQuesiti(fileContent) ?? new List<Quesito>();

                    // Controllo se la lista di quesiti è vuota o nulla
                    if (QuesitiFile == null || QuesitiFile.Count == 0)
                    {
                        MessageBox.Show("Nessun quesito trovato nel file selezionato.", "Informazione", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    else {
                        
                        TuttiQuesiti.Clear(); // Pulisce la lista di tutti i quesiti prima di aggiungere quelli del file
                        count = 0; // Resetta il contatore per i quesiti caricati
                        string CategorieInserite = string.Empty; // Stringa per tenere traccia delle categorie inserite
                        // L'ID docente viene recuperato dal token JWT e assegnato al quesito
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

                        // Inserimento dei quesiti nella lista TuttiQuesiti
                        foreach (var que in QuesitiFile)
                        {
                            count++; // Incrementa il contatore per ogni quesito
                            que.Selezionato = false; // Inizializza la proprietà Selezionato a false
                            que.IDDocente = idDocente; // Assegna l'ID del docente al quesito
                            foreach (var cat in que.Categorie)
                            {
                                cat.IDDocente = idDocente; // Associa l'ID del docente alla categoria
                                var result_c = await _crudService.GetCategorieByDocenteAndNomeAsync(cat.Nome); // Chiamata al servizio per creare il quesito
                                if (result_c.Success)
                                {
                                    if (result_c.Data == null)
                                    {
                                        CategorieInserite = string.Join(", ", cat.Nome);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show(result_c.ErrorMessage ?? $"Errore nel caricamento del quesito numero {count} da file", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            // Chiamata al servizio per creare il quesito
                            var result_q = await _crudService.CreateQuesitoAsync(que); 
                            if (result_q.Success && result_q.Data != null)
                            {
                                TuttiQuesiti.Add(que); // Quesiti creati e aggiunti alla lista TuttiQuesiti
                            }
                            else
                            {
                                MessageBox.Show(result_q.ErrorMessage ?? $"Errore nel caricamento del quesito numero {count} da file", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }

                        // Inserimento avvenuto con successo
                        MessageBox.Show($"Inseriti correttamente {QuesitiFile.Count} su {count} quesiti presenti nel file", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                        MessageBox.Show($"Le seguenti categorie non erano presenti e sono state aggiunte: {CategorieInserite}", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        // Aggiorna la griglia dei quesiti da zero con i nuovi dati
                        CaricaQuesiti();
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore durante la lettura del file: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else // L'utente ha cliccato su Annulla o chiuso la finestra
            {
                MessageBox.Show("Nessun file selezionato.", "Informazione", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }



        //Metodo per tornare alla pagina precdente 
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.GoBack();
            //else
            //    this.IsEnabled = false;
        }
        //Metodo per selezionare tutti i quesiti mostrati nella griglia.
        private void SelezionaTutteLeCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox headerCheckBox)
            {
                bool seleziona = headerCheckBox.IsChecked == true;

                // Imposta la proprietà Selezionato di tutti i quesiti mostrati nella griglia
                foreach (var que in Quesiti)
                {
                    que.Selezionato = seleziona;
                }

                // Aggiorna la DataGrid (notifica il binding)
                QuesitiGrid.Items.Refresh();
            }
        }


    }
}
