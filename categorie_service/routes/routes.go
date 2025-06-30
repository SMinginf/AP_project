package routes

import (
	"AP_project/categorie_service/handlers"
	"AP_project/categorie_service/utils"

	"github.com/gin-gonic/gin"
)

func SetupRoutes(r *gin.Engine) {
	sessionRoutes := r.Group("/categorie")
	sessionRoutes.Use(utils.AuthMiddleware())

	sessionRoutes.GET("/list", handlers.GetCategorie)
	sessionRoutes.POST("/categorie", handlers.CreateCategoria)

}
