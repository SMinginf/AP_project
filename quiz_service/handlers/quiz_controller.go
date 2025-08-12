package handlers

import (
	"AP_project/quiz_service/logic" // [AGGIORNATO] importa la logica applicativa estratta
	"AP_project/quiz_service/models"
	"errors" // [AGGIORNATO] per usare errors.Is
	"io"
	"net/http"

	"github.com/gin-gonic/gin"
)

// Nel framework Gin, ogni richiesta HTTP in arrivo viene rappresentata da un oggetto *gin.Context chiamato comunemente c.
// Il metodo c.ShouldBindJson(&input):
// 1. Estrae il body JSON della richiesta HTTP.
// 2. Fa il parsing (deserializza) il JSON nei campi della struttura input (nel nostro esempio, una struct tipo CreateQuizInput).
// 3. Controlla i tag binding:"required" e fallisce se mancano campi obbligatori.
// 4. Se tutto va bene, la struct input conterrà i valori inviati dal client.

func CreateQuiz(c *gin.Context) {
	var input models.CreateQuizInput
	if err := c.ShouldBindJSON(&input); err != nil {
		// Errore di binding dei dati JSON inviati dal client
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	// [AGGIUNTO] Validazioni di input spostate qui dal service
	if input.AIGenerated {
		// Generazione quiz tramite AI (Groq API)
		if input.AICategoria == "" {
			c.JSON(http.StatusBadRequest, gin.H{"error": "Devi specificare una categoria"})
			return
		}
	} else {
		// Generazione manuale del quiz (con quesiti prelevati dal database)
		if len(input.IdCategorie) == 0 {
			c.JSON(http.StatusBadRequest, gin.H{"error": "Devi specificare almeno una categoria"})
			return
		}
	}

	quiz, err := logic.CreateQuiz(input)
	if err != nil {
		c.JSON(mapErrorToStatus(err), gin.H{"error": err.Error()})
		return
	}

	// Risposta al client
	c.JSON(http.StatusCreated, quiz)
}

func StoreQuiz(c *gin.Context) {
	var input models.StoreQuizInput
	if err := c.ShouldBindJSON(&input); err != nil {
		// Errore di binding dei dati JSON inviati dal client
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	id, err := logic.StoreQuiz(input)
	if err != nil {
		c.JSON(mapErrorToStatus(err), gin.H{"error": err.Error()})
		return
	}

	c.JSON(http.StatusCreated, gin.H{"message": "Quiz creato con successo", "quiz_id": id})
}

// --------------- CODICE PER LA GENERAZIONE DELLE SCHEDE D'ESAME -----------------

// DTO per la richiesta dell'endpoint di generazione (manteniamo il nome e la posizione logica)
type generateReq struct {
	Questions []models.Quesito `json:"questions"`
	N         int              `json:"n"`
}

// Questo handler risponde con uno ZIP in streaming: mentre il server genera i file delle schede,
// li invia subito al client nella stessa risposta HTTP. Non usa file temporanei né accumula tutto in RAM.
func GenerateExams(c *gin.Context) {
	var req generateReq
	if err := c.ShouldBindJSON(&req); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	// [AGGIUNTO] Validazioni di input spostate qui dal service
	if req.N <= 0 {
		c.JSON(http.StatusBadRequest, gin.H{"error": "n deve essere > 0"})
		return
	}
	if len(req.Questions) == 0 {
		c.JSON(http.StatusBadRequest, gin.H{"error": "nessun quesito fornito"})
		return
	}

	// io.Pipe crea una coppia reader/writer collegati tra loro in memoria
	pr, pw := io.Pipe()

	// Parte in parallelo una goroutine che scrive lo ZIP direttamente su pw
	go func() {
		err := logic.GenerateExamZIP(req.Questions, req.N, pw)
		// CloseWithError propaga eventuale errore a chi legge (il client)
		_ = pw.CloseWithError(err)
	}()

	// Preparazione risposta HTTP
	c.Header("Content-Type", "application/zip") // dice al client che sta arrivando uno ZIP
	c.Status(http.StatusOK)                     // invia lo status code e blocca le intestazioni. Il body partirà con la prima scrittura.

	// io.Copy legge dalla pipe e scrive nella risposta pezzo per pezzo (chunked transfer encoding).
	// c.Writer è la connessione HTTP verso il client, pr è la pipe reader che riceve i dati generati dalla goroutine.
	// Così il client inizia a ricevere lo ZIP mentre viene creato, senza aspettare la fine.
	_, _ = io.Copy(c.Writer, pr)
}

// -------------------- helpers controller --------------------

// Mappa errori sentinella dei logic in status code HTTP
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
