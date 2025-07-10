package routes

import (
	"AP_project/quiz_service/handlers"
	"AP_project/quiz_service/utils"

	"github.com/gin-gonic/gin"
)

// SetupRoutes inizializza tutte le rotte per il microservizio delle sessioni
func SetupRoutes(router *gin.Engine) {
	// Gruppo di rotte con prefisso /sessions
	sessionRoutes := router.Group("/quiz")
	sessionRoutes.Use(utils.AuthMiddleware())
	{
		// POST /sessions/create → Crea una nuova sessione
		sessionRoutes.POST("/create", handlers.CreateQuiz)

		sessionRoutes.POST("/store", handlers.StoreQuiz)

		// POST /sessions/join → Partecipa a una sessione esistente
		//sessionRoutes.POST("/join", handlers.JoinSession)

		// GET /sessions/list → Lista tutte le sessioni
		//sessionRoutes.GET("/list", handlers.ListSessions)

		// GET /sessions/:id → Ottieni i dettagli di una sessione specifica
		//sessionRoutes.GET("/:id", handlers.GetSessionById)

		// POST /sessions/leave → Lascia la sessione corrente
		// Uso POST e non DELETE perchè non sto cancellando una risorsa server-side.
		// L’utente non sta cancellando la sessione. Sta solo eseguendo un’azione lato server:
		// "voglio lasciare questa sessione" → un evento, non una rimozione di risorsa.
		//sessionRoutes.POST("/leave", handlers.LeaveSession)

		//sessionRoutes.POST("/:id/start", handlers.StartRequest)
	}
}
