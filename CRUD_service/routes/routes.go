package routes

import (
<<<<<<< HEAD
	"AP_project/categorie_service/handlers"
	"AP_project/categorie_service/utils"
=======
	"AP_project/CRUD_service/handlers"
	"AP_project/CRUD_service/utils"
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba

	"github.com/gin-gonic/gin"
)

func SetupRoutes(r *gin.Engine) {
<<<<<<< HEAD
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
=======
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
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba

}
