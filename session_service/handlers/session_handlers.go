package handlers

import (
	"AP_project/broker"
	"AP_project/models"
	"AP_project/schemas"
	"encoding/json"
	"fmt"
	"net/http"
	"sync"
	"time"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
	"github.com/nats-io/nats.go"
)

var (
	/*
		Quando prendi la sessione da sessions[id], se è un valore (non puntatore)
		stai lavorando su una copia. Modifiche a sess.Players o sess.IsOpen resterebbero
		confinate alla copia, e la mappa originale non cambia. Con un puntatore invece
		tutte le goroutine vedono e modificano lo stesso oggetto in memoria.*/
	sessions = make(map[string]*models.Session)
	mu       sync.RWMutex
)

// Nel framework Gin, ogni richiesta HTTP in arrivo viene rappresentata da un oggetto *gin.Context chiamato comunemente c.
// Il metodo c.ShouldBindJson(&input):
// 1. Estrae il body JSON della richiesta HTTP.
// 2. Fa il parsing (deserializza) il JSON nei campi della struttura input (nel nostro esempio, una struct tipo JoinSessionInput).
// 3. Controlla i tag binding:"required" e fallisce se mancano campi obbligatori.
// 4. Se tutto va bene, la struct input conterrà i valori inviati dal client.

func CreateSession(c *gin.Context) {

	var input schemas.CreateSessionInput
	if err := c.ShouldBindJSON(&input); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	id := uuid.New().String()
	player := models.Player{ID: input.PlayerID, Username: input.PlayerUsername}

	session := &models.Session{
		ID:         id,
		Name:       input.Name,
		Players:    []models.Player{player},
		MaxPlayers: input.MaxPlayers,
		IsOpen:     true,
		Params:     models.QuizParams{Amount: input.Params.Amount, Category: input.Params.Category, Difficulty: input.Params.Difficulty},
	}

	// salva i dati di una sessione nella mappa sessions.
	// Protegge l’accesso concorrente alla mappa sessions usando un mutex mu.
	// Non so quanto può essere utile salvare le sessioni, in caso si cancella questa parte.
	mu.Lock()
	sessions[id] = session
	mu.Unlock()

	// Restituisce al client un JSON con la sessione appena creata e uno status HTTP 201 Created.
	c.JSON(http.StatusCreated, session)
}

// JoinSession consente a un utente autenticato di unirsi a una sessione esistente
func JoinSession(c *gin.Context) {

	// 2. Estrai il body della richiesta http
	var input schemas.JoinSessionInput
	if err := c.ShouldBindJSON(&input); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	// 3. Proteggi accesso concorrente alla mappa
	mu.Lock()
	defer mu.Unlock()

	session, exists := sessions[input.SessionID]
	if !exists {
		c.JSON(http.StatusNotFound, gin.H{"error": "Sessione non trovata"})
		return
	}

	if !session.IsOpen {
		c.JSON(http.StatusBadRequest, gin.H{"error": "La sessione è chiusa"})
		return
	}

	// 4. Verifica se l’utente è già nella sessione
	for _, p := range session.Players {
		if p.ID == input.PlayerID {
			c.JSON(http.StatusBadRequest, gin.H{"error": "Già nella sessione"})
			return
		}
	}

	// 5. Aggiungi il giocatore alla sessione
	player := models.Player{
		ID:       input.PlayerID,
		Username: input.PlayerUsername,
	}
	session.Players = append(session.Players, player)

	// 6. Salva la sessione aggiornata
	sessions[session.ID] = session

	// 7. Chiude la sessione se ha raggiunto il numero massimo di giocatori
	if len(session.Players) >= session.MaxPlayers {
		session.IsOpen = false
	}
	// 8. Rispondi con la sessione aggiornata
	c.JSON(http.StatusOK, session)
}

// ListSessions restituisce tutte le sessioni attive
func ListSessions(c *gin.Context) {
	mu.RLock()
	defer mu.RUnlock()

	// Dal punto di vista del client, che deve visualizzare i dati delle sessioni (stato, numero di giocatori, ecc.),
	// non fa alcuna differenza ricevere una slice di puntatori o una slice di valori, perché nel JSON i puntatori
	// vengono automaticamente dereferenziati.
	sessionList := make([]*models.Session, 0)

	// Go non serializza automaticamente una map in JSON nel formato corretto. è necessario convertire in una lista (slice)
	for _, s := range sessions {
		sessionList = append(sessionList, s)
	}

	c.JSON(http.StatusOK, sessionList)
}

// Lascia la sessione corrente
func LeaveSession(c *gin.Context) {

	var input schemas.LeaveSessionInput
	if err := c.ShouldBindJSON(&input); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	mu.Lock()
	defer mu.Unlock()

	// Controlla se la sessione esiste e se l'utente che ne ha richiesto
	// la chiusura è lo stesso che l'ha creata
	// sessionID := c.Param("id")
	session, exists := sessions[input.SessionID]
	if !exists {
		c.JSON(http.StatusNotFound, gin.H{"error": "Sessione non trovata"})
		return
	}

	// Verifico che il player corrente partecipi alla sessione
	for i, p := range session.Players {

		// Se sì, lo tolgo
		if p.ID == input.PlayerID {
			session.Players = append(session.Players[:i], session.Players[i+1:]...) // append come parametri successivi al primo vuole elementi singoli, non slice. l'operatore ... trasforma una slice in elementi singoli.

			// Verifico se la sessione è vuota, se sì la elimino
			if len(session.Players) == 0 {
				delete(sessions, session.ID)
			}

			c.JSON(http.StatusOK, gin.H{"message": "Sessione lasciata con successo"})
			return
		}
	}
	c.JSON(http.StatusNotFound, gin.H{"error": "L'utente non appartiene alla sessione"})
}

func StartRequest(c *gin.Context) {
	sessionID := c.Param("id")

	// 1. Verifica che la sessione esista
	mu.RLock()
	sess, ok := sessions[sessionID]
	mu.RUnlock()
	if !ok {
		c.JSON(http.StatusNotFound, gin.H{"error": "session not found"})
		return
	}
	// Verifica che l'utente che ha invocato StartRequest sia l' "host" della sessione (
	// ad esempio consideriamo l'host il player con indice 0 in sessions[sessionID].Players)
	// 2. Innesca la raccolta delle risposte in background
	go awaitStartResponses(sess)

	// 3. Pubblica l’evento di invito
	broker.Publish(
		fmt.Sprintf("session.%s.start.request", sessionID),
		models.StartRequest{SessionID: sessionID},
	)

	c.JSON(http.StatusOK, gin.H{"message": "start request sent"})
}

// awaitStartResponses raccoglie risposte e decide se partire
func awaitStartResponses(sess *models.Session) {
	sub, _ := broker.NC.SubscribeSync(
		fmt.Sprintf("session.%s.start.response", sess.ID),
	)
	defer sub.Unsubscribe()

	responses := make(map[string]bool)
	timeout := time.After(30 * time.Second) // ad esempio 30s per rispondere

	for {
		select {
		case <-timeout:
			// fallito per timeout
			broker.Publish(
				fmt.Sprintf("session.%s.start.cancelled", sess.ID),
				models.StartCancelled{SessionID: sess.ID},
			)
			return

		default:
			// prova a leggere una risposta
			msg, err := sub.NextMsg(1 * time.Second)
			if err == nats.ErrTimeout {
				continue // nessuna risposta in questo istante, ripeti il ciclo
			}
			var resp models.StartResponse
			json.Unmarshal(msg.Data, &resp)

			// se anche uno rifiuta, abortisci subito
			if !resp.Accepted {
				broker.Publish(
					fmt.Sprintf("session.%s.start.cancelled", sess.ID),
					models.StartCancelled{SessionID: sess.ID},
				)
				return
			}

			// altrimenti registra l’accettazione
			responses[resp.PlayerID] = true

			// se ho tutte le risposte positive: confermo e avvio quiz
			if len(responses) == len(sess.Players) {
				broker.Publish(
					fmt.Sprintf("session.%s.start.confirmed", sess.ID),
					models.StartConfirmed{SessionID: sess.ID},
				)
				// qui, a questo punto, potresti:
				// ➜ chiamare quiz_service con gRPC/REST, oppure
				// ➜ pubblicare un ulteriore evento “quiz.start”
				return
			}
		}
	}
}

/* PSEUDO CODICE DEL CLIENT IN C#
// 1) Sottoscrizione all’invito
nc.Subscribe($"session.{sessionId}.start.request", (_, msg) => {
  // deserializza e chiedi conferma all’utente
  bool userAccepted = ShowDialog("Start quiz?");

  // 2) Invia la risposta
  var resp = new { session_id = sessionId, player_id = myId, accepted = userAccepted };
  nc.Publish($"session.{sessionId}.start.response", Serialize(resp));
});

// 3) Gestione conferma o cancellazione
nc.Subscribe($"session.{sessionId}.start.confirmed", (_, _) => {
  // si parte! apri la UI del quiz
});
nc.Subscribe($"session.{sessionId}.start.cancelled", (_, _) => {
  // notifica che non si può partire
});
*/
