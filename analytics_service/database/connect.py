import os
from sqlalchemy import create_engine
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker


# Settaggio connessione al database MySQL. os.getenv() mi permette di ottenere 
# il valore di quelle variabili d'ambiente definite nel docker-compose.yml

DB_CONFIG = {
    "host" : os.getenv("DB_HOST", "localhost"), 
    "user" : os.getenv("DB_USER", "my_user"),
    "password" : os.getenv("DB_PASSWORD", "my_pass"),
    "database" : os.getenv("DB_NAME", "ap_quiz")
        }



DATABASE_URL = "mysql+pymysql://"+DB_CONFIG["user"]+":"+ DB_CONFIG["password"]+"@"+DB_CONFIG["host"]+":3306/"+DB_CONFIG["database"]

engine = create_engine(DATABASE_URL)

# SessionLocal è un factory function creata da sessionmaker(), che quando viene chiamata crea un nuovo oggetto sessione collegato al database.
# La sessione è l’oggetto tramite cui interagisci con il database per eseguire query ecc.
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)

# Base è la classe base da cui dovranno ereditare tutti i modelli (le classi che rappresentano le tabelle).
# Serve per usare SQLAlchemy ORM.
Base = declarative_base()


# Funzione generatore
# Una funzione generatore è una funzione speciale che al posto di restituire un valore con return 
# ne produce una sequenza di valori, uno alla volta, usando la parola chiave yield.
# In pratica: 
# 1) La funzione si “sospende” al yield db, dando la sessione attiva all’esterno.
# 2) Quando chi ha ricevuto la sessione ha finito (per esempio, la richiesta web in FastAPI è completata), la funzione “riprende” e arriva al finally.
# 3) Nel finally chiude la sessione (pulizia).
# Che vantaggi dà?
# - Gestione automatica e sicura della risorsa sessione.
# - Nessun rischio di dimenticare di chiudere la sessione.
# - Funziona perfettamente con FastAPI e il suo sistema di dependency injection.
# - Codice molto più pulito e robusto.


def get_db():
    db = SessionLocal()     # Creo una nuova sessione collegata al DB
    try:
        yield db            # Yield rende la sessione disponibile a chi la chiama 
    finally:
        db.close()
