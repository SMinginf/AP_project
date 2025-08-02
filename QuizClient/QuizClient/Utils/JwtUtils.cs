using System;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Navigation;

namespace QuizClient.Utils
{
    public static class JwtUtils
    {
        public static string GetClaimAsString(string jwt, string claimName)
        {
            if (string.IsNullOrEmpty(jwt))
                throw new ArgumentNullException(nameof(jwt), "Il token JWT non può essere nullo o vuoto.");

            var parts = jwt.Split('.');
            if (parts.Length != 3)
                throw new ArgumentException("Il token JWT non è in un formato valido.", nameof(jwt));

            var payload = parts[1];
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var bytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));
            var json = Encoding.UTF8.GetString(bytes);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty(claimName, out var value))
            {
                var str = value.GetString();
                if (str is not null)
                    return str;
            }

            throw new InvalidOperationException($"Claim '{claimName}' non trovata o non convertibile in stringa.");
        }

        public static uint GetClaimAsUInt(string jwt, string claimName)
        {
            if (string.IsNullOrEmpty(jwt))
                throw new ArgumentNullException(nameof(jwt), "Il token JWT non può essere nullo o vuoto.");

            var parts = jwt.Split('.');
            if (parts.Length != 3)
                throw new ArgumentException("Il token JWT non è in un formato valido.", nameof(jwt));

            var payload = parts[1];
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }
            var bytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));
            var json = Encoding.UTF8.GetString(bytes);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(claimName, out var value))
            {
                if (value.ValueKind == JsonValueKind.Number && value.TryGetUInt32(out uint uintValue))
                    return uintValue;
                if (value.ValueKind == JsonValueKind.String && uint.TryParse(value.GetString(), out uint uintFromString))
                    return uintFromString;
            }
            throw new InvalidOperationException($"Claim '{claimName}' non trovata o non convertibile in uint.");
        }

        public static string ValidateDocenteRole(string jwt, NavigationService? navigationService, FrameworkElement frameworkElement)
        {
            // Controllo ruolo utente dal JWT
            bool isDocente = false;
            string ruolo = "";
            try
            {
                ruolo = JwtUtils.GetClaimAsString(jwt, "ruolo");
                isDocente = ruolo == "Docente";
            }
            catch
            {
                isDocente = false;
            }

            if (!isDocente)
            {
                MessageBox.Show("Accesso non autorizzato. Solo i docenti possono visualizzare questa pagina.", "Accesso negato", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Se è una finestra, chiudila e imposta DialogResult = false se possibile
                if (frameworkElement is Window win)
                {
                    try { win.DialogResult = false; } catch { /* Ignora se non modale */ }
                    win.Close();
                }
                // Se è una pagina con NavigationService, torna indietro
                else if (navigationService != null && navigationService.CanGoBack)
                {
                    navigationService.GoBack();
                }
                // Altrimenti disabilita la pagina/finestra
                else
                {
                    frameworkElement.IsEnabled = false;
                }
                
            }
            return ruolo;
        }
    }
}