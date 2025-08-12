package utils

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"net/http"

	"AP_project/quiz_service/models"
)

<<<<<<< HEAD
const GROQ_API_KEY = "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"
=======
const GROQ_API_KEY = ""
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
const GROQ_ENDPOINT = "https://api.groq.com/openai/v1/chat/completions"

func GenerateAIQuiz(categoria string, difficolta string, nd int) (*models.CreateQuizOutput, error) {
	var groqQuiz models.GroqQuiz

	// Costruzione del prompt
	prompt := fmt.Sprintf(`
Sei un assistente per la generazione di quiz universitari. Genera un quiz in **formato JSON puro** (senza testo introduttivo o conclusivo) per aiutare uno studente a esercitarsi in "%s".

Il JSON deve avere **esattamente** questa struttura:

{
  "titolo": "Quiz su %s",
  "categoria": "%s",
  "difficolta": "%s",
  "quesiti": [
    {
      "difficolta": "facile | medio | difficile",
      "testo": "Testo della domanda",
      "opzioni": ["Risposta A", "Risposta B", "Risposta C", "Risposta D"],
      "op_corretta": 0
    }
    // Altri quesiti qui...
  ]
}

Vincoli:
- Genera esattamente %d domande, pertinenti alla categoria "%s" e alla difficoltà "%s".
- La proprietà "op_corretta" è l'indice (0–3) della risposta giusta.
- Tutti i quesiti devono essere chiari, distinti e accademicamente corretti.
- Le opzioni devono essere plausibili ma non ambigue.
- Verifica sempre che la risposta corretta indicata sia effettivamente esatta.

Istruzioni specifiche per quesiti di matematica:
- Le domande devono essere formalmente corrette e logicamente coerenti.
- Le risposte devono essere matematicamente **verificate**: calcola prima il risultato corretto.
- Usa una notazione chiara per espressioni matematiche, ad esempio: ∫, d/dx, x^2, ln(x), |x|, sqrt(x), ecc.
- Evita affermazioni ambigue, giochi di parole o generalizzazioni errate.
- L'opzione corretta deve essere **l’unica risposta valida** tra le quattro.`, categoria, categoria, categoria, difficolta, nd, categoria, difficolta)

	// Preparazione della richiesta
	reqBody := map[string]interface{}{
		"model": "mistral-saba-24b", // inserisci un modello supportato da Groq
		"messages": []map[string]string{
			{
				"role":    "user",
				"content": prompt,
			},
		},
		"temperature": 0.5,

		// NECESSARIO: specifica il formato della risposta come oggetto JSON puro (senza testo extra come "Ecco a te la risposta:")
		"response_format": map[string]string{
			"type": "json_object",
		},
	}

	// Serializzazione in JSON
	jsonData, err := json.Marshal(reqBody)
	if err != nil {
		fmt.Println("Errore nella serializzazione JSON:", err)
		return nil, err
	}

	// Creazione della richiesta HTTP POST
	req, err := http.NewRequest("POST", GROQ_ENDPOINT, bytes.NewBuffer(jsonData))
	if err != nil {
		fmt.Println("Errore nella creazione della richiesta:", err)
		return nil, err
	}

	// Headers
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("Authorization", "Bearer "+GROQ_API_KEY)

	// Invio richiesta
	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		fmt.Println("Errore nell'invio della richiesta:", err)
		return nil, err
	}
	defer resp.Body.Close()

	// Lettura risposta
	body, _ := io.ReadAll(resp.Body)
	fmt.Println("Risposta Groq:")
	fmt.Println(string(body))

	// Groq API restituisce un oggetto con una lista di "choices", dobbiamo estrarre il messaggio
	var rawResp struct {
		Choices []struct {
			Message struct {
				Content string `json:"content"`
			} `json:"message"`
		} `json:"choices"`
	}

	if err := json.Unmarshal(body, &rawResp); err != nil {
		return nil, fmt.Errorf("errore parsing JSON generale: %v", err)
	}

	if len(rawResp.Choices) == 0 {
		return nil, fmt.Errorf("nessuna risposta dal modello")
	}

	// Ora parsiamo il contenuto JSON della risposta come Quiz. "rawResp.Choices[0].Message.Content"
	// è la parte della risposta di groqAPI che contiene il JSON del quiz generato.
	if err := json.Unmarshal([]byte(rawResp.Choices[0].Message.Content), &groqQuiz); err != nil {
		return nil, fmt.Errorf("errore parsing contenuto quiz: %v", err)
	}

	// Conversione da GroqQuesito a Quesito
	quiz := &models.CreateQuizOutput{
<<<<<<< HEAD
		Categoria:  groqQuiz.Categoria,
		Difficolta: groqQuiz.Difficolta,
		Quantita:   len(groqQuiz.Quesiti),
		Quesiti:    make([]models.Quesito, 0, len(groqQuiz.Quesiti)),
=======
		AIGenerated: true,
		AICategoria: groqQuiz.Categoria,
		Difficolta:  groqQuiz.Difficolta,
		Quantita:    len(groqQuiz.Quesiti),
		Quesiti:     make([]models.Quesito, 0, len(groqQuiz.Quesiti)),
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
	}

	for _, gq := range groqQuiz.Quesiti {
		q := models.Quesito{
			Difficolta: gq.Difficolta,
			Testo:      gq.Testo,
			OpCorretta: gq.OpCorretta,
		}
		// Copia le opzioni se presenti
		if len(gq.Opzioni) > 0 {
			q.OpzioneA = gq.Opzioni[0]
		}
		if len(gq.Opzioni) > 1 {
			q.OpzioneB = gq.Opzioni[1]
		}
		if len(gq.Opzioni) > 2 {
			q.OpzioneC = gq.Opzioni[2]
		}
		if len(gq.Opzioni) > 3 {
			q.OpzioneD = gq.Opzioni[3]
		}
		quiz.Quesiti = append(quiz.Quesiti, q)
	}

	return quiz, nil
}

// Conversione da GroqQuesito a Quesito
// -----------------------------------------------------------------------------
// La struct Quesito del nostro modello GORM prevede quattro campi separati per le opzioni
// di risposta (OpzioneA, OpzioneB, OpzioneC, OpzioneD), mentre il JSON generato
// dall'AI (Groq API) restituisce le opzioni come un array ("opzioni": [...]).
// Per questo motivo, dopo aver fatto l'unmarshal del JSON in una struct di appoggio
// (GroqQuiz/GroqQuesito), è necessario ciclare su ciascun quesito generato e
// copiare manualmente i valori dell'array "opzioni" nei rispettivi campi della struct
// Quesito. In questo modo si adatta la struttura dati generica e flessibile del JSON
// prodotto dall'AI al modello dati più rigido richiesto dalla nostra applicazione e
// dal database, garantendo la compatibilità e la corretta serializzazione verso il client.
//
