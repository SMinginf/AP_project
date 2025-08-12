using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TestGeneratorClient.Models;
using TestGeneratorClient.Services;
using TestGeneratorClient.Utils;

namespace TestGeneratorClient.Views;

public partial class CategoriesPage : UserControl
{
    private readonly string _jwtToken;
    private readonly MainWindow _mainWindow;
    private readonly CRUDService _crudService;
    private List<Categoria> _tutteLeCategorie = new();
    public ObservableCollection<Categoria> Categorie { get; } = new();

    public CategoriesPage(string jwtToken, MainWindow mainWindow)
    {
        InitializeComponent();
        _jwtToken = jwtToken;
        _mainWindow = mainWindow;
        _crudService = new CRUDService(_jwtToken);
        JwtUtils.ValidateDocenteRole(_jwtToken, null, this);
        CategorieGrid.ItemsSource = Categorie;
        CaricaCategorie();
    }

    private async void CaricaCategorie()
    {
        var result = await _crudService.GetCategorieByDocenteAsync();
        if (result.Success && result.Data != null)
        {
            _tutteLeCategorie = result.Data;
        }
        else
        {
            await MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow("Errore", result.ErrorMessage ?? "Errore nel caricamento delle categorie.")
                .ShowDialog(_mainWindow);
            return;
        }
        FiltraCategorie(string.Empty);
    }

    private void FiltraCategorie(string filtro)
    {
        bool soloPubbliche = FiltroPubbliche.IsChecked == true;
        bool soloPrivate = FiltroPrivate.IsChecked == true;
        Categorie.Clear();
        foreach (var cat in _tutteLeCategorie.Where(c =>
                 (string.IsNullOrWhiteSpace(filtro) || c.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase)) &&
                 (
                    (!soloPubbliche && !soloPrivate) ||
                    (soloPubbliche && c.Pubblica) ||
                    (soloPrivate && !c.Pubblica)
                 )))
        {
            Categorie.Add(cat);
        }
    }

    private void SearchBox_TextChanged(object? sender, RoutedEventArgs e)
    {
        FiltraCategorie(SearchBox.Text ?? string.Empty);
    }

    private void FiltroPubblica_Checked(object? sender, RoutedEventArgs e)
    {
        if (FiltroPubbliche.IsChecked == true)
            FiltroPrivate.IsChecked = false;
        FiltraCategorie(SearchBox.Text ?? string.Empty);
    }

    private void FiltroPrivata_Checked(object? sender, RoutedEventArgs e)
    {
        if (FiltroPrivate.IsChecked == true)
            FiltroPubbliche.IsChecked = false;
        FiltraCategorie(SearchBox.Text ?? string.Empty);
    }

    private void Filtri_Unchecked(object? sender, RoutedEventArgs e)
    {
        FiltraCategorie(SearchBox.Text ?? string.Empty);
    }

    private async void NuovaCategoria_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new CategoryDialogWindow(_jwtToken);
        await dialog.ShowDialog(_mainWindow);
        if (dialog.DialogResult == true)
        {
            var result = await _crudService.CreateCategoriaAsync(dialog.CategoriaCreata);
            if (result.Success && result.Data != null)
            {
                await MessageBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandardWindow("Successo", "Categoria creata con successo!")
                    .ShowDialog(_mainWindow);
                _tutteLeCategorie.Add(dialog.CategoriaCreata);
                FiltraCategorie(SearchBox.Text ?? string.Empty);
            }
            else
            {
                await MessageBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandardWindow("Errore", result.ErrorMessage ?? "Errore nella creazione della categoria.")
                    .ShowDialog(_mainWindow);
            }
        }
    }

    private async void CategorieGrid_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (CategorieGrid.SelectedItem is not Categoria selezionata)
            return;
        var dialog = new CategoryDialogWindow(selezionata, _jwtToken);
        await dialog.ShowDialog(_mainWindow);
        if (dialog.DialogResult == true)
        {
            var result = await _crudService.UpdateCategoriaAsync(dialog.CategoriaCreata);
            if (result.Success && result.Data != null)
            {
                await MessageBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandardWindow("Successo", "Categoria aggiornata con successo!")
                    .ShowDialog(_mainWindow);
                var idx = _tutteLeCategorie.IndexOf(selezionata);
                if (idx >= 0)
                    _tutteLeCategorie[idx] = dialog.CategoriaCreata;
                FiltraCategorie(SearchBox.Text ?? string.Empty);
            }
            else
            {
                await MessageBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandardWindow("Errore", result.ErrorMessage ?? "Errore nell'aggiornamento della categoria.")
                    .ShowDialog(_mainWindow);
            }
        }
    }

    private async void EliminaCategorie_Click(object? sender, RoutedEventArgs e)
    {
        var selezionate = _tutteLeCategorie.Where(c => c.Selezionata).ToList();
        if (selezionate.Count == 0)
        {
            await MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow("Attenzione", "Seleziona almeno una categoria da eliminare.")
                .ShowDialog(_mainWindow);
            return;
        }

        var conferma = await MessageBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandardWindow("Conferma eliminazione", $"Vuoi eliminare {selezionate.Count} categorie selezionate?", MessageBox.Avalonia.Enums.ButtonEnum.YesNo)
            .ShowDialog(_mainWindow);
        if (conferma == MessageBox.Avalonia.Enums.ButtonResult.Yes)
        {
            var ids = selezionate.Select(c => c.ID).ToList();
            var result = await _crudService.DeleteCategoriaAsync(ids);
            if (result.Success)
            {
                await MessageBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandardWindow("Successo", "Categorie eliminate con successo!")
                    .ShowDialog(_mainWindow);
                foreach (var cat in selezionate)
                {
                    _tutteLeCategorie.Remove(cat);
                }
                FiltraCategorie(SearchBox.Text ?? string.Empty);
            }
            else
            {
                await MessageBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandardWindow("Errore", result.ErrorMessage ?? "Errore nell'eliminazione delle categorie.")
                    .ShowDialog(_mainWindow);
                CaricaCategorie();
            }
        }
    }

    private void SelezionaTutteLeCheckbox_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is CheckBox cb)
        {
            bool seleziona = cb.IsChecked == true;
            foreach (var cat in Categorie)
                cat.Selezionata = seleziona;
            CategorieGrid.ItemsSource = null;
            CategorieGrid.ItemsSource = Categorie;
        }
    }

    private void BackToLobby_Click(object? sender, RoutedEventArgs e)
    {
        _mainWindow.Navigate(new LobbyPage(_mainWindow, _jwtToken));
    }
}
