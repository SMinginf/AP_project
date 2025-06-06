from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session
from passlib.context import CryptContext

from models import User
from schemas import UserCreate, UserOut, UserLogin
from database import get_db
import utils
# Questo file contiene gli endpoint REST per la registrazione
# e il login dell’utente, all’interno del microservizio auth_service.


router = APIRouter()

# per hashare e verificare le password in modo sicuro tramite l’algoritmo bcrypt
pwd_context = CryptContext(schemes=["bcrypt"], deprecated="auto")




# ---------------------
# Endpoint: Registrazione
# ---------------------


# Che cos’è Depends(get_db)?
# Depends è una funzione di FastAPI usata per dependency injection.
# Serve a dire: "per questo parametro, usa la funzione get_db() per fornire il valore".
# Quindi, FastAPI esegue get_db() per ottenere un oggetto db (cioè una sessione), lo passa a register_user.
# Grazie a get_db() che ha yield, FastAPI sa anche quando chiudere la sessione, dopo che la funzione ha finito.
# tramite Depends() FastAPI si occupa di chiamare la funzione generator get_db(), di gestire il yield, e di chiudere la sessione automaticamente.
# Se scrivessi db = get_db(), perderesti tutto questo meccanismo automatico.



@router.post("/register", response_model=UserOut) # -> ritorna un oggetto JSON che rispetta il modello UserOut.
def register_user(user: UserCreate, db: Session = Depends(get_db)):
    # Verifica se esiste già un utente con la stessa email o nickname
    if db.query(User).filter(User.email == user.email).first():
        raise HTTPException(status_code=400, detail="Email già registrata")

    if db.query(User).filter(User.username == user.username).first():
        raise HTTPException(status_code=400, detail="Username già in uso")

    hashed_password = pwd_context.hash(user.password)

    # Costruisco un oggetto SQLAlchemy da salvare nel DB
    new_user = User(
        username=user.username,
        email=user.email,
        nome=user.nome,
        cognome=user.cognome,
        data_nascita=user.data_nascita,
        genere=user.genere,
        password=hashed_password
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
    access_token = utils.create_access_token(data={"user_id": db_user.id})
    return {
        "access_token": access_token,
        "token_type": "bearer",
        "user_id": db_user.id
    }
