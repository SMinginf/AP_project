package handlers

import (
	"AP_project/quiz_service/quiz_logic"

	"github.com/gin-gonic/gin"
)

// Nel framework Gin, ogni richiesta HTTP in arrivo viene rappresentata da un oggetto *gin.Context chiamato comunemente c.
// Il metodo c.ShouldBindJson(&input):
// 1. Estrae il body JSON della richiesta HTTP.
// 2. Fa il parsing (deserializza) il JSON nei campi della struttura input (nel nostro esempio, una struct tipo CreateQuizInput).
// 3. Controlla i tag binding:"required" e fallisce se mancano campi obbligatori.
// 4. Se tutto va bene, la struct input conterrà i valori inviati dal client.

func CreateQuizStudente(ctx *gin.Context) {
	quiz_logic.CreateQuiz(ctx, true) // isPublic = true, isPrivate = false
}
func CreateQuizDocente(ctx *gin.Context) {
	quiz_logic.CreateQuiz(ctx, false) // isPublic = false, isPrivate = true
}

func StoreQuiz(ctx *gin.Context) {
	quiz_logic.StoreQuiz(ctx)
}

// func StoreQuiz(c *gin.Context) {

// 	var input models.StoreQuizInput
// 	if err := c.ShouldBindJSON(&input); err != nil {
// 		fmt.Println("Errore:", err.Error())
// 		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
// 		return
// 	}

// 	var quesiti []models.Quesito
// 	if err := database.DB.Where("id IN ?", input.IdQuesiti).Find(&quesiti).Error; err != nil {
// 		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nel recupero dei quesiti"})
// 		return
// 	}

// 	data, err := time.Parse("2006-01-02 15:04:05", input.DataCreazione)
// 	if err != nil {
// 		// gestisci errore di parsing
// 		c.JSON(http.StatusBadRequest, gin.H{"error": "Formato data non valido. Usa 'YYYY-MM-DD HH:MM:SS'"})
// 		return
// 	}

// 	quiz := models.Quiz{
// 		IDUtente:          input.IdUtente,
// 		Difficolta:        input.Difficolta,
// 		Quantita:          input.Quantita,
// 		Durata:            input.Durata,
// 		Data:              data,
// 		RisposteCorrette:  input.RisposteCorrette,
// 		RisposteSbagliate: input.RisposteSbagliate,
// 	}

// 	if err := database.DB.Create(&quiz).Error; err != nil {
// 		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nella memorizzazione del quiz"})
// 		return
// 	}

// 	// Inserisco le relazioni tra quiz e quesiti nella tabella di join
// 	for i, quesito := range quesiti {
// 		quizQuesito := models.QuizQuesiti{
// 			QuizID:         quiz.ID,
// 			QuesitoID:      quesito.ID,
// 			RispostaUtente: input.Risposte[i],
// 		}

// 		if err := database.DB.Create(&quizQuesito).Error; err != nil {
// 			c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nella memorizzazione della relazione quiz-quesito"})
// 			return
// 		}
// 	}

// 	c.JSON(http.StatusCreated, gin.H{"message": "Quiz creato con successo", "quiz_id": quiz.ID})

// }

// func CreateQuiz(c *gin.Context, isPublic bool, isPrivate bool) {

// 	if !isPublic && !isPrivate || !isPublic && isPrivate {
// 		isPublic = true
// 		isPrivate = false
// 	}

// 	var input models.CreateQuizInput
// 	if err := c.ShouldBindJSON(&input); err != nil {
// 		fmt.Println("Errore:", err.Error())
// 		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
// 		return
// 	}

// 	var quiz *models.CreateQuizOutput
// 	var err error

// 	if input.AIGenerated { // --- TEST GENERATO CON AI ---
// 		if input.AICategoria == "" {
// 			c.JSON(http.StatusBadRequest, gin.H{"error": "Devi specificare una categoria"})
// 			return
// 		}
// 		quiz, err = utils.GenerateAIQuiz(input.AICategoria, input.Difficolta, input.Quantita)
// 		if err != nil {
// 			fmt.Println("Errore:", err.Error())
// 			c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
// 			return
// 		}
// 	} else { // --- TEST GENERATO MANUALMENTE ---
// 		if len(input.IdCategorie) == 0 {
// 			c.JSON(http.StatusBadRequest, gin.H{"error": "Devi specificare almeno una categoria"})
// 			return
// 		}
// 		// --- Recupero e controllo delle categorie pubbliche i cui ID sono specificati in input.IdCategorie ---
// 		var categorie []models.Categoria
// 		if err := database.DB.Where("id IN ? AND pubblica = ? AND privata = ?", input.IdCategorie, isPublic, isPrivate).Find(&categorie).Error; err != nil {
// 			c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nella verifica delle categorie"})
// 			return
// 		}
// 		if len(categorie) != len(input.IdCategorie) {
// 			c.JSON(http.StatusNotFound, gin.H{"error": "Alcune categorie specificate non esistono o non sono pubbliche"})
// 			return
// 		}

// 		// --- Query domande (unione/intersezione) ---
// 		var quesiti []models.Quesito
// 		var query *gorm.DB
// 		if input.Unione {
// 			// Unione: quesiti che abbiano almeno una delle categorie
// 			query = database.DB.
// 				Joins("JOIN categoria_quesito cq ON cq.id_quesito = quesiti.id").
// 				Where("cq.id_categoria IN ?", input.IdCategorie)

// 		} else {
// 			// Intersezione: quesiti che appartengano a tutte le categorie
// 			subQuery := database.DB.
// 				Table("categoria_quesito").
// 				Select("id_quesito").
// 				Where("id_categoria IN ?", input.IdCategorie).
// 				Group("id_quesito").
// 				Having("COUNT(DISTINCT id_categoria) = ?", len(input.IdCategorie))
// 			query = database.DB.Where("id IN (?)", subQuery)

// 		}

// 		// Il controllo sulla difficoltà lo inserisco solo se non è "Qualsiasi"
// 		if input.Difficolta != "Qualsiasi" {
// 			query = query.Where("quesiti.difficolta = ?", input.Difficolta)
// 		}
// 		// Eseguo la query per ottenere un insieme randomico di  quelle domande
// 		err = query.Preload("Categorie").Order("RAND()").Limit(input.Quantita).Find(&quesiti).Error

// 		if err != nil {
// 			c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nel recupero delle domande"})
// 			return
// 		}
// 		if len(quesiti) < input.Quantita {
// 			msg := "Non ci sono abbastanza domande nelle categorie selezionate e nella difficoltà specificata"
// 			if !input.Unione {
// 				msg = "Non ci sono abbastanza domande che appartengono a tutte le categorie selezionate nella difficoltà specificata"
// 			}
// 			c.JSON(http.StatusBadRequest, gin.H{"error": msg})
// 			return
// 		}
// 		quiz = &models.CreateQuizOutput{
// 			AIGenerated: input.AIGenerated,
// 			Categoria:   fmt.Sprintf("%v", input.IdCategorie),
// 			Difficolta:  input.Difficolta,
// 			Quantita:    input.Quantita,
// 			Quesiti:     quesiti,
// 		}
// 	}

// 	fmt.Printf("Quiz generato: %+v\n", quiz)
// 	c.JSON(http.StatusCreated, quiz)
// }

// package handlers

// import (
// 	"AP_project/quiz_service/database"
// 	"AP_project/quiz_service/models"
// 	"AP_project/quiz_service/utils"
// 	"fmt"
// 	"math/rand/v2"
// 	"net/http"
// 	"time"

// 	"github.com/gin-gonic/gin"
// )

// // Nel framework Gin, ogni richiesta HTTP in arrivo viene rappresentata da un oggetto *gin.Context chiamato comunemente c.
// // Il metodo c.ShouldBindJson(&input):
// // 1. Estrae il body JSON della richiesta HTTP.
// // 2. Fa il parsing (deserializza) il JSON nei campi della struttura input (nel nostro esempio, una struct tipo CreateQuizInput).
// // 3. Controlla i tag binding:"required" e fallisce se mancano campi obbligatori.
// // 4. Se tutto va bene, la struct input conterrà i valori inviati dal client.

// // Funzione di supporto per restituire il minimo tra due numeri
// func mancatoMin(a, b int) int {
// 	if a < b {
// 		return a
// 	}
// 	return b
// }

// func CreateQuiz(c *gin.Context) {
// 	var input models.CreateQuizInput
// 	if err := c.ShouldBindJSON(&input); err != nil {
// 		// Errore di binding dei dati JSON inviati dal client
// 		fmt.Println("Errore:", err.Error())
// 		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
// 		return
// 	}

// 	var quiz *models.CreateQuizOutput
// 	var err error

// 	if input.AIGenerated {
// 		// Generazione quiz tramite AI (Groq API)
// 		if input.AICategoria == "" {
// 			c.JSON(http.StatusBadRequest, gin.H{"error": "Devi specificare una categoria"})
// 			return
// 		}
// 		quiz, err = utils.GenerateAIQuiz(input.AICategoria, input.Difficolta, input.Quantita)
// 		if err != nil {
// 			fmt.Println("Errore:", err.Error())
// 			c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
// 			return
// 		}
// 	} else {
// 		// Generazione manuale del quiz (con quesiti prelevati dal database)
// 		if len(input.IdCategorie) == 0 {
// 			c.JSON(http.StatusBadRequest, gin.H{"error": "Devi specificare almeno una categoria"})
// 			return
// 		}

// 		// Verifica che le categorie siano pubbliche
// 		var categorie []models.Categoria
// 		if err := database.DB.Where("id IN ? AND pubblica = ?", input.IdCategorie, true).Find(&categorie).Error; err != nil {
// 			c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nella verifica delle categorie"})
// 			return
// 		}
// 		if len(categorie) != len(input.IdCategorie) {
// 			c.JSON(http.StatusNotFound, gin.H{"error": "Alcune categorie specificate non esistono o non sono pubbliche"})
// 			return
// 		}

// 		if input.Unione {

// 			// --- QUIZ PER UNIONE DI CATEGORIE CON DISTRIBUZIONE BILANCIATA ---

// 			// Recupera listaQuesiti i quesiti disponibili per ciascuna categoria selezionata
// 			catQuesitiMap := make(map[uint][]models.Quesito)
// 			availablePerCat := make(map[uint]int)

// 			for _, catID := range input.IdCategorie {
// 				var catQuesiti []models.Quesito
// 				q := database.DB.
// 					Joins("JOIN categoria_quesito cq ON cq.id_quesito = quesiti.id").
// 					Where("cq.id_categoria = ?", catID)
// 				if input.Difficolta != "Qualsiasi" {
// 					q = q.Where("quesiti.difficolta = ?", input.Difficolta)
// 				}
// 				if err := q.Find(&catQuesiti).Error; err != nil {
// 					c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nel recupero delle domande per categoria"})
// 					return
// 				}
// 				availablePerCat[catID] = len(catQuesiti)
// 				catQuesitiMap[catID] = catQuesiti
// 			}

// 			// Verifica se ci sono abbastanza quesiti in totale per soddisfare la richiesta
// 			totalAvailable := 0
// 			for _, v := range availablePerCat {
// 				totalAvailable += v
// 			}
// 			if totalAvailable < input.Quantita {
// 				c.JSON(http.StatusBadRequest, gin.H{"error": "Non ci sono abbastanza domande per generare il quiz richiesto"})
// 				return
// 			}

// 			// Calcola una distribuzione bilanciata (o quasi) dei quesiti tra le categorie
// 			numCategorie := len(input.IdCategorie)
// 			baseCount := input.Quantita / numCategorie
// 			resto := input.Quantita % numCategorie
// 			distribuzione := make(map[uint]int)
// 			for i, catID := range input.IdCategorie {
// 				count := baseCount
// 				if i < resto { // distribuisco le domande rimanenti tra le prime categorie
// 					count++
// 				}
// 				if count > availablePerCat[catID] {
// 					count = availablePerCat[catID]
// 				}
// 				distribuzione[catID] = count
// 			}

// 			// Se ci sono ancora quesiti da assegnare (es. perché alcune categorie avevano troppi pochi quesiti e quindi qule resto non è stato distribuito),
// 			// cerchiamo di ridistribuirli tra le altre categorie che hanno ancora disponibilità
// 			totassegnati := 0
// 			for _, v := range distribuzione {
// 				totassegnati += v
// 			}
// 			avanzo := input.Quantita - totassegnati
// 			if avanzo > 0 {
// 				// Scorriamo tutte le categorie selezionate
// 				for _, catID := range input.IdCategorie {
// 					// Calcoliamo quanti quesiti in più questa categoria potrebbe ancora fornire
// 					mancano := availablePerCat[catID] - distribuzione[catID]

// 					if mancano > 0 {
// 						// Determiniamo quanti quesiti possiamo effettivamente prendere:
// 						// il minimo tra quelli che mancano ancora (avanzo) e quelli disponibili (mancano)
// 						toAdd := mancatoMin(avanzo, mancano)

// 						// Aggiungiamo quei quesiti alla distribuzione della categoria corrente
// 						distribuzione[catID] += toAdd

// 						// Riduciamo il numero di quesiti ancora da assegnare
// 						avanzo -= toAdd

// 						// Se abbiamo assegnato listaQuesiti i quesiti richiesti, possiamo uscire dal ciclo
// 						if avanzo == 0 {
// 							break
// 						}
// 					}
// 				}
// 			}

// 			// Estraiamo casualmente i quesiti secondo la distribuzione calcolata
// 			quesiti := []models.Quesito{}
// 			for _, catID := range input.IdCategorie {
// 				// Recupera tutti i quesiti disponibili per questa categoria
// 				listaQuesiti := catQuesitiMap[catID]

// 				// Numero di quesiti da prendere per questa categoria, secondo la distribuzione calcolata prima
// 				quanti := distribuzione[catID]

// 				// Se per questa categoria non dobbiamo prendere nessun quesito, passa alla prossima
// 				if quanti == 0 {
// 					continue
// 				}

// 				// Mischia in modo casuale l'ordine dei quesiti disponibili per evitare di prendere sempre gli stessi
// 				// rand.Shuffle in Go richiede di specificare una funzione di scambio per mescolare gli elementi.
// 				// i e j sono gli indici degli elementi da scambiare selezionati automaticamente da rand.Shuffle
// 				rand.Shuffle(len(listaQuesiti), func(i, j int) {
// 					listaQuesiti[i], listaQuesiti[j] = listaQuesiti[j], listaQuesiti[i]
// 				})

// 				// Aggiunge i primi 'quanti' quesiti (che ora sono in ordine casuale) alla lista finale del quiz
// 				quesiti = append(quesiti, listaQuesiti[:quanti]...)
// 			}

// 			// Mescola l'elenco finale dei quesiti prima di inviarli al client
// 			rand.Shuffle(len(quesiti), func(i, j int) {
// 				quesiti[i], quesiti[j] = quesiti[j], quesiti[i]
// 			})

// 			// Costruisce la risposta finale del quiz
// 			quiz = &models.CreateQuizOutput{
// 				AIGenerated: input.AIGenerated,
// 				Difficolta:  input.Difficolta,
// 				Quantita:    input.Quantita,
// 				Quesiti:     quesiti,
// 			}

// 		} else {
// 			// --- INTERSEZIONE: quiz con quesiti che appartengono a tutte le categorie selezionate ---
// 			subQuery := database.DB.
// 				Table("categoria_quesito").
// 				Select("id_quesito").
// 				Where("id_categoria IN ?", input.IdCategorie).
// 				Group("id_quesito").
// 				Having("COUNT(DISTINCT id_categoria) = ?", len(input.IdCategorie))

// 			query := database.DB.Where("id IN (?)", subQuery)

// 			if input.Difficolta != "Qualsiasi" {
// 				query = query.Where("quesiti.difficolta = ?", input.Difficolta)
// 			}

// 			var quesiti []models.Quesito
// 			err = query.Order("RAND()").Limit(input.Quantita).Find(&quesiti).Error
// 			if err != nil {
// 				c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nel recupero delle domande"})
// 				return
// 			}
// 			if len(quesiti) < input.Quantita {
// 				c.JSON(http.StatusBadRequest, gin.H{"error": "Non ci sono abbastanza domande che appartengono a tutte le categorie selezionate nella difficoltà specificata"})
// 				return
// 			}

// 			quiz = &models.CreateQuizOutput{
// 				AIGenerated: input.AIGenerated,
// 				Difficolta:  input.Difficolta,
// 				Quantita:    input.Quantita,
// 				Quesiti:     quesiti,
// 			}
// 		}
// 	}

// 	// Stampa di debug del quiz generato
// 	fmt.Printf("Quiz generato: %+v\n", quiz)

// 	// Risposta al client
// 	c.JSON(http.StatusCreated, quiz)
// }

// func StoreQuiz(c *gin.Context) {

// 	var input models.StoreQuizInput
// 	if err := c.ShouldBindJSON(&input); err != nil {
// 		fmt.Println("Errore:", err.Error())
// 		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
// 		return
// 	}

// 	var quesiti []models.Quesito
// 	if err := database.DB.Where("id IN ?", input.IdQuesiti).Find(&quesiti).Error; err != nil {
// 		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nel recupero dei quesiti"})
// 		return
// 	}

// 	data, err := time.Parse("2006-01-02 15:04:05", input.DataCreazione)
// 	if err != nil {
// 		// gestisci errore di parsing
// 		c.JSON(http.StatusBadRequest, gin.H{"error": "Formato data non valido. Usa 'YYYY-MM-DD HH:MM:SS'"})
// 		return
// 	}

// 	quiz := models.Quiz{
// 		IDUtente:          input.IdUtente,
// 		Difficolta:        input.Difficolta,
// 		Quantita:          input.Quantita,
// 		Durata:            input.Durata,
// 		Data:              data,
// 		RisposteCorrette:  input.RisposteCorrette,
// 		RisposteSbagliate: input.RisposteSbagliate,
// 	}

// 	if err := database.DB.Create(&quiz).Error; err != nil {
// 		c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nella memorizzazione del quiz"})
// 		return
// 	}

// 	// Inserisco le relazioni tra quiz e quesiti nella tabella di join
// 	for i, quesito := range quesiti {
// 		quizQuesito := models.QuizQuesiti{
// 			QuizID:         quiz.ID,
// 			QuesitoID:      quesito.ID,
// 			RispostaUtente: input.Risposte[i],
// 		}

// 		if err := database.DB.Create(&quizQuesito).Error; err != nil {
// 			c.JSON(http.StatusInternalServerError, gin.H{"error": "Errore nella memorizzazione della relazione quiz-quesito"})
// 			return
// 		}

// 	}

// 	c.JSON(http.StatusCreated, gin.H{"message": "Quiz creato con successo", "quiz_id": quiz.ID})

// }
