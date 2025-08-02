package routes

import (
	"AP_project/CRUD_service/handlers"
	"AP_project/CRUD_service/utils"

	"github.com/gin-gonic/gin"
)

func SetupRoutes(r *gin.Engine) {
	CategoriesRouter := r.Group("/categorie")
	UserRouter := r.Group("/utente")

	UserRouter.Use(utils.AuthMiddleware())
	CategoriesRouter.Use(utils.AuthMiddleware())

	CategoriesRouter.GET("/pubbliche", handlers.GetCategoriePubbliche)
	CategoriesRouter.GET("/pubbliche/docente/:id_docente", handlers.GetCategoriePubblicheByDocente)
	CategoriesRouter.GET("/studente/:id_studente", handlers.GetCategorieByStudente)
	CategoriesRouter.POST("/create", handlers.CreateCategoria)
	CategoriesRouter.PUT("/update", handlers.UpdateCategoria)
	CategoriesRouter.DELETE("/delete", handlers.DeleteCategorie)
	CategoriesRouter.GET("/docente/:id_docente", handlers.GetCategorieByDocente)

	UserRouter.GET("/:id_utente", handlers.GetUtente)

}
