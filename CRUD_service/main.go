package main

import (
	"AP_project/CRUD_service/routes"

	"github.com/gin-gonic/gin"
)

func main() {
	r := gin.Default()
	routes.SetupRoutes(r)
	r.Run(":8082") // Porta diversa da quiz_service
}
