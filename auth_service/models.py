from sqlalchemy import Column, Integer, String, Date
from enum import Enum
from sqlalchemy import Enum as SqlEnum
from sqlalchemy.ext.declarative import declarative_base


# In Python un Enum si definisce come una classe che eredita da Enum 
# (e opzionalmente da str se vuoi che i valori siano stringhe)
class RoleEnum(str, Enum):
    Studente = "Studente"
    Docente = "Docente"

# MODELLI ORM
# Rappresentano la struttura reale delle tabelle del db.
# Sono utilizzati per interagire direttamente con il database (lettura/scrittura/query).

# Base è la classe base da cui dovranno ereditare tutti i modelli (le classi che rappresentano le tabelle).
# Serve per usare SQLAlchemy ORM.
Base = declarative_base()

class User(Base):
    __tablename__ = "utenti"

    id = Column(Integer, primary_key=True, index=True)
    username = Column(String(50), unique=True, index=True)
    email = Column(String(100), unique=True, index=True)
    nome = Column(String(100))
    cognome = Column(String(100))
    password = Column(String(255))
    data_nascita = Column(Date)
    genere = Column(String(10))
    ruolo = Column(SqlEnum(RoleEnum), nullable=False, default=RoleEnum.Studente)