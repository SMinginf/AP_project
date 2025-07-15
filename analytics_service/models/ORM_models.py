from sqlalchemy import Column, Integer, String, Enum, DateTime, Time, ForeignKey
from sqlalchemy.orm import relationship
from database import Base

class Quiz(Base):
    __tablename__ = "quiz"
    id = Column(Integer, primary_key=True)
    id_utente = Column(Integer)
    difficolta = Column(Enum('Facile', 'Intermedia', 'Difficile', 'Qualsiasi'))
    quantita = Column(Integer)
    durata = Column(Time)
    data = Column(DateTime)
    risposte_corrette = Column(Integer)
    risposte_sbagliate = Column(Integer)

class QuizQuesiti(Base):
    __tablename__ = "quiz_quesiti"
    quiz_id = Column(Integer, primary_key=True)
    quesito_id = Column(Integer, primary_key=True)
    risposta_utente = Column(Integer)