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

	}
}
