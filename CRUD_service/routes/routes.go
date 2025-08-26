package routes

import (
	"AP_project/CRUD_service/handlers"
	"AP_project/CRUD_service/utils"

	"github.com/gin-gonic/gin"
)

func SetupRoutes(r *gin.Engine) {
	CategoriesRouter := r.Group("/categorie")
	UserRouter := r.Group("/utente")
	QuestionsRoutes := r.Group("/quesiti")

	UserRouter.Use(utils.AuthMiddleware())
	CategoriesRouter.Use(utils.AuthMiddleware())
	QuestionsRoutes.Use(utils.AuthMiddleware())

	CategoriesRouter.GET("/pubbliche", handlers.GetCategoriePubbliche)
	CategoriesRouter.GET("/pubbliche/docente/:id_docente", handlers.GetCategoriePubblicheByDocente)
	CategoriesRouter.GET("/studente/:id_studente", handlers.GetCategorieByStudente)
	CategoriesRouter.POST("/create", handlers.CreateCategoria)
	CategoriesRouter.PUT("/update", handlers.UpdateCategoria)
	CategoriesRouter.DELETE("/delete", handlers.DeleteCategorie)
	CategoriesRouter.GET("/docente/:id_docente", handlers.GetCategorieByDocente)

	UserRouter.GET("/:id_utente", handlers.GetUtente)

	// Route per i quesiti
	QuestionsRoutes.GET("/docente/:id_docente", handlers.GetQuesitiByDocente)
	QuestionsRoutes.POST("/create", handlers.CreateQuesito)
	QuestionsRoutes.PUT("/update", handlers.UpdateQuesito)
	QuestionsRoutes.DELETE("/delete", handlers.DeleteQuesito)

}
