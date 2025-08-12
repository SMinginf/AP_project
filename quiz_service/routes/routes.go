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
		sessionRoutes.POST("/create-for-student", handlers.CreateQuizStudente)
		sessionRoutes.POST("/create-for-teacher", handlers.CreateQuizDocente)

		sessionRoutes.POST("/store", handlers.StoreQuiz)
	}
}
