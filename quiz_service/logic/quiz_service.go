package logic

import (
	"AP_project/quiz_service/database"
	"AP_project/quiz_service/models"
	"AP_project/quiz_service/utils"
	"archive/zip"
	"bytes"
	"encoding/csv"
	"errors" // [AGGIORNATO] per esporre errori sentinella e permettere al controller di usare errors.Is
	"fmt"
	"io"
	"math/rand"
	"strconv"
	"time"
)

// Errori sentinella per consentire al controller di mappare correttamente gli HTTP status
var (
	ErrBadRequest = errors.New("bad request")
	ErrNotFound   = errors.New("not found")
)

// -------------------- BUSINESS LOGIC: CREATE QUIZ --------------------

func CreateQuiz(input models.CreateQuizInput, isStudent bool) (*models.CreateQuizOutput, error) { // [AGGIORNATO] passa il ruolo
	var quiz *models.CreateQuizOutput
	var err error

	if input.AIGenerated {
		// Generazione quiz tramite AI (Groq API)
		// [AGGIORNATO] controllo su input.AICategoria spostato nel controller
		quiz, err = utils.GenerateAIQuiz(input.AICategoria, input.Difficolta, input.Quantita)
		if err != nil {
			// [AGGIORNATO] logging lasciato al chiamante; restituiamo solo l'errore incapsulato
			return nil, fmt.Errorf("GenerateAIQuiz: %w", err)
		}
		return quiz, nil
	}

	// Generazione manuale del quiz (con quesiti prelevati dal database)
	var categorie []models.Categoria
	qu := database.DB.Where("id IN ?", input.IdCategorie)
	if isStudent {
		qu = qu.Where("pubblica = ?", true)
	}
	if err := qu.Find(&categorie).Error; err != nil {
		return nil, fmt.Errorf("errore nella verifica delle categorie: %w", err)
	}
	if len(categorie) != len(input.IdCategorie) {
		if isStudent {
			return nil, wrap(ErrNotFound, "Alcune categorie specificate non esistono o non sono pubbliche")
		}
		return nil, wrap(ErrNotFound, "Alcune categorie specificate non esistono")
	}

	if input.Unione {
		// --- QUIZ PER UNIONE DI CATEGORIE CON DISTRIBUZIONE BILANCIATA ---
		return createQuizByUnion(input)
	}

	// --- INTERSEZIONE: quiz con quesiti che appartengono a tutte le categorie selezionate ---
	return createQuizByIntersection(input)
}

func createQuizByUnion(input models.CreateQuizInput) (*models.CreateQuizOutput, error) {
	// Recupera listaQuesiti i quesiti disponibili per ciascuna categoria selezionata
	catQuesitiMap := make(map[uint][]models.Quesito)
	availablePerCat := make(map[uint]int)

	for _, catID := range input.IdCategorie {
		var catQuesiti []models.Quesito

		q := database.DB.Preload("Categorie"). // [AGGIORNATO] Preload per caricare le categorie associate
							Joins("JOIN categoria_quesito cq ON cq.id_quesito = quesiti.id").
							Where("cq.id_categoria = ?", catID)

		if len(input.IdQuesiti) > 0 { // [AGGIUNTO] Reroll: escludi i già usati
			q = q.Where("quesiti.id NOT IN ?", input.IdQuesiti)
		}
		// Aggiungi eventuali filtri aggiuntivi
		if input.Difficolta != "Qualsiasi" {
			q = q.Where("quesiti.difficolta = ?", input.Difficolta)
		}
		if err := q.Find(&catQuesiti).Error; err != nil {
			return nil, fmt.Errorf("errore nel recupero delle domande per categoria: %w", err)
		}
		availablePerCat[catID] = len(catQuesiti)
		catQuesitiMap[catID] = catQuesiti
	}

	// Verifica se ci sono abbastanza quesiti in totale per soddisfare la richiesta
	totalAvailable := 0
	for _, v := range availablePerCat {
		totalAvailable += v
	}
	if totalAvailable < input.Quantita {
		return nil, wrap(ErrBadRequest, "Non ci sono abbastanza domande per generare il quiz richiesto")
	}

	// Calcola una distribuzione bilanciata (o quasi) dei quesiti tra le categorie
	numCategorie := len(input.IdCategorie)
	baseCount := input.Quantita / numCategorie
	resto := input.Quantita % numCategorie
	distribuzione := make(map[uint]int)
	for i, catID := range input.IdCategorie {
		count := baseCount
		if i < resto { // distribuisco le domande rimanenti tra le prime categorie
			count++
		}
		if count > availablePerCat[catID] {
			count = availablePerCat[catID]
		}
		distribuzione[catID] = count
	}

	// Se ci sono ancora quesiti da assegnare (es. perché alcune categorie avevano troppi pochi quesiti e quindi quel resto non è stato distribuito),
	// cerchiamo di ridistribuirli tra le altre categorie che hanno ancora disponibilità
	totassegnati := 0
	for _, v := range distribuzione {
		totassegnati += v
	}
	avanzo := input.Quantita - totassegnati
	if avanzo > 0 {
		// Scorriamo tutte le categorie selezionate
		for _, catID := range input.IdCategorie {
			// Calcoliamo quanti quesiti in più questa categoria potrebbe ancora fornire
			mancano := availablePerCat[catID] - distribuzione[catID]

			if mancano > 0 {
				// Determiniamo quanti quesiti possiamo effettivamente prendere:
				// il minimo tra quelli che mancano ancora (avanzo) e quelli disponibili (mancano)
				toAdd := min(avanzo, mancano) // [AGGIORNATO] helper min implementato in fondo

				// Aggiungiamo quei quesiti alla distribuzione della categoria corrente
				distribuzione[catID] += toAdd

				// Riduciamo il numero di quesiti ancora da assegnare
				avanzo -= toAdd

				// Se abbiamo assegnato listaQuesiti i quesiti richiesti, possiamo uscire dal ciclo
				if avanzo == 0 {
					break
				}
			}
		}
	}

	// Estraiamo casualmente i quesiti secondo la distribuzione calcolata
	quesiti := []models.Quesito{}
	for _, catID := range input.IdCategorie {
		// Recupera tutti i quesiti disponibili per questa categoria
		listaQuesiti := catQuesitiMap[catID]

		// Numero di quesiti da prendere per questa categoria, secondo la distribuzione calcolata prima
		quanti := distribuzione[catID]

		// Se per questa categoria non dobbiamo prendere nessun quesito, passa alla prossima
		if quanti == 0 {
			continue
		}

		// Mischia in modo casuale l'ordine dei quesiti disponibili per evitare di prendere sempre gli stessi
		// rand.Shuffle in Go richiede di specificare una funzione di scambio per mescolare gli elementi.
		// i e j sono gli indici degli elementi da scambiare selezionati automaticamente da rand.Shuffle
		rand.Shuffle(len(listaQuesiti), func(i, j int) {
			listaQuesiti[i], listaQuesiti[j] = listaQuesiti[j], listaQuesiti[i]
		})

		// Aggiunge i primi 'quanti' quesiti (che ora sono in ordine casuale) alla lista finale del quiz
		quesiti = append(quesiti, listaQuesiti[:quanti]...)
	}

	// Mescola l'elenco finale dei quesiti prima di inviarli al client
	rand.Shuffle(len(quesiti), func(i, j int) {
		quesiti[i], quesiti[j] = quesiti[j], quesiti[i]
	})

	// Costruisce la risposta finale del quiz
	return &models.CreateQuizOutput{
		AIGenerated: input.AIGenerated,
		Difficolta:  input.Difficolta,
		Quantita:    input.Quantita,
		Quesiti:     quesiti,
	}, nil
}

func createQuizByIntersection(input models.CreateQuizInput) (*models.CreateQuizOutput, error) {
	// --- INTERSEZIONE: quiz con quesiti che appartengono a tutte le categorie selezionate ---
	subQuery := database.DB.
		Table("categoria_quesito").
		Select("id_quesito").
		Where("id_categoria IN ?", input.IdCategorie).
		Group("id_quesito").
		Having("COUNT(DISTINCT id_categoria) = ?", len(input.IdCategorie))

	query := database.DB.Where("id IN (?)", subQuery)

	if len(input.IdQuesiti) > 0 { // [AGGIUNTO] Reroll: escludi i già usati
		query = query.Where("quesiti.id NOT IN ?", input.IdQuesiti)
	}

	if input.Difficolta != "Qualsiasi" {
		query = query.Where("quesiti.difficolta = ?", input.Difficolta)
	}

	var quesiti []models.Quesito
	err := query.Preload("Categorie").Order("RAND()").Limit(input.Quantita).Find(&quesiti).Error
	if err != nil {
		return nil, fmt.Errorf("errore nel recupero delle domande: %w", err)
	}
	if len(quesiti) < input.Quantita {
		return nil, wrap(ErrBadRequest, "Non ci sono abbastanza domande che appartengono a tutte le categorie selezionate nella difficoltà specificata")
	}

	return &models.CreateQuizOutput{
		AIGenerated: input.AIGenerated,
		Difficolta:  input.Difficolta,
		Quantita:    input.Quantita,
		Quesiti:     quesiti,
	}, nil
}

// -------------------- BUSINESS LOGIC: STORE QUIZ --------------------

func StoreQuiz(input models.StoreQuizInput) (uint, error) {
	var quesiti []models.Quesito
	if err := database.DB.Where("id IN ?", input.IdQuesiti).Find(&quesiti).Error; err != nil {
		return 0, fmt.Errorf("errore nel recupero dei quesiti: %w", err)
	}

	data, err := time.Parse("2006-01-02 15:04:05", input.DataCreazione)
	if err != nil {
		// gestisci errore di parsing
		return 0, wrap(ErrBadRequest, "Formato data non valido. Usa 'YYYY-MM-DD HH:MM:SS'")
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
		return 0, fmt.Errorf("errore nella memorizzazione del quiz: %w", err)
	}

	// Inserisco le relazioni tra quiz e quesiti nella tabella di join
	for i, quesito := range quesiti {
		quizQuesito := models.QuizQuesiti{
			QuizID:         quiz.ID,
			QuesitoID:      quesito.ID,
			RispostaUtente: input.Risposte[i],
		}

		if err := database.DB.Create(&quizQuesito).Error; err != nil {
			return 0, fmt.Errorf("errore nella memorizzazione della relazione quiz-quesito: %w", err)
		}
	}

	return quiz.ID, nil
}

// --------------- CODICE PER LA GENERAZIONE DELLE SCHEDE D'ESAME -----------------

type option struct {
	Label     string // A/B/C/D
	Text      string
	IsCorrect bool
}

func copyAndShuffle(r *rand.Rand, qs []models.Quesito) []models.Quesito {
	out := make([]models.Quesito, len(qs))
	copy(out, qs)
	r.Shuffle(len(out), func(i, j int) { out[i], out[j] = out[j], out[i] })
	return out
}

func findCorrectLabel(ops []option) string {
	for _, op := range ops {
		if op.IsCorrect {
			return op.Label
		}
	}
	return ""
}

func writeQuestion(w io.Writer, num int, q models.Quesito, ops []option) error {
	if _, err := io.WriteString(w, fmt.Sprintf("Quesito %d:\n%s\n", num, q.Testo)); err != nil {
		return err
	}
	for _, op := range ops {
		if _, err := io.WriteString(w, fmt.Sprintf("  %s) %s\n", op.Label, op.Text)); err != nil {
			return err
		}
	}
	_, err := io.WriteString(w, "\n")
	return err
}

func shuffledOptions(r *rand.Rand, q models.Quesito) []option {
	ops := []option{
		{Label: "", Text: q.OpzioneA, IsCorrect: q.OpCorretta == 0},
		{Label: "", Text: q.OpzioneB, IsCorrect: q.OpCorretta == 1},
		{Label: "", Text: q.OpzioneC, IsCorrect: q.OpCorretta == 2},
		{Label: "", Text: q.OpzioneD, IsCorrect: q.OpCorretta == 3},
	}
	r.Shuffle(len(ops), func(i, j int) { ops[i], ops[j] = ops[j], ops[i] })

	// Assegna le etichette in ordine A/B/C/D dopo lo shuffle
	labels := []string{"A", "B", "C", "D"}

	for i := range ops {
		ops[i].Label = labels[i]
	}

	return ops
}

func GenerateExamZIP(questions []models.Quesito, n int, w io.Writer) (retErr error) {
	if w == nil {
		return wrap(ErrBadRequest, "writer nullo")
	}

	r := rand.New(rand.NewSource(time.Now().UnixNano()))
	zw := zip.NewWriter(w)

	// Defer 1: chiusura ZIP
	defer func() {
		if cerr := zw.Close(); cerr != nil && retErr == nil {
			retErr = fmt.Errorf("chiusura zip: %w", cerr)
		}
		fmt.Println("[DEBUG] ZIP chiuso correttamente")
	}()

	// Crea un buffer in memoria per il CSV
	var csvBuffer bytes.Buffer
	cw := csv.NewWriter(&csvBuffer)
	cw.Comma = ';'
	cw.UseCRLF = true

	// Scrittura intestazione e righe delle soluzioni nel buffer in memoria
	if err := cw.Write([]string{"SCHEDA", "QUESITO", "RISPOSTA CORRETTA"}); err != nil {
		return fmt.Errorf("scrittura header soluzioni.csv: %w", err)
	}

	// Generazione schede
	for i := 1; i <= n; i++ {
		shuffled := copyAndShuffle(r, questions)

		entry, err := zw.Create(fmt.Sprintf("scheda_%d.txt", i))
		if err != nil {
			return fmt.Errorf("creazione scheda_%d.txt: %w", i, err)
		}

		if _, err := io.WriteString(entry,
			fmt.Sprintf("Scheda d'esame #%d\n=====================================\n\n", i),
		); err != nil {
			return fmt.Errorf("scrittura intestazione scheda_%d: %w", i, err)
		}

		for qIdx, q := range shuffled {
			ops := shuffledOptions(r, q)

			if err := writeQuestion(entry, qIdx+1, q, ops); err != nil {
				return fmt.Errorf("scheda_%d, quesito_%d: %w", i, qIdx+1, err)
			}

			correct := findCorrectLabel(ops)
			if correct == "" {
				return fmt.Errorf("scheda_%d, quesito_%d: nessuna opzione marcata come corretta", i, qIdx+1)
			}

			// Scrivi la riga nel buffer del CSV, NON nel file ZIP
			if err := cw.Write([]string{strconv.Itoa(i), strconv.Itoa(qIdx + 1), correct}); err != nil {
				return fmt.Errorf("scrittura soluzioni (scheda_%d, quesito_%d): %w", i, qIdx+1, err)
			}

		}
	}

	// Forza il flush finale del buffer del CSV
	cw.Flush()
	if ferr := cw.Error(); ferr != nil {
		return fmt.Errorf("flush soluzioni.csv: %w", ferr)
	}

	// A questo punto, il contenuto del CSV è completo e si trova in `csvBuffer`.
	// Crea il file "soluzioni.csv" all'interno dello ZIP e ci copia il contenuto del buffer
	solutionsFile, err := zw.Create("soluzioni.csv")
	if err != nil {
		return fmt.Errorf("creazione soluzioni.csv: %w", err)
	}
	if _, err := solutionsFile.Write(csvBuffer.Bytes()); err != nil {
		return fmt.Errorf("scrittura finale soluzioni.csv: %w", err)
	}

	return nil
}

// -------------------- helpers --------------------

func wrap(sentinel error, msg string) error { // [AGGIORNATO] per incapsulare messaggi con sentinella
	return fmt.Errorf("%w: %s", sentinel, msg)
}
