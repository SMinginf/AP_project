package handlers

import (
	"AP_project/quiz_service/database"
	"AP_project/quiz_service/models"
	"AP_project/quiz_service/utils"
	"fmt"
	"net/http"

	"github.com/gin-gonic/gin"
	"gorm.io/gorm"
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
		fmt.Println("Errore:", err.Error())
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	var quiz *models.CreateQuizOutput
	var err error

	if input.AIGenerated {
		if input.AICategoria == "" {
			c.JSON(http.StatusBadRequest, gin.H{"error": "Devi specificare almeno una categoria"})
			return
		}
		quiz, err = utils.GenerateAIQuiz(input.AICategoria, input.Difficolta, input.Quantita)
		if err != nil {
			fmt.Println("Errore:", err.Error())
			c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
			return
		}
	} else {
		if len(input.IdCategorie) == 0 {
			c.JSON(http.StatusBadRequest, gin.H{"error": "Devi specificare almeno una categoria"})
			return
		}
		// --- Recupero e controllo delle categorie pubbliche i cui ID sono specificati in input.IdCategorie ---
		var categorie []models.Categoria
		if err := database.DB.Where("id IN ? AND pubblica = ?", input.IdCategorie, true).Find(&categorie).Error; err != nil {
			c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nella verifica delle categorie"})
			return
		}
		if len(categorie) != len(input.IdCategorie) {
			c.JSON(http.StatusNotFound, gin.H{"error": "Alcune categorie specificate non esistono o non sono pubbliche"})
			return
		}

		// --- Query domande (unione/intersezione) ---
		var quesiti []models.Quesito
		var query *gorm.DB
		if input.Unione {
			// Unione: almeno una delle categorie
			query = database.DB.
				Joins("JOIN categoria_quesito cq ON cq.id_quesito = quesiti.id").
				Where("cq.id_categoria IN ?", input.IdCategorie)

		} else {
			// Intersezione: tutte le categorie
			subQuery := database.DB.
				Table("categoria_quesito").
				Select("id_quesito").
				Where("id_categoria IN ?", input.IdCategorie).
				Group("id_quesito").
				Having("COUNT(DISTINCT id_categoria) = ?", len(input.IdCategorie))
			query = database.DB.Where("id IN (?)", subQuery)

		}

		// Il controllo sulla difficoltà lo inserisco solo se non è "Qualsiasi"
		if input.Difficolta != "Qualsiasi" {
			query = query.Where("quesiti.difficolta = ?", input.Difficolta)
		}
		// Eseguo la query per ottenere le domande
		err = query.Order("RAND()").Limit(input.Quantita).Find(&quesiti).Error

		if err != nil {
			c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nel recupero delle domande"})
			return
		}
		if len(quesiti) < input.Quantita {
			msg := "Non ci sono abbastanza domande nelle categorie selezionate e nella difficoltà specificata"
			if !input.Unione {
				msg = "Non ci sono abbastanza domande che appartengono a tutte le categorie selezionate nella difficoltà specificata"
			}
			c.JSON(http.StatusBadRequest, gin.H{"error": msg})
			return
		}
		quiz = &models.CreateQuizOutput{
			Categoria:  fmt.Sprintf("%v", input.IdCategorie),
			Difficolta: input.Difficolta,
			Quantita:   input.Quantita,
			Quesiti:    quesiti,
		}
	}

	fmt.Printf("Quiz generato: %+v\n", quiz)
	c.JSON(http.StatusCreated, quiz)
}
