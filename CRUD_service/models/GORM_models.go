package models

import (
	"time"
)

// Strutture dati per il database, utilizzate da GORM per mappare le tabelle
// Queste strutture sono mappate alle tabelle del database MySQL

type Utente struct {
	ID          uint        `gorm:"primaryKey" json:"id"`
	Username    string      `gorm:"type:varchar(45);unique;not null" json:"username"`
	Email       string      `gorm:"type:varchar(45);unique;not null" json:"email"`
	Nome        string      `gorm:"type:varchar(45);not null" json:"nome"`
	Cognome     string      `gorm:"type:varchar(45);not null" json:"cognome"`
	Password    string      `gorm:"type:varchar(255);not null" json:"-"`
	DataNascita time.Time   `gorm:"type:date;not null" json:"data_nascita"`
	Genere      string      `gorm:"type:varchar(45);not null" json:"genere"`
	Ruolo       string      `gorm:"type:enum('studente','docente');default:'studente';not null" json:"ruolo"`
	Quesiti     []Quesito   `gorm:"foreignKey:IDDocente" json:"quesiti,omitempty"`
	Categorie   []Categoria `gorm:"foreignKey:IDDocente" json:"categorie,omitempty"`
}

// TableName specifica a GORM il nome della tabella del db mysql associata a questa struttura.
func (Utente) TableName() string {
	return "utenti"
}

type Quesito struct {
	ID         uint        `gorm:"primaryKey" json:"id"`
	Difficolta string      `gorm:"type:enum('Facile','Intermedia','Difficile','');default:null" json:"difficolta"`
	Testo      string      `gorm:"type:text;not null" json:"testo"`
	OpzioneA   string      `gorm:"column:opzione_a;type:varchar(255);not null" json:"opzione_a"`
	OpzioneB   string      `gorm:"column:opzione_b;type:varchar(255);not null" json:"opzione_b"`
	OpzioneC   string      `gorm:"column:opzione_c;type:varchar(255);not null" json:"opzione_c"`
	OpzioneD   string      `gorm:"column:opzione_d;type:varchar(255);not null" json:"opzione_d"`
	OpCorretta int         `gorm:"column:op_corretta;not null" json:"op_corretta"`
	IDDocente  uint        `gorm:"column:id_docente;not null" json:"id_docente"`
	Docente    Utente      `gorm:"foreignKey:IDDocente;constraint:OnUpdate:CASCADE,OnDelete:CASCADE" json:"docente"`
	Categorie  []Categoria `gorm:"many2many:categoria_quesito;joinForeignKey:id_quesito;joinReferences:id_categoria;" json:"categorie,omitempty"`
}

func (Quesito) TableName() string {
	return "quesiti"
}

type Categoria struct {
	ID          uint      `gorm:"primaryKey" json:"id"`
	Nome        string    `gorm:"type:varchar(50);not null" json:"nome"`
	Tipo        string    `gorm:"type:varchar(50);default:null" json:"tipo"`
	Descrizione string    `gorm:"type:text;not null" json:"descrizione"`
	IDDocente   uint      `gorm:"column:id_docente;not null" json:"id_docente"`
	Pubblica    bool      `gorm:"not null" json:"pubblica"`
	Docente     Utente    `gorm:"foreignKey:IDDocente;constraint:OnUpdate:CASCADE,OnDelete:CASCADE" json:"docente"`
	Quesiti     []Quesito `gorm:"many2many:categoria_quesito;joinForeignKey:id_categoria;joinReferences:id_quesito;" json:"quesiti,omitempty"`
}

func (Categoria) TableName() string {
	return "categorie"
}
