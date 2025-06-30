// main.go
package main

import (
	//"AP_project/broker"
	"AP_project/quiz_service/routes"

	"github.com/gin-gonic/gin"
)

func main() {

	// inizializza NATS (se non ti serve, salta questa riga)
	//broker.Init("nats://localhost:4222")
	r := gin.Default()

	// Inizializza le rotte
	routes.SetupRoutes(r)

	// Avvia il server sulla porta 8081 (puoi cambiarla)
	r.Run(":8081")
}
