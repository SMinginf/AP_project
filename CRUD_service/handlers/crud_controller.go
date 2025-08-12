package handlers

import (
	"AP_project/CRUD_service/logic" // [AGGIUNTO] delega alla logica applicativa
	"AP_project/CRUD_service/models"
	"errors" // [AGGIUNTO] per errors.Is
	"fmt"
	"net/http"

	"github.com/gin-gonic/gin"
)

// --- FUNZIONI CRUD PER LE CATEGORIE ---

// Restituisco tutte le categorie pubbliche presenti nel database, popolando anche il campo Docente
// che contiene le informazioni del docente associato a ciascuna categoria grazie al metodo Preload di GORM.
func GetCategoriePubbliche(c *gin.Context) {
	categorie, err := logic.GetCategoriePubbliche() // [AGGIUNTO]
	if err != nil {
		// Stampa errore anche su console
		fmt.Println("Errore nel recupero delle categorie:", err.Error())

		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nel recupero delle categorie pubbliche"})
		return
	}
	c.JSON(http.StatusOK, categorie)
}

func GetCategoriePubblicheByDocente(c *gin.Context) {
	idDocente := c.Param("id_docente")
	if idDocente == "" {
		c.JSON(http.StatusBadRequest, gin.H{"error": "ID del docente mancante"})
		return
	}
	categorie, err := logic.GetCategoriePubblicheByDocente(idDocente) // [AGGIUNTO]
	if err != nil {
		c.JSON(mapErrorToStatus(err), gin.H{"error": "Errore nel recupero delle categorie pubbliche per il docente"})
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
	created, err := logic.CreateCategoria(categoria) // [AGGIUNTO]
	if err != nil {
		c.JSON(mapErrorToStatus(err), gin.H{"error": err.Error()})
		return
	}
	c.JSON(http.StatusCreated, created)
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

	// 0 è il valore di default per un uint. L'autoincrement parte da 1, ciò vuol dire che
	// se l'ID è 0 significa che categoria.ID non è stato impostato.
	if categoria.ID == 0 {
		c.JSON(http.StatusBadRequest, gin.H{"error": "ID categoria mancante"})
		return
	}

	if err := logic.UpdateCategoria(categoria); err != nil { // [AGGIUNTO]
		c.JSON(mapErrorToStatus(err), gin.H{"error": "Errore nella modifica della categoria"})
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

	if err := logic.DeleteCategorie(ids); err != nil { // [AGGIUNTO]
		c.JSON(mapErrorToStatus(err), gin.H{"error": "Errore nella cancellazione delle categorie"})
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
	categorie, err := logic.GetCategorieByDocente(idDocente) // [AGGIUNTO]
	if err != nil {
		c.JSON(mapErrorToStatus(err), gin.H{"error": "Errore nel recupero delle categorie per il docente"})
		return
	}
	c.JSON(http.StatusOK, categorie)
}

func GetCategorieByStudente(c *gin.Context) {
	idStudente := c.Param("id_studente")
	if idStudente == "" {
		c.JSON(http.StatusBadRequest, gin.H{"error": "ID dello studente mancante"})
		return
	}

	categorie, err := logic.GetCategorieByStudente(idStudente) // [AGGIUNTO]
	if err != nil {
		c.JSON(mapErrorToStatus(err), gin.H{"error": "Errore nel recupero delle categorie per lo studente"})
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

	created, err := logic.CreateQuesito(quesito) // [AGGIUNTO]
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nella creazione del quesito"})
		return
	}

	c.JSON(http.StatusCreated, created)
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

	if quesito.ID == 0 {
		c.JSON(http.StatusBadRequest, gin.H{"error": "ID del quesito mancante"})
		return
	}

	if err := logic.UpdateQuesito(quesito); err != nil { // [AGGIUNTO]
		c.JSON(mapErrorToStatus(err), gin.H{"error": "Errore nella modifica del quesito"})
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
	if err := logic.DeleteQuesito(id); err != nil { // [AGGIUNTO]
		c.JSON(mapErrorToStatus(err), gin.H{"error": "Errore nella cancellazione del quesito"})
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
	quesiti, err := logic.GetQuesitiByDocente(idDocente) // [AGGIUNTO]
	if err != nil {
		c.JSON(mapErrorToStatus(err), gin.H{"error": "Errore nel recupero dei quesiti per il docente"})
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

	categorie, err := logic.GetQuesitiByDocenteAndCategoria(idDocente, categoria) // [AGGIUNTO]
	if err != nil {
		c.JSON(mapErrorToStatus(err), gin.H{"error": "Errore nel recupero delle categorie per il docente e la categoria"})
		return
	}
	c.JSON(http.StatusOK, categorie)
}

// Funzioni CRUD per gli utenti
func GetUtente(c *gin.Context) {
	id := c.Param("id_utente")
	if id == "" {
		c.JSON(http.StatusBadRequest, gin.H{"error": "ID utente mancante"})
		return
	}

	utente, err := logic.GetUtente(id) // [AGGIUNTO]
	if err != nil {
		c.JSON(mapErrorToStatus(err), gin.H{"error": "Utente non trovato"})
		return
	}

	c.JSON(http.StatusOK, utente)
}

func UpdateUtente(c *gin.Context) {

	var utente models.Utente
	if err := c.ShouldBindJSON(&utente); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	if utente.ID == 0 {
		c.JSON(http.StatusBadRequest, gin.H{"error": "ID dell'utente mancante"})
		return
	}

	if err := logic.UpdateUtente(utente); err != nil { // [AGGIUNTO]
		c.JSON(mapErrorToStatus(err), gin.H{"error": "Errore nella modifica dell'utente"})
		return
	}

	c.JSON(http.StatusOK, gin.H{"message": "Utente modificato con successo"})
}

// -------------------- helpers controller --------------------

// [AGGIUNTO] mapping errori dominio -> HTTP
func mapErrorToStatus(err error) int {
	switch {
	case errors.Is(err, logic.ErrBadRequest):
		return http.StatusBadRequest
	case errors.Is(err, logic.ErrNotFound):
		return http.StatusNotFound
	default:
		return http.StatusInternalServerError
	}
}
