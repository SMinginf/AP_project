package main

import (
<<<<<<< HEAD
	"AP_project/categorie_service/routes"
=======
	"AP_project/CRUD_service/routes"
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba

	"github.com/gin-gonic/gin"
)

func main() {
	r := gin.Default()
	routes.SetupRoutes(r)
	r.Run(":8082") // Porta diversa da quiz_service
}
