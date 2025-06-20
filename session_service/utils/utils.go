package utils

//bisogna installare la libreria github.com/golang-jwt/jwt/v5
// il comando è: go get github.com/golang-jwt/jwt/v5

import (
	"net/http"
	"strings"

	"github.com/gin-gonic/gin"
	"github.com/golang-jwt/jwt/v5"
)

// 🔐 Chiave segreta usata per verificare la firma del token JWT.
// ⚠️ In un'applicazione reale, questa chiave dovrebbe essere caricata da una variabile d'ambiente o un file di configurazione.
var jwtSecret = []byte("supersegreta123")

// AuthMiddleware è una funzione middleware che verifica la validità del token JWT
// presente nell'header Authorization della richiesta.
/*
1) Parsing: JWT viene diviso in 3 parti (header, payload/claims, signature).

2) Decodifica: Viene decodificato il contenuto base64.

3) Verifica della firma:

	- Chiama la tua keyFunc(token) per ottenere la chiave segreta.

	- Controlla che la firma del token sia corretta con l’algoritmo specificato nell’header (es. HS256).

4) Parsing dei claims (contenuto del payload): restituisce i dati solo se la firma è corretta.

5) Verifica del contenuto:
	- Se il token è scaduto (exp claim), Valid() fallirà.
*/
func AuthMiddleware() gin.HandlerFunc {
	return func(c *gin.Context) {
		// Estrae l'header "Authorization" dalla richiesta
		authHeader := c.GetHeader("Authorization")

		// Se l'header è vuoto o non inizia con "Bearer ", blocca la richiesta
		if authHeader == "" || !strings.HasPrefix(authHeader, "Bearer ") {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "Token mancante o malformato"})
			return
		}

		// Rimuove la parte "Bearer " per ottenere solo il token
		tokenString := strings.TrimPrefix(authHeader, "Bearer ")

		/* Esegue il parsing e la validazione del token.
		Parametri:
		- tokenString: è la stringa del JWT che il client ha inviato (di solito in un header HTTP tipo Authorization: Bearer <token>).
		- func(token *jwt.Token) (interface{}, error) {...}: è una funzione anonima che, dato il *Token, restituisce la chiave segreta usata per verificare la firma.
		- options: (facoltativo) sono opzioni avanzate per configurare il parser (ad esempio accettare token senza firma, clock skew, ecc).*/
		token, err := jwt.Parse(tokenString, func(token *jwt.Token) (interface{}, error) {

			// Qui viene verificato se token.Method è effettivamente un'istanza di *jwt.SigningMethodHMAC.
			// Se il controllo fallisce, significa che il token usa un altro metodo di firma e la verifica deve essere interrotta.
			if _, ok := token.Method.(*jwt.SigningMethodHMAC); !ok {
				return nil, jwt.ErrSignatureInvalid
			}

			// Ritorna la chiave segreta usata per verificare la firma
			return jwtSecret, nil
		})

		// Se c'è un errore o il token non è valido, blocca la richiesta
		if err != nil || !token.Valid {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "Token non valido"})
			return
		}

		// Se il token è valido, estraiamo le claims (cioè i dati contenuti nel token)
		claims, ok := token.Claims.(jwt.MapClaims)
		if ok {
			// Salviamo l'ID utente (che di solito è nel campo "sub" del token)
			// nel contesto `c`, così sarà accessibile dagli handler successivi
			c.Set("user_id", claims["sub"])
		}

		// Passa la richiesta all'handler successivo
		c.Next()
	}
}
