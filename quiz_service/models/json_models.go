package models

// Oggetto in cui farò il parse del JSON inviato dal client per creare un quiz.
type CreateQuizInput struct {
	AIGenerated     bool   `json:"ai_generated"` // il tag Gin "required" per i booelani non funziona bene
	AICategoria     string `json:"ai_categoria"` // categoria da cui generare il quiz con AI
	IdCategorie     []uint `json:"id_categorie" binding:"required"`
	Difficolta      string `json:"difficolta" binding:"required,oneof=Facile Intermedia Difficile Qualsiasi"`
	Quantita        int    `json:"quantita" binding:"required,min=3"`
	UsernameDocente string `json:"username_docente"` // uso l'username per riconoscere univocamente un docente. Non uso il tag `binding:"required"` perché voglio permettere che possa essere una stringa vuota
	Unione          bool   `json:"unione"`           // true = unione, false = intersezione
}

// Oggetto da cui farò il parse a json per restituire un quiz generato manualmente,
// ovvero con quesiti prelevati dal database e non generati da AI.
type CreateQuizOutput struct {
	AIGenerated bool      `json:"ai_generated"` // il tag Gin "required" per i booelani non funziona bene
	AICategoria string    `json:"ai_categoria"` // categoria da cui generare il quiz con AI
	Difficolta  string    `json:"difficolta"`
	Quantita    int       `json:"quantita"`
	Quesiti     []Quesito `json:"quesiti"`
}

type GroqQuesito struct {
	Difficolta string   `json:"difficolta"`
	Testo      string   `json:"testo"`
	Opzioni    []string `json:"opzioni"`
	OpCorretta int      `json:"op_corretta"`
}

type GroqQuiz struct {
	Titolo     string        `json:"titolo"`
	Categoria  string        `json:"categoria"`
	Difficolta string        `json:"difficolta"`
	Quesiti    []GroqQuesito `json:"quesiti"`
}

type StoreQuizInput struct {
	Difficolta        string `json:"difficolta" binding:"required,oneof=Facile Intermedia Difficile Qualsiasi"`
	Quantita          int    `json:"quantita" binding:"required,min=3"`
	IdUtente          uint   `json:"id_utente" binding:"required"`
	IdQuesiti         []uint `json:"id_quesiti" binding:"required"`
	RisposteCorrette  int    `json:"corrette" binding:"gte=0"` // binding:"required" da solo non accetta 0 come valore valido per un campo numerico, gte=0 (greater than or equal to 0) lo fa esplicitamente
	RisposteSbagliate int    `json:"sbagliate" binding:"gte=0"`
	Durata            string `json:"durata" binding:"required"`
	DataCreazione     string `json:"data_creazione" binding:"required"`  // formato: "YYYY-MM-DD HH:MM:SS"
	Risposte          []*int `json:"risposte_utente" binding:"required"` // può essere nil se l'utente non ha risposto
}
