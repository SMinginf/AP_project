from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session
from models.ORM_models import Quiz, QuizQuesito, Quesito, CategoriaQuesito, Categoria, Utente
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
        QuizQuesito.quesito_id,
        QuizQuesito.risposta_utente,
        Quesito.difficolta,
        Quesito.op_corretta,
        Categoria.id.label("categoria_id"),
        Categoria.nome.label("categoria_nome"),
        Utente.username.label("docente_username")
    ).join(QuizQuesito, Quiz.id == QuizQuesito.quiz_id)\
     .join(Quesito, QuizQuesito.quesito_id == Quesito.id)\
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


# ----------------------------
# Endpoint: Statistiche per Docenti
# ----------------------------
@routes_group.get("/teacher/{teacher_id}", summary="Statistiche docente")
def get_teacher_stats(teacher_id: int, db: Session = Depends(get_db)):
    """Restituisce statistiche sui quesiti creati dal docente.

    Le informazioni sono calcolate aggregando le risposte che gli studenti
    hanno fornito nei quiz che includevano tali quesiti. I docenti non
    creano quiz, quindi l'analisi riguarda esclusivamente i quesiti.
    """

    # Verifica che il docente esista
    docente = db.query(Utente).filter(Utente.id == teacher_id).first()
    if not docente:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Docente con ID {teacher_id} non trovato.",
        )

    # Recupera tutte le risposte ai quesiti creati dal docente
    records = (
        db.query(
            Utente.username.label("studente_username"),  # Usa lo username dello studente
            Quiz.data.label("data_quiz"),
            QuizQuesito.quesito_id,
            QuizQuesito.risposta_utente,
            Quesito.difficolta,
            Quesito.op_corretta,
            Quesito.testo.label("quesito_testo"),
            Categoria.id.label("categoria_id"),
            Categoria.nome.label("categoria_nome"),
        )
        .join(Quiz, Quiz.id_utente == Utente.id)  # Join con la tabella Utente
        .join(QuizQuesito, Quiz.id == QuizQuesito.quiz_id)
        .join(Quesito, QuizQuesito.quesito_id == Quesito.id)
        .join(CategoriaQuesito, CategoriaQuesito.id_quesito == Quesito.id)
        .join(Categoria, Categoria.id == CategoriaQuesito.id_categoria)
        .filter(Quesito.id_docente == teacher_id)
        .all()
    )

    if not records:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Nessuna risposta trovata per i quesiti di questo docente.",
        )

    # Costruzione DataFrame dal risultato della query
    df = pd.DataFrame(records)


    # Calcolo indicatori binari per ogni risposta
    df["corretta"] = (df["risposta_utente"] == df["op_corretta"]).astype(int)
    df["sbagliata"] = (
        (df["risposta_utente"].notnull()) & (df["risposta_utente"] != df["op_corretta"])
    ).astype(int)
    df["non_data"] = df["risposta_utente"].isnull().astype(int)

    # Statistiche aggregate per difficoltà complessiva
    diff_grouped = (
        df.groupby("difficolta")
        .agg(
            corrette=("corretta", "sum"),
            sbagliate=("sbagliata", "sum"),
            non_date=("non_data", "sum"),
            tot_risposte=("corretta", "count"),
            quesiti_unici=("quesito_id", "nunique"),
        )
        .reset_index()
    )

    # Percentuali di correttezza per livello di difficoltà
    diff_grouped["perc_corrette"] = (diff_grouped["corrette"] / diff_grouped["tot_risposte"]).fillna(0).round(2)
    diff_grouped["perc_sbagliate"] = (diff_grouped["sbagliate"] / diff_grouped["tot_risposte"]).fillna(0).round(2)
    diff_grouped["perc_non_date"] = (diff_grouped["non_date"] / diff_grouped["tot_risposte"]).fillna(0).round(2)

    # Statistiche aggregate per categoria e difficoltà
    # studenti_unici rappresenta il numero di studenti distinti 
    # che hanno risposto ai quesiti di una determinata categoria e difficoltà.
    cat_grouped = (
        df.groupby(["categoria_nome", "difficolta"])
        .agg(
            corrette=("corrette", "sum"),
            sbagliate=("sbagliata", "sum"),
            non_date=("non_data", "sum"),
            tot_risposte=("corrette", "count"),
            studenti_unici=("studente_id", "nunique"),
        )
        .reset_index()
    )

    # Percentuali per categoria e difficoltà
    cat_grouped["perc_corrette"] = (cat_grouped["corrette"] / cat_grouped["tot_risposte"]).fillna(0).round(2)
    cat_grouped["perc_sbagliate"] = (cat_grouped["sbagliate"] / cat_grouped["tot_risposte"]).fillna(0).round(2)
    cat_grouped["perc_non_date"] = (cat_grouped["non_date"] / cat_grouped["tot_risposte"]).fillna(0).round(2)

    # Statistiche aggregate per singolo quesito
    quesiti_grouped = (
        df.groupby("quesito_id")
        .agg(
            corrette=("corretta", "sum"),
            sbagliate=("sbagliata", "sum"),
            non_date=("non_data", "sum"),
            tot_risposte=("corretta", "count"),
            studenti_unici=("studente_id", "nunique"),
        )
        .reset_index()
    )

    # Percentuali di esito per singola domanda
    quesiti_grouped["perc_corrette"] = (quesiti_grouped["corrette"] / quesiti_grouped["tot_risposte"]).fillna(0).round(2)
    quesiti_grouped["perc_sbagliate"] = (quesiti_grouped["sbagliate"] / quesiti_grouped["tot_risposte"]).fillna(0).round(2)
    quesiti_grouped["perc_non_date"] = (quesiti_grouped["non_date"] / quesiti_grouped["tot_risposte"]).fillna(0).round(2)

    # Statistiche per ogni studente che ha risposto ai quesiti del docente
    student_grouped = (
        df.groupby("studente_username")  # Raggruppa per username dello studente
        .agg(
            corrette=("corretta", "sum"),
            sbagliate=("sbagliata", "sum"),
            non_date=("non_data", "sum"),
            tot_risposte=("corretta", "count"),
        )
        .reset_index()
    )
    student_grouped["perc_corrette"] = (student_grouped["corrette"] / student_grouped["tot_risposte"]).fillna(0).round(2)
    student_grouped["perc_sbagliate"] = (student_grouped["sbagliate"] / student_grouped["tot_risposte"]).fillna(0).round(2)
    student_grouped["perc_non_date"] = (student_grouped["non_date"] / student_grouped["tot_risposte"]).fillna(0).round(2)

    return {
        "docente": {
            "id": docente.id,
            "username": docente.username,
            "nome": docente.nome,
            "cognome": docente.cognome,
        },
        "stats_per_categoria_difficolta": cat_grouped.to_dict(orient="records"),
        "stats_per_difficolta": diff_grouped.to_dict(orient="records"),
        "stats_per_domanda": quesiti_grouped.to_dict(orient="records"),
        "stats_per_studente": student_grouped.to_dict(orient="records"), 
    }
