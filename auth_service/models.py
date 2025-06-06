
from sqlalchemy import Column, Integer, String, Date
from database import Base

# MODELLI ORM
# Rappresentano la struttura reale delle tabelle del db.
# Sono utilizzati per interagire direttamente con il database (lettura/scrittura/query).

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
