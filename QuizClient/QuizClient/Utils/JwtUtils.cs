using System;
using System.Text;
using System.Text.Json;

namespace QuizClient.Utils
{
    public static class JwtUtils
    {
        public static string? GetClaim(string jwt, string claimName)
        {
            // Verifica se il JWT è nullo o vuoto; in tal caso restituisce null
            if (string.IsNullOrEmpty(jwt)) return null;

            // Divide il JWT in tre parti (header, payload, signature) usando il punto come separatore
            var parts = jwt.Split('.');
            if (parts.Length != 3) return null; // Se il JWT non ha esattamente 3 parti, restituisce null

            // Estrae la parte del payload (seconda parte del JWT)
            var payload = parts[1];

            // Aggiunge il padding necessario per la decodifica base64, in base alla lunghezza del payload
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            // Decodifica il payload da base64 a byte array, sostituendo i caratteri URL-safe con quelli standard
            var bytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));

            // Converte il byte array in una stringa JSON UTF-8
            var json = Encoding.UTF8.GetString(bytes);

            // Analizza la stringa JSON per ottenere un oggetto JsonDocument
            using var doc = JsonDocument.Parse(json);

            // Tenta di estrarre il valore della claim specificata dal payload
            if (doc.RootElement.TryGetProperty(claimName, out var value))
                return value.GetString(); // Se la claim esiste, restituisce il suo valore come stringa

            return null; // Se la claim non esiste, restituisce null
        }

        public static int? GetClaimAsInt(string jwt, string claimName)
        {
            if (string.IsNullOrEmpty(jwt)) return null;
            var parts = jwt.Split('.');
            if (parts.Length != 3) return null;

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
                if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int intValue))
                    return intValue;
                // Se la claim è una stringa numerica, prova a convertirla
                if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out int intFromString))
                    return intFromString;
            }
            return null;
        }
    }
}