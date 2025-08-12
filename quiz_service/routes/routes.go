package routes

import (
	"AP_project/quiz_service/handlers"
	"AP_project/quiz_service/utils"

	"github.com/gin-gonic/gin"
)

// SetupRoutes inizializza tutte le rotte per il microservizio delle sessioni
func SetupRoutes(router *gin.Engine) {
	// Gruppo di rotte con prefisso /sessions
	groupRoutes := router.Group("/quiz")
	groupRoutes.Use(utils.AuthMiddleware())
	{
		// POST /sessions/create → Crea una nuova sessione
		groupRoutes.POST("/create", handlers.CreateQuiz)

		groupRoutes.POST("/store", handlers.StoreQuiz)

		groupRoutes.POST("/exams", handlers.GenerateExams)

		// POST /sessions/join → Partecipa a una sessione esistente
		//groupRoutes.POST("/join", handlers.JoinSession)

		// GET /sessions/list → Lista tutte le sessioni
		//groupRoutes.GET("/list", handlers.ListSessions)

		// GET /sessions/:id → Ottieni i dettagli di una sessione specifica
		//groupRoutes.GET("/:id", handlers.GetSessionById)

		// POST /sessions/leave → Lascia la sessione corrente
		// Uso POST e non DELETE perchè non sto cancellando una risorsa server-side.
		// L’utente non sta cancellando la sessione. Sta solo eseguendo un’azione lato server:
		// "voglio lasciare questa sessione" → un evento, non una rimozione di risorsa.
		//groupRoutes.POST("/leave", handlers.LeaveSession)

		//groupRoutes.POST("/:id/start", handlers.StartRequest)
	}
}
