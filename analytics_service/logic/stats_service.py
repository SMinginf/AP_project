from typing import List
from sqlalchemy.orm import Session
from sqlalchemy import func
from models.ORM_models import Quiz, QuizQuesito, Quesito, CategoriaQuesito, Categoria, Utente
import pandas as pd
import math

# [AGGIUNTO] Eccezioni di dominio che il controller mapperà in HTTP status
class BadRequestError(Exception):
    pass

class NotFoundError(Exception):
    pass


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
# Service: Statistiche generali per Studenti
# ----------------------------
def get_student_general_stats_logic(student_id: int, db: Session) -> dict:
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
        raise NotFoundError(f"Studente con ID {student_id} non trovato.")

    # Recupera tutti i dati dei quiz dello studente
    records = db.query(
        Quiz.id.label("quiz_id"),
        Quiz.data.label("data_quiz"),
        Quiz.durata,
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
     .filter(Categoria.pubblica == True)\
     .filter(Quiz.id_utente == student_id)\
     .all()

    if not records:
        raise NotFoundError("Nessun quiz trovato per questo studente.")

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

    # Durata media dei quiz
    # Converti la colonna 'durata' da datetime.time a timedelta
    df["durata_timedelta"] = pd.to_timedelta(df["durata"].astype(str))
    # Calcola la media
    durata_media_quiz = df["durata_timedelta"].mean()
    # Ottieni la durata in secondi totali
    total_seconds = int(durata_media_quiz.total_seconds())
    # Calcola ore, minuti e secondi
    hours = total_seconds // 3600
    minutes = (total_seconds % 3600) // 60
    seconds = total_seconds % 60
    # Formatta come hh:mm:ss
    durata_media_formattata = f"{hours:02}:{minutes:02}:{seconds:02}"
    
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
        "categoria_piu_debole": categoria_piu_debole,
        "durata_media_quiz": durata_media_formattata
    }


# ----------------------------
# Service: Statistiche per Studenti per categorie selezionate
# ----------------------------
def get_student_stats_per_categorie_logic(student_id: int, categoria_ids_list: List[int], db: Session) -> dict:
    # Verifica che lo studente esista
    studente = db.query(Utente).filter(Utente.id == student_id).first()
    if not studente:
        raise NotFoundError(f"Studente con ID {student_id} non trovato.")

    # Verifica che le categorie esistano e che siano pubbliche
    categorie_esistenti = db.query(Categoria.id).filter(Categoria.id.in_(categoria_ids_list), Categoria.pubblica == True).all()
    categorie_esistenti_ids = [cat.id for cat in categorie_esistenti]
    if len(categorie_esistenti_ids) != len(categoria_ids_list):
        categorie_non_trovate = set(categoria_ids_list) - set(categorie_esistenti_ids)
        raise NotFoundError(f"Categorie non trovate: {list(categorie_non_trovate)}")

    # Step 1: Trova i quiz che contengono TUTTE le categorie richieste
    quiz_con_tutte_categorie = (
        db.query(Quiz.id)
        .join(QuizQuesito, Quiz.id == QuizQuesito.quiz_id)
        .join(Quesito, QuizQuesito.quesito_id == Quesito.id)
        .join(CategoriaQuesito, CategoriaQuesito.id_quesito == Quesito.id)
        .join(Categoria, Categoria.id == CategoriaQuesito.id_categoria)
        .filter(Categoria.pubblica == True)
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
     .filter(Categoria.pubblica == True)\
     .all()

    if not records:
        raise NotFoundError("Nessun quiz trovato per questo studente nelle categorie specificate.")

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

    andamento_per_difficolta = df.groupby(["data_solo", "difficolta"]).agg(
        corrette=("corretta", "sum"),
        totale_quesiti=("corretta", "count")  # Conta tutte le righe in cui la colonna corretta è valorizzata (ovvero tutte perchè "corretta" non è mai null. Conta corretti + sbagliati + non dati)
    ).reset_index()

    # Converti i valori numpy in tipi Python nativi per timeline_per_difficolta
    timeline_per_difficolta = []
    for _, row in andamento_per_difficolta.iterrows():
        timeline_per_difficolta.append({
            "data_quiz": row["data_solo"].strftime("%Y-%m-%d") if pd.notnull(row["data_solo"]) else None,
            "difficolta": row["difficolta"],
            "corrette": int(row["corrette"]),
            "totale_quesiti": int(row["totale_quesiti"])
        })

    # 🟦 Andamento totale aggregato per giorno (somma su tutte le difficoltà)
    andamento_totale = df.groupby("data_solo").agg(
        corrette=("corretta", "sum"),
        totale_quesiti=("corretta", "count")
    ).reset_index()

    # Converte in lista di dict come nel caso per difficoltà
    timeline_totale = []
    for _, row in andamento_totale.iterrows():
        timeline_totale.append({
            "data_quiz": row["data_solo"].strftime("%Y-%m-%d") if pd.notnull(row["data_solo"]) else None,
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
        "andamento_temporale_per_difficolta": timeline_per_difficolta,
        "andamento_temporale_totale": timeline_totale,
    }


# ----------------------------
# Service: Statistiche generali per Docenti
# ----------------------------
def get_teacher_general_stats_logic(teacher_id: int, db: Session) -> dict:
    """Restituisce statistiche generali per un docente specifico."""
    # Verifica che il docente esista
    docente = db.query(Utente).filter(Utente.id == teacher_id).first()
    if not docente:
        raise NotFoundError(f"Docente con ID {teacher_id} non trovato.")

    # Recupera tutte le risposte ai quesiti creati dal docente
    records = (
        db.query(
            Quiz.id_utente.label("studente_id"),
            Quiz.durata,
            Quiz.data.label("data_quiz"),
            QuizQuesito.quesito_id,
            QuizQuesito.risposta_utente,
            Quesito.difficolta,
            Quesito.op_corretta,
            Quesito.testo.label("quesito_testo"),
            Categoria.id.label("categoria_id"),
            Categoria.nome.label("categoria_nome"),
            Categoria.pubblica  
        )
        .join(QuizQuesito, Quiz.id == QuizQuesito.quiz_id)
        .join(Quesito, QuizQuesito.quesito_id == Quesito.id)
        .join(CategoriaQuesito, CategoriaQuesito.id_quesito == Quesito.id)
        .join(Categoria, Categoria.id == CategoriaQuesito.id_categoria)
        .filter(Quesito.id_docente == teacher_id)
        .all()
    )

    if not records:
        raise NotFoundError("Nessuna risposta trovata per i quesiti di questo docente.")

    # Costruzione DataFrame dal risultato della query
    df = pd.DataFrame(records)

    # Calcola il numero totale di quesiti inseriti dal docente
    num_quesiti_inseriti = db.query(Quesito).filter(Quesito.id_docente == teacher_id).count()
    # Calcola il numero di quesiti del docente affrontati dagli utenti
    num_quesiti_affrontati = df["quesito_id"].nunique()
    # Calcola il numero di utenti che hanno affrontato almeno un quesito del docente
    num_utenti = df["studente_id"].nunique()

    # CALCOLA LA DURATA MEDIA DEI QUIZ IN CUI COMPAIONO I QUESITI DEL DOCENTE
    # Converti la colonna 'durata' da datetime.time a timedelta
    df["durata_timedelta"] = pd.to_timedelta(df["durata"].astype(str))
    # Calcola la media
    durata_media_quiz = df["durata_timedelta"].mean()
    # Ottieni la durata in secondi totali
    total_seconds = int(durata_media_quiz.total_seconds())
    # Calcola ore, minuti e secondi
    hours = total_seconds // 3600
    minutes = (total_seconds % 3600) // 60
    seconds = total_seconds % 60
    # Formatta come hh:mm:ss
    durata_media_formattata = f"{hours:02}:{minutes:02}:{seconds:02}"

    # Calcolo indicatori binari per ogni risposta
    df["corretta"] = (df["risposta_utente"] == df["op_corretta"]).astype(int)
    df["sbagliata"] = (
        (df["risposta_utente"].notnull()) & (df["risposta_utente"] != df["op_corretta"])
    ).astype(int)
    df["non_data"] = df["risposta_utente"].isnull().astype(int)

    df["visibilità"] = df["pubblica"].apply(lambda x: "Pubblica" if x else "Privata")

    categories_grouped = (
        df.groupby(["categoria_nome", "visibilità"])
        .agg(
            corrette=("corretta", "sum"),
            sbagliate=("sbagliata", "sum"),
            non_date=("non_data", "sum"),
            tot_quesiti=("quesito_id", "nunique"), # quesiti del docente affrontati dagli utenti
        )
        .reset_index()
    )

    # Percentuali di correttezza per livello di difficoltà
    categories_grouped["perc_corrette"] = (categories_grouped["corrette"] / categories_grouped["tot_quesiti"]).fillna(0).round(2)
    categories_grouped["perc_sbagliate"] = (categories_grouped["sbagliate"] / categories_grouped["tot_quesiti"]).fillna(0).round(2)
    categories_grouped["perc_non_date"] = (categories_grouped["non_date"] / categories_grouped["tot_quesiti"]).fillna(0).round(2)

    return {
        "stats_per_categoria": categories_grouped.to_dict(orient="records"),
        "num_quesiti_affrontati": int(num_quesiti_affrontati),
        "num_utenti": int(num_utenti),
        "durata_media_quiz": durata_media_formattata,
        "num_quesiti_inseriti": int(num_quesiti_inseriti)
    }


# ----------------------------
# Service: Statistiche docente per categorie selezionate
# ----------------------------
def get_teacher_stats_per_categorie_logic(teacher_id: int, categoria_ids_list: List[int], db: Session) -> dict:
    """Restituisce statistiche sui quesiti creati dal docente.

    Le informazioni sono calcolate aggregando le risposte che gli studenti
    hanno fornito nei quiz che includevano tali quesiti.
    """
    # Verifica che il docente esista
    docente = db.query(Utente).filter(Utente.id == teacher_id).first()
    if not docente:
        raise NotFoundError(f"Docente con ID {teacher_id} non trovato.")
    
    # Verifica che le categorie esistano
    categorie_esistenti = db.query(Categoria.id).filter(Categoria.id.in_(categoria_ids_list)).all()
    categorie_esistenti_ids = [cat.id for cat in categorie_esistenti]
    if len(categorie_esistenti_ids) != len(categoria_ids_list):
        categorie_non_trovate = set(categoria_ids_list) - set(categorie_esistenti_ids)
        raise NotFoundError(f"Categorie non trovate: {list(categorie_non_trovate)}")

    # Recupera tutte le risposte ai quesiti creati dal docente che appartengono a TUTTE le categorie selezionate
    # Step 1: Trova i quesiti che appartengono a TUTTE le categorie richieste
    quesiti_con_tutte_categorie = (
        db.query(Quesito.id)
        .join(CategoriaQuesito, CategoriaQuesito.id_quesito == Quesito.id)
        .join(Categoria, Categoria.id == CategoriaQuesito.id_categoria)
        .filter(Quesito.id_docente == teacher_id)
        .filter(Categoria.id.in_(categoria_ids_list))
        .group_by(Quesito.id)
        .having(func.count(func.distinct(Categoria.id)) == len(categoria_ids_list))
    )

    # Step 2: Recupera tutte le risposte per questi quesiti specifici       
    records = (
        db.query(
            Utente.username.label("studente_username"),
            Quiz.data.label("data_quiz"),
            QuizQuesito.quesito_id,
            QuizQuesito.risposta_utente,
            Quesito.difficolta,
            Quesito.op_corretta,
            Quesito.testo.label("quesito_testo"),
            Categoria.id.label("categoria_id"),
            Categoria.nome.label("categoria_nome"),
        )
        .join(Quiz, Quiz.id_utente == Utente.id)
        .join(QuizQuesito, Quiz.id == QuizQuesito.quiz_id)
        .join(Quesito, QuizQuesito.quesito_id == Quesito.id)
        .join(CategoriaQuesito, CategoriaQuesito.id_quesito == Quesito.id)
        .join(Categoria, Categoria.id == CategoriaQuesito.id_categoria)
        .filter(Quesito.id.in_(quesiti_con_tutte_categorie))  # Filtra per quesiti con tutte le categorie
        .all()
    )

    # Equivale a fare:
    # SELECT quesito.id
    # FROM quesito 
    # JOIN categoria_quesito ON categoria_quesito.id_quesito = quesito.id
    # JOIN categoria ON categoria.id = categoria_quesito.id_categoria
    # WHERE quesito.id_docente = {teacher_id} AND categoria.id IN ({categoria_ids_list})
    # GROUP BY quesito.id
    # HAVING COUNT(DISTINCT categoria.id) = {len(categoria_ids_list)}

    if not records:
        raise NotFoundError("Nessuna risposta trovata per i quesiti di questo docente.")

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
            tot_quesiti=("quesito_id", "nunique"),
        )
        .reset_index()
    )

    # Percentuali di correttezza per livello di difficoltà
    diff_grouped["perc_corrette"] = (diff_grouped["corrette"] / diff_grouped["tot_quesiti"] * 100).fillna(0).round(2)
    diff_grouped["perc_sbagliate"] = (diff_grouped["sbagliate"] / diff_grouped["tot_quesiti"] * 100).fillna(0).round(2)
    diff_grouped["perc_non_date"] = (diff_grouped["non_date"] / diff_grouped["tot_quesiti"] * 100).fillna(0).round(2)

    # Classifica studenti e relativi punteggi
    student_rankings = (
        df.groupby("studente_username")
        .agg(
            corrette=("corretta", "sum"),
            sbagliate=("sbagliata", "sum"),
            non_date=("non_data", "sum"),
            tot_risposte=("corretta", "count"),
        )
        .reset_index()
    )

    # Calcola il punteggio per ogni studente usando la funzione calcola_punteggio
    student_rankings["punteggio"] = student_rankings.apply(
        lambda row: calcola_punteggio(int(row["corrette"]), int(row["sbagliate"])), axis=1
    )

    # Calcola percentuali
    student_rankings["perc_corrette"] = (student_rankings["corrette"] / student_rankings["tot_risposte"] * 100).fillna(0).round(2)
    student_rankings["perc_sbagliate"] = (student_rankings["sbagliate"] / student_rankings["tot_risposte"] * 100).fillna(0).round(2)
    student_rankings["perc_non_date"] = (student_rankings["non_date"] / student_rankings["tot_risposte"] * 100).fillna(0).round(2)

    # Ordina per punteggio decrescente e prendi i primi 10
    top_10_students = student_rankings.nlargest(10, "punteggio")

    # Converti in lista di dizionari con posizione in classifica
    classifica_studenti = []
    for idx, (_, row) in enumerate(top_10_students.iterrows(), 1):
        classifica_studenti.append({
            "posizione": idx,
            "studente_username": row["studente_username"],
            "corrette": int(row["corrette"]),
            "sbagliate": int(row["sbagliate"]),
            "non_date": int(row["non_date"]),
            "tot_risposte": int(row["tot_risposte"]),
            "perc_corrette": float(row["perc_corrette"]),
            "perc_sbagliate": float(row["perc_sbagliate"]),
            "perc_non_date": float(row["perc_non_date"]),
            "punteggio": round(float(row["punteggio"]), 3)
        })

    # Statistiche aggregate per singolo quesito
    quesiti_stats = (
        df.groupby(["quesito_id", "quesito_testo", "difficolta"])  # ✅ Aggiungi le colonne necessarie
        .agg(
            corrette=("corretta", "sum"),
            sbagliate=("sbagliata", "sum"),
            non_date=("non_data", "sum"),
            tot_risposte=("corretta", "count"),
        )
        .reset_index()
    )

    # Calcola percentuali per ogni quesito
    quesiti_stats["perc_corrette"] = (quesiti_stats["corrette"] / quesiti_stats["tot_risposte"] * 100).fillna(0).round(2)
    quesiti_stats["perc_sbagliate"] = (quesiti_stats["sbagliate"] / quesiti_stats["tot_risposte"] * 100).fillna(0).round(2)
    quesiti_stats["perc_non_date"] = (quesiti_stats["non_date"] / quesiti_stats["tot_risposte"] * 100).fillna(0).round(2)

    # Trova il quesito più indovinato (percentuale corrette più alta)
    quesito_piu_indovinato = None
    quesito_piu_sbagliato = None

    if len(quesiti_stats) > 0:
        # Ordina per percentuale corrette (decrescente)
        quesiti_ordinati_corrette = quesiti_stats.sort_values("perc_corrette", ascending=False)
        migliore = quesiti_ordinati_corrette.iloc[0]
        quesito_piu_indovinato = {
            "quesito_id": int(migliore["quesito_id"]),
            "quesito_testo": migliore["quesito_testo"],  
            "difficolta": migliore["difficolta"],        
            "corrette": int(migliore["corrette"]),
            "sbagliate": int(migliore["sbagliate"]),
            "non_date": int(migliore["non_date"]),
            "tot_risposte": int(migliore["tot_risposte"]),
            "perc_corrette": float(migliore["perc_corrette"]),
            "perc_sbagliate": float(migliore["perc_sbagliate"]),
            "perc_non_date": float(migliore["perc_non_date"])
        }
        peggiore = quesiti_ordinati_corrette.iloc[-1]
        quesito_piu_sbagliato = {
            "quesito_id": int(peggiore["quesito_id"]),
            "quesito_testo": peggiore["quesito_testo"],  
            "difficolta": peggiore["difficolta"],        
            "corrette": int(peggiore["corrette"]),
            "sbagliate": int(peggiore["sbagliate"]),
            "non_date": int(peggiore["non_date"]),
            "tot_risposte": int(peggiore["tot_risposte"]),
            "perc_corrette": float(peggiore["perc_corrette"]),
            "perc_sbagliate": float(peggiore["perc_sbagliate"]),
            "perc_non_date": float(peggiore["perc_non_date"])
        }

    return {
        "stats_per_difficolta": diff_grouped.to_dict(orient="records"),
        "top_10_studenti": classifica_studenti,
        "quesito_piu_indovinato": quesito_piu_indovinato,
        "quesito_piu_sbagliato": quesito_piu_sbagliato,
    }
