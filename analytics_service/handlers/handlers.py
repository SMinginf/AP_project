from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session
from models import User
from schemas import UserCreate, UserOut, UserLogin
from database import get_db
from utils import validate_token


# Una dependency globale in FastAPI è una funzione o un oggetto che viene 
# eseguito automaticamente per tutte le rotte di un router o dell'intera 
# applicazione. Questo permette di applicare logica comune (come validazione, 
# autenticazione, logging, ecc.) a tutte le richieste senza doverla specificare 
# manualmente per ogni endpoint.
routes_group = APIRouter(dependencies=[Depends(validate_token)])

# ----------------------------
# Endpoint: Registrazione
# ----------------------------

@routes_group.post("/student", response_model=UserOut) # -> ritorna un oggetto JSON che rispetta il modello UserOut.
def register_user(user: UserCreate, db: Session = Depends(get_db)):
   
    return

# ---------------------
# Endpoint: Login
# ---------------------
@routes_group.post("/login")
def login(user: UserLogin, db: Session = Depends(get_db)):
  return
