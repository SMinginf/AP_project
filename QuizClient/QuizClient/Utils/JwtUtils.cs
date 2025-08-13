using Microsoft.VisualBasic;
using QuizClient.Models;
using System;
using System.Buffers.Text;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Navigation;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QuizClient.Utils
{
    public static class JwtUtils
    {
        // Metodo che prende una JWT (JSON Web Token) e un nome di claim da ricercare all'interno del
        // token e resituisce il valore di quel claim come string.
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

        // Metodo che prende una JWT (JSON Web Token) e un nome di claim da ricercare all'interno del
        // token e resituisce il valore di quel claim come uint.
        public static uint GetClaimAsUInt(string jwt, string claimName)
        {
            // Controlla se il token JWT è nullo o vuoto
            if (string.IsNullOrEmpty(jwt))
                throw new ArgumentNullException(nameof(jwt), "Il token JWT non può essere nullo o vuoto.");
            
            //Un JWT è composto da 3 parti separate da:
            //Header(metadata, algoritmo di firma, ecc;
            //Payload(dati — cioè le claims);
            //Signature(firma crittografica).
            //Qui viene preso solo il payload: parts[1]

            var parts = jwt.Split('.');
            if (parts.Length != 3)
                throw new ArgumentException("Il token JWT non è in un formato valido.", nameof(jwt));
            var payload = parts[1];

            //I JWT usano Base64 URL-safe (senza padding =, e con - e _ al posto di + e /).
            //Qui si aggiunge il padding = mancante per poter decodificare correttamente il Base64.
            //Base64 richiede che la stringa sia lunga un multiplo di 4. Ma il JWT spesso omette il padding.
            //% 4 calcola il resto della lunghezza rispetto a 4.
            //Se il resto è 2 → aggiungi "==" per completare.
            //Se è 3 → aggiungi "=".
            //Senza padding, Convert.FromBase64String() fallirebbe.
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }
            //Viene convertito in un array di byte dopo averlo reso conforme al formato standard Base64.
            var bytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));
            
            //Si ottiene una stringa JSON dal payload (es. {"sub":"123", "exp": 1715622345})
            var json = Encoding.UTF8.GetString(bytes);

            //JsonDocument.Parse carica il JSON in modo da poter interrogare le sue proprietà (le claims).
            //la stringa json viene convertita in un JsonDocument (oggetto), che permette di accedere ai dati JSON in modo strutturato.
            //Usa using per garantire che venga rilasciata la memoria quando non serve più
            //Funziona con oggetti che implementano l’interfaccia IDisposable
            using var doc = JsonDocument.Parse(json);

            //Cerca la proprietà claimName (es: "exp", "userId", ecc.)
            //"out" indica che il metodo restituirà un valore attraverso il parametro.
            //È utile per ottenere più risultati da una funzione(oltre al valore di ritorno booleano true / false).
            if (doc.RootElement.TryGetProperty(claimName, out var value))
            {
                //Se il valore è già numerico (JsonValueKind.Number), prova a leggerlo come uint (TryGetUInt32)
                //e lo ritorna direttamente convertito alla fine.
                //(Ricorda che con "out" parametro di ingresso == uno dei valori di ritorno)
                if (value.ValueKind == JsonValueKind.Number && value.TryGetUInt32(out uint uintValue))
                    return uintValue;
                //Se il valore è una stringa, prova a convertirlo in uint.
                if (value.ValueKind == JsonValueKind.String && uint.TryParse(value.GetString(), out uint uintFromString))
                    return uintFromString;
            }
            throw new InvalidOperationException($"Claim '{claimName}' non trovata o non convertibile in uint.");
        }

        // Metodo per validare il ruolo di un docente basato sul JWT.
        public static void ValidateDocenteRole(string jwt, NavigationService? navigationService, FrameworkElement frameworkElement)
        {
            // Controllo ruolo utente dal JWT
            bool isDocente = false;
            try
            {
                var ruolo = JwtUtils.GetClaimAsString(jwt, "ruolo");
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
                return;
            }
        }
    }
}