from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session
from models.ORM_models import Quiz, QuizQuesiti, Quesito, CategoriaQuesito, Categoria, Utente
from database.connect import get_db
from utils.auth import validate_token
import pandas as pd



# Una dependency globale in FastAPI è una funzione o un oggetto che viene 
# eseguito automaticamente per tutte le rotte di un router o dell'intera 
# applicazione. Questo permette di applicare logica comune (come validazione, 
# autenticazione, logging, ecc.) a tutte le richieste senza doverla specificare 
# manualmente per ogni endpoint.
routes_group = APIRouter(dependencies=[Depends(validate_token)])

# ----------------------------
# Endpoint: Statistiche per Studenti
# ----------------------------
@routes_group.get("/student/{student_id}", summary="Statistiche studente")
def get_student_stats(student_id: int, db: Session = Depends(get_db)):
    # Verifica che lo studente esista
    studente = db.query(Utente).filter(Utente.id == student_id).first()
    if not studente:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Studente con ID {student_id} non trovato."
        )

    # Carica dati dei quiz e quesiti associati
    records = db.query(
        Quiz.id.label("quiz_id"),
        Quiz.data.label("data_quiz"),
        QuizQuesiti.quesito_id,
        QuizQuesiti.risposta_utente,
        Quesito.difficolta,
        Quesito.op_corretta,
        Categoria.id.label("categoria_id"),
        Categoria.nome.label("categoria_nome"),
        Utente.username.label("docente_username")
    ).join(QuizQuesiti, Quiz.id == QuizQuesiti.quiz_id)\
     .join(Quesito, QuizQuesiti.quesito_id == Quesito.id)\
     .join(CategoriaQuesito, CategoriaQuesito.id_quesito == Quesito.id)\
     .join(Categoria, Categoria.id == CategoriaQuesito.id_categoria)\
     .join(Utente, Categoria.id_docente == Utente.id)\
     .filter(Quiz.id_utente == student_id)\
     .all()

    if not records:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Nessun quiz trovato per questo studente."
        )

    # Converti in DataFrame
    df = pd.DataFrame(records)

    # Etichetta categoria con nome docente
    df["categoria_label"] = df["categoria_nome"] + " (" + df["docente_username"] + ")"

    # Calcola indicatori
    df["corretta"] = (df["risposta_utente"] == df["op_corretta"]).astype(int)
    df["sbagliata"] = ((df["risposta_utente"].notnull()) & (df["risposta_utente"] != df["op_corretta"])).astype(int)
    df["non_data"] = df["risposta_utente"].isnull().astype(int)

    # Statistiche aggregate per categoria e difficoltà
    grouped = df.groupby(["categoria_label", "difficolta"]).agg(
        corrette=("corretta", "sum"),
        sbagliate=("sbagliata", "sum"),
        non_date=("non_data", "sum"),
        tot_quesiti=("corretta", "count")
    ).reset_index()

    stats = grouped.to_dict(orient="records")

    # Andamento temporale per categoria e difficoltà
    andamento = df.groupby(["data_quiz", "categoria_label", "difficolta"]).agg(
        corrette=("corretta", "sum"),
        sbagliate=("sbagliata", "sum"),
        non_date=("non_data", "sum")
    ).reset_index()

    timeline = andamento.to_dict(orient="records")

    return {
        "studente": {
            "id": studente.id,
            "username": studente.username,
            "nome": studente.nome,
            "cognome": studente.cognome,
        },
        "stats_per_categoria_difficolta": stats,
        "andamento_temporale": timeline
    }
