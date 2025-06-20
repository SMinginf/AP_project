package broker

import (
	"encoding/json"
	"log"

	"github.com/nats-io/nats.go"
)

var NC *nats.Conn

// Init the NATS connection
func Init(url string) {
	var err error
	NC, err = nats.Connect(url)
	if err != nil {
		log.Fatalf("Connessione a NATS fallita: %v", err)
	}
}

// Publish serializza v in JSON e lo invia su subject
func Publish(subject string, v interface{}) {
	b, err := json.Marshal(v) // Converte l'oggetto in JSON
	if err != nil {
		log.Printf("broker: marshal error: %v", err) // Log errore di serializzazione
		return
	}
	if err := NC.Publish(subject, b); err != nil {
		log.Printf("broker: publish error: %v", err) // Log errore di pubblicazione
	}
}
