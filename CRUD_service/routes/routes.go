package routes

import (
	"AP_project/categorie_service/handlers"
	"AP_project/categorie_service/utils"

	"github.com/gin-gonic/gin"
)

func SetupRoutes(r *gin.Engine) {
	// Route per le categorie
	sessionRoutes := r.Group("/categorie")
	sessionRoutes.Use(utils.AuthMiddleware())

	sessionRoutes.GET("/pubbliche", handlers.GetCategoriePubbliche)
	sessionRoutes.POST("/create", handlers.CreateCategoria)
	sessionRoutes.PUT("/update", handlers.UpdateCategoria)
	sessionRoutes.DELETE("/delete", handlers.DeleteCategorie)
	sessionRoutes.GET("/docente/:id_docente", handlers.GetCategorieByDocente)

	// Route per i quesiti
	questionsRoutes := r.Group("/quesiti")
	questionsRoutes.Use(utils.AuthMiddleware())

	questionsRoutes.GET("/docente/:id_docente", handlers.GetQuesitiByDocente)
	questionsRoutes.POST("/create", handlers.CreateQuesito)
	questionsRoutes.PUT("/update", handlers.UpdateQuesito)
	questionsRoutes.DELETE("/delete", handlers.DeleteQuesito)

}
