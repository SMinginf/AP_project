package database

import (
	"fmt"
	"os"

	"gorm.io/driver/mysql"
	"gorm.io/gorm"
)

var DB *gorm.DB

func init() {
	// Recupera le variabili d'ambiente (puoi settarle in Docker o nel sistema)
	host := os.Getenv("DB_HOST")
	if host == "" {
		host = "localhost"
	}
	user := os.Getenv("DB_USER")
	if user == "" {
		user = "my_user"
	}
	password := os.Getenv("DB_PASSWORD")
	if password == "" {
		password = "my_pass"
	}
	database := os.Getenv("DB_NAME")
	if database == "" {
		database = "ap_quiz"
	}

	// Costruisci la DSN (Data Source Name)
	dsn := fmt.Sprintf("%s:%s@tcp(%s:3306)/%s?charset=utf8mb4&parseTime=True&loc=Local",
		user, password, host, database)

	// Connessione al database con GORM
	db, err := gorm.Open(mysql.Open(dsn), &gorm.Config{})
	if err != nil {
		panic("Impossibile connettersi al database: " + err.Error())
	}

	DB = db
}
