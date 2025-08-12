from sqlalchemy.orm import Session
from passlib.context import CryptContext
from models import User
from schemas import UserCreate, UserLogin
import utils

# Questo file contiene gli endpoint REST per la registrazione
# e il login dell’utente, all’interno del microservizio auth_service.
# [AGGIUNTO] Nota: nel refactoring teniamo questo commento qui per coerenza storica,
# ma gli "endpoint" ora sono nel controller; qui rimane la sola logica applicativa.

# per hashare e verificare le password in modo sicuro tramite l’algoritmo bcrypt
pwd_context = CryptContext(schemes=["bcrypt"], deprecated="auto")  # (commento originale)

# [AGGIUNTO] Eccezioni di dominio mappabili dal controller in HTTP status
class BadRequestError(Exception):
    pass

class NotFoundError(Exception):
    pass


# ----------------------------
# Service: Registrazione
# ----------------------------
def register_user_logic(user: UserCreate, db: Session) -> User:
    # Verifica se esiste già un utente con la stessa email o nickname (commenti originali preservati)
    if db.query(User).filter(User.email == user.email).first():
        raise BadRequestError("Email già registrata")

    if db.query(User).filter(User.username == user.username).first():
        raise BadRequestError("Username già in uso")

    if user.ruolo not in ["Studente", "Docente"]:
        raise BadRequestError("Ruolo non valido. Deve essere 'Studente' o 'Docente'.")

    # Hash della password
    # pwd_context è un oggetto di passlib che gestisce l'hashing delle password. (commento originale)
    hashed_password = pwd_context.hash(user.password)

    # Costruisco un oggetto SQLAlchemy da salvare nel DB (commento originale)
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


# ----------------------------
# Service: Login
# ----------------------------
def login_logic(user: UserLogin, db: Session) -> dict:
    db_user = db.query(User).filter(User.email == user.email).first()

    if not db_user:
        raise BadRequestError("Email non trovata")

    if not pwd_context.verify(user.password, db_user.password):
        raise BadRequestError("Password errata")

    # Genera e ritorna un token JWT (commento originale)
    access_token = utils.create_access_token(
        data={"user_id": db_user.id, "username": db_user.username, "ruolo": db_user.ruolo}
    )
    return {
        "access_token": access_token,
        "token_type": "bearer",
    }
