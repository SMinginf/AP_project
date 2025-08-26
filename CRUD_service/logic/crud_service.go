package logic

import (
	"AP_project/CRUD_service/database"
	"AP_project/CRUD_service/models"
	"errors"
	"fmt"
	"strings"
)

// --- FUNZIONI CRUD PER LE CATEGORIE ---
var (
	ErrBadRequest = errors.New("bad request")
	ErrNotFound   = errors.New("not found")
)

func GetCategoriePubbliche() ([]models.Categoria, error) {
	var categorie []models.Categoria
	if err := database.DB.Preload("Docente").Where("pubblica = ?", true).Find(&categorie).Error; err != nil {
		fmt.Println("Errore nel recupero delle categorie:", err.Error())
		return nil, fmt.Errorf("recupero categorie pubbliche: %w", err)
	}
	return categorie, nil
}

func GetCategoriePubblicheByDocente(idDocente string) ([]models.Categoria, error) {
	// [AGGIORNATO] rimosso controllo su idDocente vuoto (ora verificato nel controller)
	var categorie []models.Categoria
	if err := database.DB.Where("id_docente = ? AND pubblica = ?", idDocente, true).Find(&categorie).Error; err != nil {
		fmt.Println("Errore nel recupero delle categorie pubbliche per il docente:", err.Error())
		return nil, fmt.Errorf("recupero categorie pubbliche per docente: %w", err)
	}
	return categorie, nil
}

func CreateCategoria(categoria models.Categoria) (models.Categoria, error) {
	if err := database.DB.Create(&categoria).Error; err != nil {
		if strings.Contains(err.Error(), "1062") {
			return models.Categoria{}, wrap(ErrBadRequest, "Categoria con stesso nome e visibilità già esistente")
		}
		return models.Categoria{}, fmt.Errorf("creazione categoria: %w", err)
	}
	return categoria, nil
}

func UpdateCategoria(categoria models.Categoria) error {
	/*
		GORM per default ignora i campi con valore zero (incluso false per i boolean)
		quando usa il metodo Updates(). Questo è un comportamento standard di GORM per
		evitare di sovrascrivere valori esistenti con valori zero non intenzionali.
		Usando il select possiamo esplicitamente specificare quali campi vogliamo aggiornare.
	*/
	// [AGGIORNATO] rimosso controllo su categoria.ID == 0 (validato nel controller)
	if err := database.DB.Model(&models.Categoria{}).
		Where("id = ?", categoria.ID).
		Select("nome", "tipo", "descrizione", "pubblica", "id_docente").
		Updates(categoria).Error; err != nil {
		return fmt.Errorf("modifica categoria: %w", err)
	}
	return nil
}

func DeleteCategorie(ids []uint) error {
	// [AGGIORNATO] rimosso controllo su len(ids)==0 (validato nel controller)
	if err := database.DB.Delete(&models.Categoria{}, ids).Error; err != nil {
		return fmt.Errorf("cancellazione categorie: %w", err)
	}
	return nil
}

func GetCategorieByDocente(idDocente string) ([]models.Categoria, error) {
	// [AGGIORNATO] rimosso controllo su idDocente vuoto (validato nel controller)
	var categorie []models.Categoria
	if err := database.DB.Where("id_docente = ?", idDocente).Preload("Docente").Find(&categorie).Error; err != nil {
		fmt.Println("Errore nel recupero delle categorie per il docente:", err.Error())
		return nil, fmt.Errorf("recupero categorie per docente: %w", err)
	}
	return categorie, nil
}

func GetCategorieByStudente(idStudente string) ([]models.Categoria, error) {
	// [AGGIORNATO] rimosso controllo su idStudente vuoto (validato nel controller)
	var categorie []models.Categoria

	err := database.DB.
		Table("categorie").
		Select("DISTINCT categorie.*").
		Joins("JOIN categoria_quesito ON categorie.id = categoria_quesito.id_categoria").
		Joins("JOIN quesiti ON categoria_quesito.id_quesito = quesiti.id").
		Joins("JOIN quiz_quesiti ON quesiti.id = quiz_quesiti.quesito_id").
		Joins("JOIN quiz ON quiz_quesiti.quiz_id = quiz.id").
		Where("quiz.id_utente = ?", idStudente).
		Preload("Docente").
		Find(&categorie).Error

	if err != nil {
		fmt.Println("Errore nel recupero delle categorie per lo studente:", err.Error())
		return nil, fmt.Errorf("recupero categorie per studente: %w", err)
	}

	return categorie, nil
}

// --- FUNZIONI CRUD PER I QUESITI ---

func CreateQuesito(quesito models.Quesito) (models.Quesito, error) {
	if err := database.DB.Create(&quesito).Error; err != nil {
		return models.Quesito{}, fmt.Errorf("creazione quesito: %w", err)
	}
	return quesito, nil
}

func UpdateQuesito(quesito models.Quesito) error {
	// [AGGIORNATO] rimosso controllo su quesito.ID == 0 (validato nel controller)
	if err := database.DB.Model(&models.Quesito{}).Where("id = ?", quesito.ID).Updates(quesito).Error; err != nil {
		return fmt.Errorf("modifica quesito: %w", err)
	}
	return nil
}

func DeleteQuesito(id string) error {
	// [AGGIORNATO] rimosso controllo su id vuoto (validato nel controller)
	if err := database.DB.Delete(&models.Quesito{}, id).Error; err != nil {
		return fmt.Errorf("cancellazione quesito: %w", err)
	}
	return nil
}

func GetQuesitiByDocente(idDocente string) ([]models.Quesito, error) {
	// [AGGIORNATO] rimosso controllo su idDocente vuoto (validato nel controller)
	var quesiti []models.Quesito
	if err := database.DB.Where("id_docente = ?", idDocente).Preload("Docente").Preload("Categorie").Find(&quesiti).Error; err != nil {
		fmt.Println("Errore nel recupero dei quesiti per il docente:", err.Error())
		return nil, fmt.Errorf("recupero quesiti per docente: %w", err)
	}
	return quesiti, nil
}

func GetQuesitiByDocenteAndCategoria(idDocente, categoria string) ([]models.Categoria, error) {
	// [AGGIORNATO] rimosso controllo su parametri vuoti (validati nel controller)
	var categorie []models.Categoria
	if err := database.DB.Where("id_docente = ? AND nome = ?", idDocente, categoria).Preload("Docente").Find(&categorie).Error; err != nil {
		fmt.Println("Errore nel recupero delle categorie per il docente e la categoria:", err.Error())
		return nil, fmt.Errorf("recupero categorie per docente e categoria: %w", err)
	}
	return categorie, nil
}

// Funzioni CRUD per gli utenti
func GetUtente(id string) (models.Utente, error) {
	// [AGGIORNATO] rimosso controllo su id vuoto (validato nel controller)
	var utente models.Utente
	if err := database.DB.First(&utente, id).Error; err != nil {
		return models.Utente{}, wrap(ErrNotFound, "Utente non trovato")
	}
	return utente, nil
}

func UpdateUtente(utente models.Utente) error {
	// [AGGIORNATO] rimosso controllo su id vuoto (validato nel controller)
	if err := database.DB.Model(&models.Utente{}).Where("id = ?", utente.ID).Updates(utente).Error; err != nil {
		return fmt.Errorf("modifica utente: %w", err)
	}
	return nil
}

// helper
func wrap(sentinel error, msg string) error {
	return fmt.Errorf("%w: %s", sentinel, msg)
}
