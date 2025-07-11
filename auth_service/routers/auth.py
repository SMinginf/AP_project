from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session
from passlib.context import CryptContext

from models import User
from schemas import UserCreate, UserOut, UserLogin
from database import get_db
import utils
# Questo file contiene gli endpoint REST per la registrazione
# e il login dell’utente, all’interno del microservizio auth_service.

 


# per hashare e verificare le password in modo sicuro tramite l’algoritmo bcrypt
pwd_context = CryptContext(schemes=["bcrypt"], deprecated="auto")


# Che cos’è get_db())? 
# get_db() è una funzione che restituisce un generatore che gestisce la sessione del database.
# Quando FastAPI chiama register_user, passa automaticamente un oggetto db che è una sessione del database.
# Questo permette di eseguire query e operazioni sul database senza dover gestire manualmente la sessione.

# Depends è una funzione di FastAPI usata per dependency injection.
# Serve a dire: "per questo parametro, usa la funzione get_db() per fornire il valore".
# Quindi, FastAPI esegue get_db() per ottenere un oggetto db (cioè una sessione), lo passa a register_user.
# Grazie a get_db() che ha yield, FastAPI sa anche quando chiudere la sessione, dopo che la funzione ha finito.
# tramite Depends() FastAPI si occupa di chiamare la funzione generator get_db(), di gestire il yield, e di chiudere la sessione automaticamente.
# Se scrivessi db = get_db(), perderesti tutto questo meccanismo automatico.

# Crea un router FastAPI per raggruppare e organizzare gli endpoint relativi all'autenticazione
router = APIRouter()

# ----------------------------
# Endpoint: Registrazione
# ----------------------------

@router.post("/register", response_model=UserOut) # -> ritorna un oggetto JSON che rispetta il modello UserOut.
def register_user(user: UserCreate, db: Session = Depends(get_db)):
    # Verifica se esiste già un utente con la stessa email o nickname
    if db.query(User).filter(User.email == user.email).first():
        raise HTTPException(status_code=400, detail="Email già registrata")

    if db.query(User).filter(User.username == user.username).first():
        raise HTTPException(status_code=400, detail="Username già in uso")
    
    if user.ruolo not in ["Studente", "Docente"]:
        raise HTTPException(status_code=400, detail="Ruolo non valido. Deve essere 'Studente' o 'Docente'.")
    
    # Hash della password
    # pwd_context è un oggetto di passlib che gestisce l'hashing delle password.
    hashed_password = pwd_context.hash(user.password)

    # Costruisco un oggetto SQLAlchemy da salvare nel DB
    new_user = User(
        ruolo=user.ruolo,
        username=user.username,
        email=user.email,
        nome=user.nome,
        cognome=user.cognome,
        data_nascita=user.data_nascita,
        genere=user.genere,
        password=hashed_password,
    )

    db.add(new_user)
    db.commit()
    db.refresh(new_user)

    return new_user

# ---------------------
# Endpoint: Login
# ---------------------
@router.post("/login")
def login(user: UserLogin, db: Session = Depends(get_db)):
    db_user = db.query(User).filter(User.email == user.email).first()
    
    if not db_user:
        raise HTTPException(status_code=400, detail="Email non trovata")

    if not pwd_context.verify(user.password, db_user.password):
        raise HTTPException(status_code=400, detail="Password errata")

    # Genera e ritorna un token JWT
    access_token = utils.create_access_token(data={"user_id": db_user.id, "username": db_user.username, "ruolo": db_user.ruolo} )
    return {
        "access_token": access_token,
        "token_type": "bearer",
    }
