package routes

import (
	"AP_project/categorie_service/handlers"
	"AP_project/categorie_service/utils"

	"github.com/gin-gonic/gin"
)

func SetupRoutes(r *gin.Engine) {
	sessionRoutes := r.Group("/categorie")
	sessionRoutes.Use(utils.AuthMiddleware())

	sessionRoutes.GET("/pubbliche", handlers.GetCategoriePubbliche)
	sessionRoutes.POST("/create", handlers.CreateCategoria)
	sessionRoutes.PUT("/update", handlers.UpdateCategoria)
	sessionRoutes.DELETE("/delete", handlers.DeleteCategorie)
	sessionRoutes.GET("/docente/:id_docente", handlers.GetCategorieByDocente)

}
