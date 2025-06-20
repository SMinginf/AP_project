// models/event.go
package models

// StartRequest rappresenta l’evento di invito all’inizio
type StartRequest struct {
	SessionID string `json:"session_id"`
}

// StartResponse evento di risposta di un giocatore
type StartResponse struct {
	SessionID string `json:"session_id"`
	PlayerID  string `json:"player_id"`
	Accepted  bool   `json:"accepted"`
}

// StartConfirmed notifiche di conferma
type StartConfirmed struct {
	SessionID string `json:"session_id"`
}

// StartCancelled notifica di cancellazione
type StartCancelled struct {
	SessionID string `json:"session_id"`
}
