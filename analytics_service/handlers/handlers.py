from fastapi import APIRouter, Depends, HTTPException, status, Body
from pydantic import BaseModel
from typing import List, Optional
from sqlalchemy.orm import Session
from sqlalchemy import func, and_
from models.ORM_models import Quiz, QuizQuesito, Quesito, CategoriaQuesito, Categoria, Utente
from database.connect import get_db
from utils.auth import validate_token
import pandas as pd
import math

# Una dependency globale in FastAPI è una funzione o un oggetto che viene 
# eseguito automaticamente per tutte le rotte di un router o dell'intera 
# applicazione. Questo permette di applicare logica comune (come validazione, 
# autenticazione, logging, ecc.) a tutte le richieste senza doverla specificare 
# manualmente per ogni endpoint.
routes_group = APIRouter(dependencies=[Depends(validate_token)])


# Questo punteggio combina accuratezza e confidenza statistica.
# La formula: ((corrette - errate) / risposte_date) * sqrt(risposte_date)
# bilancia la qualità delle risposte (precisione netta) con la quantità (radice del volume),
# penalizzando risposte errate e riducendo l'effetto di performance elevate su campioni poco significativi.
def calcola_punteggio(corrette, errate):
    risposte_date = corrette + errate
    if risposte_date == 0:
        return 0  # niente da valutare
    precisione = (corrette - errate) / risposte_date
    punteggio = precisione * math.sqrt(risposte_date)
    return punteggio


# ----------------------------
# Endpoint: Statistiche generali per Studenti
# ----------------------------
@routes_group.get("/student/{student_id}/general", summary="Statistiche generali dello studente")
def get_student_general_stats(student_id: int, db: Session = Depends(get_db)):
    """
    Restituisce le statistiche generali sulle prestazioni di uno studente.
    
    Include:
    - Percentuali complessive (corrette/sbagliate/non date)
    - Punteggi per categoria
    - Categoria più forte/debole
    - Numero di quiz completati
    """
    
    # Verifica che lo studente esista
    studente = db.query(Utente).filter(Utente.id == student_id).first()
    if not studente:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Studente con ID {student_id} non trovato."
        )

    # Recupera tutti i dati dei quiz dello studente
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

    # Calcola indicatori
    df["corretta"] = (df["risposta_utente"] == df["op_corretta"]).astype(int)
    df["sbagliata"] = ((df["risposta_utente"].notnull()) & (df["risposta_utente"] != df["op_corretta"])).astype(int)
    df["non_data"] = df["risposta_utente"].isnull().astype(int)

    # 1. STATISTICHE GENERALI COMPLESSIVE
    totale_quesiti = len(df)
    totale_corrette = int(df["corretta"].sum())
    totale_sbagliate = int(df["sbagliata"].sum())
    totale_non_date = int(df["non_data"].sum())

    perc_corrette_generali = round((totale_corrette / totale_quesiti) * 100, 2) if totale_quesiti > 0 else 0.0
    perc_sbagliate_generali = round((totale_sbagliate / totale_quesiti) * 100, 2) if totale_quesiti > 0 else 0.0
    perc_non_date_generali = round((totale_non_date / totale_quesiti) * 100, 2) if totale_quesiti > 0 else 0.0

    # 2. PUNTEGGI PER CATEGORIA
    categoria_stats = df.groupby(["categoria_id", "categoria_nome", "docente_username"]).agg(
        corrette=("corretta", "sum"),
        sbagliate=("sbagliata", "sum"),
        non_date=("non_data", "sum"),
        totale_quesiti=("corretta", "count")
    ).reset_index()

    # Calcola percentuali per categoria
    categoria_stats["perc_corrette"] = (categoria_stats["corrette"] / categoria_stats["totale_quesiti"] * 100).round(2)
    categoria_stats["perc_sbagliate"] = (categoria_stats["sbagliate"] / categoria_stats["totale_quesiti"] * 100).round(2)
    categoria_stats["perc_non_date"] = (categoria_stats["non_date"] / categoria_stats["totale_quesiti"] * 100).round(2)

    # Converti in lista di dizionari
    punteggi_per_categoria = []
    for _, row in categoria_stats.iterrows():
        corrette = int(row["corrette"])
        sbagliate = int(row["sbagliate"])
        
        # Calcola il punteggio usando la funzione esistente
        punteggio_calcolato = calcola_punteggio(corrette, sbagliate)
        
        punteggi_per_categoria.append({
            "categoria_id": int(row["categoria_id"]),
            "categoria_nome": row["categoria_nome"],
            "docente_username": row["docente_username"],
            "corrette": corrette,
            "sbagliate": sbagliate,
            "non_date": int(row["non_date"]),
            "totale_quesiti": int(row["totale_quesiti"]),
            "perc_corrette": float(row["perc_corrette"]),
            "perc_sbagliate": float(row["perc_sbagliate"]),
            "perc_non_date": float(row["perc_non_date"]),
            "punteggio": round(punteggio_calcolato, 3)  # Aggiungi il punteggio calcolato
        })

    # 3. CATEGORIA PIÙ FORTE E PIÙ DEBOLE
    categoria_piu_forte = None
    categoria_piu_debole = None

    if len(punteggi_per_categoria) > 0:
        # Ordina la lista dei punteggi per categoria in base al punteggio già calcolato
        punteggi_ordinati = sorted(punteggi_per_categoria, key=lambda x: x["punteggio"], reverse=True)
        
        # La migliore è la prima (punteggio più alto)
        migliore = punteggi_ordinati[0]
        categoria_piu_forte = {
            "categoria_id": migliore["categoria_id"],
            "categoria_nome": migliore["categoria_nome"],
            "docente_username": migliore["docente_username"],
            "perc_corrette": migliore["perc_corrette"],
            "punteggio": migliore["punteggio"],
            "totale_quesiti": migliore["totale_quesiti"]
        }
        
        # La peggiore è l'ultima (punteggio più basso)
        peggiore = punteggi_ordinati[-1]
        categoria_piu_debole = {
            "categoria_id": peggiore["categoria_id"],
            "categoria_nome": peggiore["categoria_nome"],
            "docente_username": peggiore["docente_username"],
            "perc_corrette": peggiore["perc_corrette"],
            "punteggio": peggiore["punteggio"],
            "totale_quesiti": peggiore["totale_quesiti"]
        }

    # 4. NUMERO DI QUIZ COMPLETATI
    quiz_completati = int(df["quiz_id"].nunique())

    return {
        "statistiche_generali": {
            "totale_quesiti": totale_quesiti,
            "totale_corrette": totale_corrette,
            "totale_sbagliate": totale_sbagliate,
            "totale_non_date": totale_non_date,
            "perc_corrette": float(perc_corrette_generali),
            "perc_sbagliate": float(perc_sbagliate_generali),
            "perc_non_date": float(perc_non_date_generali)
        },
        "quiz_completati": quiz_completati,
        "punteggi_per_categoria": punteggi_per_categoria,
        "categoria_piu_forte": categoria_piu_forte,
        "categoria_piu_debole": categoria_piu_debole
    }



# ----------------------------
# Endpoint: Statistiche per Studenti per categorie selezionate
# ----------------------------
@routes_group.get("/student/{student_id}", summary="Statistiche studente per categorie specifiche")
def get_student_stats_per_categorie(
    student_id: int, 
    categoria_ids: str,
    db: Session = Depends(get_db)
):
    # Converti la stringa in lista di interi
    try:
        categoria_ids_list = [int(id.strip()) for id in categoria_ids.split(",") if id.strip()]
        if not categoria_ids_list:
            raise ValueError("Lista categorie vuota")
    except ValueError:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Formato categoria_ids non valido. Usa il formato: '1,2,3'"
        )
    
    # Verifica che lo studente esista
    studente = db.query(Utente).filter(Utente.id == student_id).first()
    if not studente:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Studente con ID {student_id} non trovato."
        )

    # Verifica che le categorie esistano
    categorie_esistenti = db.query(Categoria.id).filter(Categoria.id.in_(categoria_ids_list)).all()
    categorie_esistenti_ids = [cat.id for cat in categorie_esistenti]
    
    if len(categorie_esistenti_ids) != len(categoria_ids_list):
        categorie_non_trovate = set(categoria_ids_list) - set(categorie_esistenti_ids)
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Categorie non trovate: {list(categorie_non_trovate)}"
        )

    # Step 1: Trova i quiz che contengono TUTTE le categorie richieste
    quiz_con_tutte_categorie = (
        db.query(Quiz.id)
        .join(QuizQuesito, Quiz.id == QuizQuesito.quiz_id)
        .join(Quesito, QuizQuesito.quesito_id == Quesito.id)
        .join(CategoriaQuesito, CategoriaQuesito.id_quesito == Quesito.id)
        .join(Categoria, Categoria.id == CategoriaQuesito.id_categoria)
        .filter(Quiz.id_utente == student_id)
        .filter(Categoria.id.in_(categoria_ids_list))
        .group_by(Quiz.id)
        .having(func.count(func.distinct(Categoria.id)) == len(categoria_ids_list))
    )

    # Step 2: Carica tutti i dati per questi quiz specifici
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
     .filter(Quiz.id.in_(quiz_con_tutte_categorie))\
     .all()

    if not records:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Nessun quiz trovato per questo studente nelle categorie specificate."
        )

    # Converti in DataFrame
    df = pd.DataFrame(records)

    # Etichetta categoria con nome docente
    df["categoria_label"] = df["categoria_nome"] + " (" + df["docente_username"] + ")"

    # Calcola indicatori
    df["corretta"] = (df["risposta_utente"] == df["op_corretta"]).astype(int)
    df["sbagliata"] = ((df["risposta_utente"].notnull()) & (df["risposta_utente"] != df["op_corretta"])).astype(int)
    df["non_data"] = df["risposta_utente"].isnull().astype(int)

    # Calcolo percentuali totali - CONVERSIONE A TIPI PYTHON NATIVI
    totale_quesiti = int(len(df))
    totale_corrette = int(df["corretta"].sum())
    totale_sbagliate = int(df["sbagliata"].sum())
    totale_non_date = int(df["non_data"].sum())

    perc_corrette_totali = round((totale_corrette / totale_quesiti) * 100, 2) if totale_quesiti > 0 else 0.0
    perc_sbagliate_totali = round((totale_sbagliate / totale_quesiti) * 100, 2) if totale_quesiti > 0 else 0.0
    perc_non_date_totali = round((totale_non_date / totale_quesiti) * 100, 2) if totale_quesiti > 0 else 0.0

    # Statistiche aggregate per difficoltà
    grouped = df.groupby(["difficolta"]).agg(
        corrette=("corretta", "sum"),
        sbagliate=("sbagliata", "sum"),
        non_date=("non_data", "sum"),
    ).reset_index()

    # Converti i valori numpy in tipi Python nativi
    stats = []
    for _, row in grouped.iterrows():
        stats.append({
            "difficolta": row["difficolta"],
            "corrette": int(row["corrette"]),
            "sbagliate": int(row["sbagliate"]),
            "non_date": int(row["non_date"])
        })

    # Numero di quiz e quesiti - CONVERSIONE A TIPI PYTHON NATIVI
    quiz_e_quesiti_totali = {
        "quiz_unici": int(df["quiz_id"].nunique()),
        "quesiti_totali": int(len(df)),
        "categorie_coinvolte": int(df["categoria_nome"].nunique())
    }

    # Andamento temporale per difficoltà - raggruppa per data (solo giorno, non l'orario)
    df["data_solo"] = df["data_quiz"].dt.date  # Estrae solo la data (YYYY-MM-DD)

    andamento = df.groupby(["data_solo", "difficolta"]).agg(
        corrette=("corretta", "sum"),
        totale_quesiti=("corretta", "count")  # Conta tutte le righe in cui la colonna corretta è valorizzata (ovvero tutte perchè "corretta" non è mai null. Conta corretti + sbagliati + non dati)
    ).reset_index()

    # Converti i valori numpy in tipi Python nativi per timeline
    timeline = []
    for _, row in andamento.iterrows():
        timeline.append({
            "data_quiz": row["data_solo"].strftime("%Y-%m-%d") if pd.notnull(row["data_solo"]) else None,
            "difficolta": row["difficolta"],
            "corrette": int(row["corrette"]),
            "totale_quesiti": int(row["totale_quesiti"])
        })

    return {
        "categorie_filtrate": categoria_ids_list,
        "percentuali_totali": {
            "perc_corrette": float(perc_corrette_totali),
            "perc_sbagliate": float(perc_sbagliate_totali),
            "perc_non_date": float(perc_non_date_totali),
            "totale_quesiti": totale_quesiti,
            "totale_corrette": totale_corrette,
            "totale_sbagliate": totale_sbagliate,
            "totale_non_date": totale_non_date
        },
        "quiz_e_quesiti_totali": quiz_e_quesiti_totali,  
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
            tot_risposte=("corrette", "count"),
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
            corrette=("corrette", "sum"),
            sbagliate=("sbagliata", "sum"),
            non_date=("non_data", "sum"),
            tot_risposte=("corrette", "count"),
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
            corrette=("corrette", "sum"),
            sbagliate=("sbagliata", "sum"),
            non_date=("non_data", "sum"),
            tot_risposte=("corrette", "count"),
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

