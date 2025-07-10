package handlers

import (
	"AP_project/categorie_service/database"
	"AP_project/categorie_service/models"
	"fmt"
	"net/http"

	"github.com/gin-gonic/gin"
)

// --- FUNZIONI CRUD PER LE CATEGORIE ---

// Restituisco tutte le categorie pubbliche presenti nel database, popolando anche il campo Docente
// che contiene le informazioni del docente associato a ciascuna categoria grazie al metodo Preload di GORM.
func GetCategoriePubbliche(c *gin.Context) {
	var categorie []models.Categoria
	if err := database.DB.Preload("Docente").Where("pubblica = ?", true).Find(&categorie).Error; err != nil {
		// Stampa errore anche su console
		fmt.Println("Errore nel recupero delle categorie:", err.Error())

		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nel recupero delle categorie pubbliche"})
		return
	}
	c.JSON(http.StatusOK, categorie)
}
func CreateCategoria(c *gin.Context) {
	ruolo, exists := c.Get("ruolo")
	if !exists || ruolo != "Docente" {
		c.JSON(http.StatusForbidden, gin.H{"error": "Solo i docenti possono creare categorie"})
		return
	}
	var categoria models.Categoria
	if err := c.ShouldBindJSON(&categoria); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}
	if err := database.DB.Create(&categoria).Error; err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nella creazione della categoria"})
		return
	}
	c.JSON(http.StatusCreated, categoria)
}

func UpdateCategoria(c *gin.Context) {
	ruolo, exists := c.Get("ruolo")
	if !exists || ruolo != "Docente" {
		c.JSON(http.StatusForbidden, gin.H{"error": "Solo i docenti possono modificare categorie"})
		return
	}
	var categoria models.Categoria
	if err := c.ShouldBindJSON(&categoria); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	if err := database.DB.Model(&models.Categoria{}).Where("id = ?", categoria.ID).Updates(categoria).Error; err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nella modifica della categoria"})
		return
	}

	c.JSON(http.StatusOK, gin.H{"message": "Categoria modificata con successo"})
}

func DeleteCategorie(c *gin.Context) {
	ruolo, exists := c.Get("ruolo")
	if !exists || ruolo != "Docente" {
		c.JSON(http.StatusForbidden, gin.H{"error": "Solo i docenti possono cancellare categorie"})
		return
	}

	var ids []uint
	if err := c.ShouldBindJSON(&ids); err != nil || len(ids) == 0 {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Devi specificare almeno un ID di categoria da cancellare"})
		return
	}

	if err := database.DB.Delete(&models.Categoria{}, ids).Error; err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nella cancellazione delle categorie"})
		return
	}

	c.JSON(http.StatusOK, gin.H{"message": "Categorie cancellate con successo"})
}

func GetCategorieByDocente(c *gin.Context) {
	idDocente := c.Param("id_docente")
	if idDocente == "" {
		c.JSON(http.StatusBadRequest, gin.H{"error": "ID del docente mancante"})
		return
	}
	var categorie []models.Categoria
	if err := database.DB.Where("id_docente = ?", idDocente).Preload("Docente").Find(&categorie).Error; err != nil {
		fmt.Println("Errore nel recupero delle categorie per il docente:", err.Error())
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nel recupero delle categorie per il docente"})
		return
	}
	c.JSON(http.StatusOK, categorie)
}

// --- FUNZIONI CRUD PER I QUESITI ---

func CreateQuesito(c *gin.Context) {
	ruolo, exists := c.Get("ruolo")
	if !exists || ruolo != "Docente" {
		c.JSON(http.StatusForbidden, gin.H{"error": "Solo i docenti possono creare quesiti"})
		return
	}

	var quesito models.Quesito
	if err := c.ShouldBindJSON(&quesito); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	if err := database.DB.Create(&quesito).Error; err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nella creazione del quesito"})
		return
	}

	c.JSON(http.StatusCreated, quesito)
}

func UpdateQuesito(c *gin.Context) {
	ruolo, exists := c.Get("ruolo")
	if !exists || ruolo != "Docente" {
		c.JSON(http.StatusForbidden, gin.H{"error": "Solo i docenti possono modificare i quesiti"})
		return
	}
	var quesito models.Quesito
	if err := c.ShouldBindJSON(&quesito); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	if err := database.DB.Model(&models.Quesito{}).Where("id = ?", quesito.ID).Updates(quesito).Error; err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nella modifica del quesito"})
		return
	}

	c.JSON(http.StatusOK, gin.H{"message": "Quesito modificato con successo"})
}

func DeleteQuesito(c *gin.Context) {
	ruolo, exists := c.Get("ruolo")
	if !exists || ruolo != "Docente" {
		c.JSON(http.StatusForbidden, gin.H{"error": "Solo i docenti possono cancellare i quesiti"})
		return
	}

	id := c.Param("id")
	if id == "" {
		c.JSON(http.StatusBadRequest, gin.H{"error": "ID del quesito mancante"})
		return
	}
	if err := database.DB.Delete(&models.Quesito{}, id).Error; err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nella cancellazione del quesito"})
		return
	}
	c.JSON(http.StatusOK, gin.H{"message": "Quesito cancellato con successo"})
}
func GetQuesitiByDocente(c *gin.Context) {
	idDocente := c.Param("id_docente")
	if idDocente == "" {
		c.JSON(http.StatusBadRequest, gin.H{"error": "ID del docente mancante"})
		return
	}
	var quesiti []models.Quesito
	if err := database.DB.Where("id_docente = ?", idDocente).Preload("Docente").Find(&quesiti).Error; err != nil {
		fmt.Println("Errore nel recupero dei quesiti per il docente:", err.Error())
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nel recupero dei quesiti per il docente"})
		return
	}
	c.JSON(http.StatusOK, quesiti)
}

func GetQuesitiByDocenteAndCategoria(c *gin.Context) {
	idDocente := c.Param("id_docente") // usato per i parametri dinamici definiti nella route
	categoria := c.Query("categoria")  // usato per i parametri di query (query string) come ?categoria=nome_categoria
	if idDocente == "" || categoria == "" {
		c.JSON(http.StatusBadRequest, gin.H{"error": "ID del docente o categoria mancante"})
		return
	}

	var categorie []models.Categoria
	if err := database.DB.Where("id_docente = ? AND nome = ?", idDocente, categoria).Preload("Docente").Find(&categorie).Error; err != nil {
		fmt.Println("Errore nel recupero delle categorie per il docente e la categoria:", err.Error())
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nel recupero delle categorie per il docente e la categoria"})
		return
	}
	c.JSON(http.StatusOK, categorie)
}
