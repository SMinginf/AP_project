from pydantic import BaseModel, EmailStr
from datetime import date

# Questo file definisce come devono essere strutturati i dati che entrano ed escono dall'API, 
# sfruttando le classi di Pydantic, una libreria usata da FastAPI per validare automaticamente i dati.
# Controllano, validano e definiscono la forma dei dati che: il client può inviare (es. richiesta di registrazione)
# e l’API può restituire (es. profilo utente).

# Schema per registrazione
class UserCreate(BaseModel):
    ruolo: str
    username: str
    email: EmailStr
    nome: str
    cognome: str
    data_nascita: date
    genere: str
    password: str
<<<<<<< HEAD
    ruolo: str
=======
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba

# Schema per login
class UserLogin(BaseModel):
    email: EmailStr
    password: str

# Schema per risposte API (output)
class UserOut(BaseModel):
    id: int
    email: str
    username: str
    nome: str
    cognome: str
    data_nascita: date
    genere: str

    class Config:
        orm_mode = True
