package quiz_logic

import (
	"AP_project/quiz_service/database"
	"AP_project/quiz_service/models"
	"AP_project/quiz_service/utils"
	"fmt"
	"net/http"
	"time"

	"github.com/gin-gonic/gin"
	"gorm.io/gorm"
)

func CreateQuiz(c *gin.Context, student bool) {

	var input models.CreateQuizInput
	if err := c.ShouldBindJSON(&input); err != nil {
		fmt.Println("Errore:", err.Error())
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}
	fmt.Printf("Input ricevuto: %+v\n", input)

	// Se l'input.IdQuesiti è vuoto, ovvero non ho una lista di quesiti selezionati, significa che non sto facendo un Reroll
	var reroll bool
	if len(input.IdQuesiti) > 0 {
		reroll = true
	} else {
		reroll = false
	}

	var quiz *models.CreateQuizOutput
	var err error

	if input.AIGenerated { // --- TEST GENERATO CON AI ---
		if input.AICategoria == "" {
			c.JSON(http.StatusBadRequest, gin.H{"error": "Devi specificare una categoria"})
			return
		}
		quiz, err = utils.GenerateAIQuiz(input.AICategoria, input.Difficolta, input.Quantita)
		if err != nil {
			fmt.Println("Errore:", err.Error())
			c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
			return
		}
	} else { // --- TEST GENERATO MANUALMENTE ---
		if len(input.IdCategorie) == 0 {
			c.JSON(http.StatusBadRequest, gin.H{"error": "Devi specificare almeno una categoria"})
			return
		}
		// --- Recupero e controllo delle categorie pubbliche i cui ID sono specificati in input.IdCategorie ---
		var categorie []models.Categoria

		// capire come scrivere la query in modo che per student = true, consideri solo le categorie pubbliche
		// e per student = false, consideri tutte le categorie (pubbliche e private)
		qu := database.DB.Model(&models.Categoria{}).Where("id IN ?", input.IdCategorie)
		if student {
			qu = qu.Where("pubblica = ?", true)
		}
		if err := qu.Find(&categorie).Error; err != nil {
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
		if !reroll {
			if input.Unione {
				// Unione: quesiti che abbiano almeno una delle categorie
				query = database.DB.
					Joins("JOIN categoria_quesito cq ON cq.id_quesito = quesiti.id").
					Where("cq.id_categoria IN ?", input.IdCategorie)

			} else {
				// Intersezione: quesiti che appartengano a tutte le categorie
				subQuery := database.DB.
					Table("categoria_quesito").
					Select("id_quesito").
					Where("id_categoria IN ?", input.IdCategorie).
					Group("id_quesito").
					Having("COUNT(DISTINCT id_categoria) = ?", len(input.IdCategorie))
				query = database.DB.Where("id IN (?)", subQuery)

			}
		} else {
			// Se sto facendo un Reroll, escludo i quesiti specificati in input.IdQuesiti
			query = database.DB.
				Joins("JOIN categoria_quesito cq ON cq.id_quesito = quesiti.id").
				Where("cq.id_categoria IN ? AND cq.id_quesito NOT IN ?", input.IdCategorie, input.IdQuesiti) // Considera solo le categorie specificate
		}

		// Il controllo sulla difficoltà lo inserisco solo se non è "Qualsiasi"
		if input.Difficolta != "Qualsiasi" {
			query = query.Where("quesiti.difficolta = ?", input.Difficolta)
		}
		// Eseguo la query per ottenere un insieme randomico di  quelle domande
		err = query.Preload("Categorie").Order("RAND()").Limit(input.Quantita).Find(&quesiti).Error

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
			AIGenerated: input.AIGenerated,
			Categoria:   fmt.Sprintf("%v", input.IdCategorie),
			Difficolta:  input.Difficolta,
			Quantita:    input.Quantita,
			Quesiti:     quesiti,
		}
	}

	fmt.Printf("Quiz generato: %+v\n", quiz)
	c.JSON(http.StatusCreated, quiz)
}
func StoreQuiz(c *gin.Context) {

	var input models.StoreQuizInput
	if err := c.ShouldBindJSON(&input); err != nil {
		fmt.Println("Errore:", err.Error())
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	var quesiti []models.Quesito
	if err := database.DB.Where("id IN ?", input.IdQuesiti).Find(&quesiti).Error; err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nel recupero dei quesiti"})
		return
	}

	data, err := time.Parse("2006-01-02 15:04:05", input.DataCreazione)
	if err != nil {
		// gestisci errore di parsing
		c.JSON(http.StatusBadRequest, gin.H{"error": "Formato data non valido. Usa 'YYYY-MM-DD HH:MM:SS'"})
		return
	}

	quiz := models.Quiz{
		IDUtente:          input.IdUtente,
		Difficolta:        input.Difficolta,
		Quantita:          input.Quantita,
		Durata:            input.Durata,
		Data:              data,
		RisposteCorrette:  input.RisposteCorrette,
		RisposteSbagliate: input.RisposteSbagliate,
	}

	if err := database.DB.Create(&quiz).Error; err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nella memorizzazione del quiz"})
		return
	}

	// Inserisco le relazioni tra quiz e quesiti nella tabella di join
	for i, quesito := range quesiti {
		quizQuesito := models.QuizQuesiti{
			QuizID:         quiz.ID,
			QuesitoID:      quesito.ID,
			RispostaUtente: input.Risposte[i],
		}

		if err := database.DB.Create(&quizQuesito).Error; err != nil {
			c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nella memorizzazione della relazione quiz-quesito"})
			return
		}
	}

	c.JSON(http.StatusCreated, gin.H{"message": "Quiz creato con successo", "quiz_id": quiz.ID})

}
