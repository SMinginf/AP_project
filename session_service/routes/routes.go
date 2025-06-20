package routes

import (
	"AP_project/handlers"
	"AP_project/utils"

	"github.com/gin-gonic/gin"
)

// SetupRoutes inizializza tutte le rotte per il microservizio delle sessioni
func SetupRoutes(router *gin.Engine) {
	// Gruppo di rotte con prefisso /sessions
	sessionRoutes := router.Group("/sessions")
	sessionRoutes.Use(utils.AuthMiddleware())
	{
		// POST /sessions/create → Crea una nuova sessione
		sessionRoutes.POST("/create", handlers.CreateSession)

		// POST /sessions/join → Partecipa a una sessione esistente
		sessionRoutes.POST("/:id/join", handlers.JoinSession)

		// GET /sessions/list → Lista tutte le sessioni attive
		sessionRoutes.GET("/list", handlers.ListSessions)

		// POST /sessions/leave → Lascia la sessione corrente
		// Uso POST e non DELETE perchè non sto cancellando una risorsa server-side.
		// L’utente non sta cancellando la sessione. Sta solo eseguendo un’azione lato server:
		// "voglio lasciare questa sessione" → un evento, non una rimozione di risorsa.
		sessionRoutes.POST("/:id/leave", handlers.LeaveSession)

		sessionRoutes.POST("/:id/start", handlers.StartRequest)
	}
}
