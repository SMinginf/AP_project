using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using TestGeneratorClient.Services;
using TestGeneratorClient.Utils;

namespace TestGeneratorClient.Views;

public partial class RegisterPage : UserControl
{
    private readonly MainWindow _mainWindow;
    private readonly AuthService _authService = new();

    public RegisterPage(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
    }

    private async void RegisterUser_ClickAsync(object? sender, RoutedEventArgs e)
    {
        string ruolo = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
        string username = NicknameBox.Text ?? string.Empty;
        string email = EmailBox.Text ?? string.Empty;
        string nome = NameBox.Text ?? string.Empty;
        string cognome = SurnameBox.Text ?? string.Empty;
        string genere = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
        string password = PasswordBox.Text ?? string.Empty;
        string confirmPassword = ConfirmPasswordBox.Text ?? string.Empty;
        string data_nascita = BirthDatePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? string.Empty;

        if (password != confirmPassword)
        {
            await MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow("Errore", "Le password non coincidono.")
                .ShowDialog(_mainWindow);
            return;
        }

        try
        {
            var (success, message) = await _authService.RegisterAsync(ruolo, username, email, nome, cognome, genere, password, data_nascita);
            if (success)
            {
                await MessageBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandardWindow("Successo", message)
                    .ShowDialog(_mainWindow);
                _mainWindow.Navigate(new LoginPage(_mainWindow));
            }
            else
            {
                await MessageBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandardWindow("Errore", message)
                    .ShowDialog(_mainWindow);
            }
        }
        catch (Exception ex)
        {
            await MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow("Exception", $"Errore durante la registrazione: {ex.Message}")
                .ShowDialog(_mainWindow);
        }
    }

    private void BackToLogin_Click(object? sender, RoutedEventArgs e)
    {
        _mainWindow.Navigate(new LoginPage(_mainWindow));
    }
}
