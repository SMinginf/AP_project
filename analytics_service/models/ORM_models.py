from sqlalchemy import Column, Integer, String, Boolean, ForeignKey, Enum, Text, Date, DateTime, Time
from sqlalchemy.orm import relationship, declarative_base
import enum

Base = declarative_base()

# Enum di difficoltà
class DifficoltaEnum(enum.Enum):
    Facile = "Facile"
    Intermedia = "Intermedia"
    Difficile = "Difficile"
    Qualsiasi = "Qualsiasi"
    Nessuna = ""  # solo per compatibilità


class RuoloEnum(enum.Enum):
    Studente = "Studente"
    Docente = "Docente"


# Modelli SQLAlchemy
class Utente(Base):
    __tablename__ = "utenti"

    id = Column(Integer, primary_key=True)
    username = Column(String(45), unique=True, nullable=False)
    email = Column(String(45), unique=True, nullable=False)
    nome = Column(String(45), nullable=False)
    cognome = Column(String(45), nullable=False)
    password = Column(String(255), nullable=False)
    data_nascita = Column(Date, nullable=False)
    genere = Column(String(45), nullable=False)
    
    # Usa String per memorizzare i valori dell'Enum come stringa
    ruolo = Column(String(20), default=RuoloEnum.Studente.value, nullable=False)

    quiz_svolti = relationship("Quiz", back_populates="studente")
    categorie_create = relationship("Categoria", back_populates="docente")
    quesiti_creati = relationship("Quesito", back_populates="docente")


class Categoria(Base):
    __tablename__ = "categorie"

    id = Column(Integer, primary_key=True)
    nome = Column(String(50), nullable=False)
    tipo = Column(String(50))
    descrizione = Column(Text, nullable=False)
    id_docente = Column(Integer, ForeignKey("utenti.id"), nullable=False)
    pubblica = Column(Boolean, nullable=False)

    docente = relationship("Utente", back_populates="categorie_create")
    quesiti = relationship("CategoriaQuesito", back_populates="categoria", cascade="all, delete-orphan")


class Quesito(Base):
    __tablename__ = "quesiti"

    id = Column(Integer, primary_key=True)
    # Usa String per memorizzare i valori dell'Enum come stringa
    difficolta = Column(String(20), nullable=True)  # Usa una stringa per la difficoltà
    testo = Column(Text, nullable=False)
    opzione_a = Column(String(255), nullable=False)
    opzione_b = Column(String(255), nullable=False)
    opzione_c = Column(String(255), nullable=False)
    opzione_d = Column(String(255), nullable=False)
    op_corretta = Column(Integer, nullable=False)
    id_docente = Column(Integer, ForeignKey("utenti.id"), nullable=False)

    docente = relationship("Utente", back_populates="quesiti_creati")
    categorie = relationship("CategoriaQuesito", back_populates="quesito", cascade="all, delete-orphan")
    risposte_date = relationship("QuizQuesito", back_populates="quesito")


class CategoriaQuesito(Base):
    __tablename__ = "categoria_quesito"

    id_categoria = Column(Integer, ForeignKey("categorie.id"), primary_key=True)
    id_quesito = Column(Integer, ForeignKey("quesiti.id"), primary_key=True)

    categoria = relationship("Categoria", back_populates="quesiti")
    quesito = relationship("Quesito", back_populates="categorie")


class Quiz(Base):
    __tablename__ = "quiz"

    id = Column(Integer, primary_key=True)
    id_utente = Column(Integer, ForeignKey("utenti.id"), nullable=False)
    # Usa String per memorizzare i valori dell'Enum come stringa
    difficolta = Column(String(20), default=DifficoltaEnum.Qualsiasi.value, nullable=False)
    quantita = Column(Integer, nullable=False)
    durata = Column(Time, nullable=False)
    data = Column(DateTime, nullable=False)
    risposte_corrette = Column(Integer, nullable=False)
    risposte_sbagliate = Column(Integer, nullable=False)

    studente = relationship("Utente", back_populates="quiz_svolti")
    domande = relationship("QuizQuesito", back_populates="quiz", cascade="all, delete-orphan")


class QuizQuesito(Base):
    __tablename__ = "quiz_quesiti"

    quiz_id = Column(Integer, ForeignKey("quiz.id"), primary_key=True)
    quesito_id = Column(Integer, ForeignKey("quesiti.id"), primary_key=True)
    risposta_utente = Column(Integer, nullable=True)  # può essere NULL

    quiz = relationship("Quiz", back_populates="domande")
    quesito = relationship("Quesito", back_populates="risposte_date")
