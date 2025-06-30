package handlers

import (
	"AP_project/categorie_service/database"
	"AP_project/categorie_service/models"
	"fmt"
	"net/http"

	"github.com/gin-gonic/gin"
)

// Restituisco tutte le categorie presenti nel database, popolando anche il campo Docente
// che contiene le informazioni del docente associato a ciascuna categoria grazie al metodo Preload di GORM.
func GetCategorie(c *gin.Context) {
	var categorie []models.Categoria
	if err := database.DB.Preload("Docente").Find(&categorie).Error; err != nil {
		// Stampa errore anche su console
		fmt.Println("Errore nel recupero delle categorie:", err.Error())

		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nel recupero delle categorie"})
		return
	}
	c.JSON(http.StatusOK, categorie)
}
func CreateCategoria(c *gin.Context) {
	var input models.Categoria
	if err := c.ShouldBindJSON(&input); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}
	if err := database.DB.Create(&input).Error; err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nella creazione della categoria"})
		return
	}
	c.JSON(http.StatusCreated, input)
}
