using System;
using System.Text;
using System.Text.Json;
using Avalonia.Controls;

namespace TestGeneratorClient.Utils
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

        public static async void ValidateDocenteRole(string jwt, object? navigationService, Control control)
        {
            bool isDocente = false;
            try{ var ruolo = GetClaimAsString(jwt, "ruolo"); isDocente = ruolo == "Docente"; } catch{ isDocente = false; }
            if(!isDocente){ await MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Accesso negato","Accesso non autorizzato. Solo i docenti possono visualizzare questa pagina.").ShowDialog((Window)control.VisualRoot!); if(control is Window win){ win.Close(); } else { control.IsEnabled=false; } }
        }
    }
}