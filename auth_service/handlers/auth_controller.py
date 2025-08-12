from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session

from schemas import UserCreate, UserOut, UserLogin
from database import get_db
from logic import auth_service  # [AGGIUNTO] delega alla logica applicativa

# Questo file contiene gli endpoint REST per la registrazione
# e il login dell’utente, all’interno del microservizio auth_service. (commento originale)

# Che cos’è get_db())?  (commenti originali preservati)
# get_db() è una funzione che restituisce un generatore che gestisce la sessione del database.
# Quando FastAPI chiama register_user, passa automaticamente un oggetto db che è una sessione del database.
# Questo permette di eseguire query e operazioni sul database senza dover gestire manualmente la sessione.

# Depends è una funzione di FastAPI usata per dependency injection.
# Serve a dire: "per questo parametro, usa la funzione get_db() per fornire il valore".
# Quindi, FastAPI esegue get_db() per ottenere un oggetto db (cioè una sessione), lo passa a register_user.
# Grazie a get_db() che ha yield, FastAPI sa anche quando chiudere la sessione, dopo che la funzione ha finito.
# tramite Depends() FastAPI si occupa di chiamare la funzione generator get_db(), di gestire il yield, e di chiudere la sessione automaticamente.
# Se scrivessi db = get_db(), perderesti tutto questo meccanismo automatico.

# Crea un router FastAPI per raggruppare e organizzare gli endpoint relativi all'autenticazione (commento originale)
router = APIRouter()

# ----------------------------
# Endpoint: Registrazione
# ----------------------------
@router.post("/register", response_model=UserOut)  # -> ritorna un oggetto JSON che rispetta il modello UserOut. (commento originale)
def register_user(user: UserCreate, db: Session = Depends(get_db)):
    try:
        new_user = auth_service.register_user_logic(user, db)  # [AGGIUNTO] delega al service
        return new_user
    except auth_service.BadRequestError as e:  # [AGGIUNTO] mapping errori dominio -> HTTP
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=str(e))
    except auth_service.NotFoundError as e:  # [AGGIUNTO] (non usato qui ma pronto per estensioni)
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail=str(e))
    except Exception as e:  # [AGGIUNTO] fallback
        raise HTTPException(status_code=status.HTTP_500_INTERNAL_SERVER_ERROR, detail=str(e))

# ---------------------
# Endpoint: Login
# ---------------------
@router.post("/login")
def login(user: UserLogin, db: Session = Depends(get_db)):
    try:
        return auth_service.login_logic(user, db)  # [AGGIUNTO] delega al service
    except auth_service.BadRequestError as e:  # [AGGIUNTO]
        # In origine usavi 400 sia per email non trovata che per password errata; manteniamo lo stesso comportamento
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=str(e))
    except Exception as e:  # [AGGIUNTO]
        raise HTTPException(status_code=status.HTTP_500_INTERNAL_SERVER_ERROR, detail=str(e))
