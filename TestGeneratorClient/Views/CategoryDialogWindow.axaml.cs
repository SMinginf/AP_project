using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using TestGeneratorClient.Models;
using TestGeneratorClient.Utils;

namespace TestGeneratorClient.Views;

public partial class CategoryDialogWindow : Window
{
    public Categoria CategoriaCreata { get; private set; } = new();
    public bool? DialogResult { get; private set; }
    private readonly string _jwtToken;

    public CategoryDialogWindow(string tk)
    {
        _jwtToken = tk;
        InitializeComponent();
        JwtUtils.ValidateDocenteRole(_jwtToken, null, this);
    }

    public CategoryDialogWindow(Categoria esistente, string tk) : this(tk)
    {
        NomeBox.Text = esistente.Nome;
        TipoBox.Text = esistente.Tipo;
        DescrizioneBox.Text = esistente.Descrizione;
        PubblicaRadio.IsChecked = esistente.Pubblica;
        PrivataRadio.IsChecked = !esistente.Pubblica;
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

    private void Salva_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NomeBox.Text) || string.IsNullOrWhiteSpace(DescrizioneBox.Text) ||
            (PubblicaRadio.IsChecked != true && PrivataRadio.IsChecked != true))
        {
            MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow("Errore", "Compila tutti i campi obbligatori")
                .ShowDialog(this);
            return;
        }

        uint idDocente;
        try
        {
            idDocente = JwtUtils.GetClaimAsUInt(_jwtToken, "user_id");
        }
        catch (Exception ex)
        {
            MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow("Errore JWT", $"Errore nel recupero dell'ID docente dal token JWT: {ex.Message}")
                .ShowDialog(this);
            return;
        }

        CategoriaCreata.Nome = NomeBox.Text.Trim();
        CategoriaCreata.Tipo = TipoBox.Text.Trim();
        CategoriaCreata.Descrizione = DescrizioneBox.Text.Trim();
        CategoriaCreata.Pubblica = PubblicaRadio.IsChecked == true;
        CategoriaCreata.IDDocente = idDocente;
        DialogResult = true;
        Close();
    }

    private void Annulla_Click(object? sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
