from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session
from typing import List
from database.connect import get_db
from utils.auth import validate_token
from logic import stats_service  # [AGGIUNTO] delega alla logica applicativa

# Una dependency globale in FastAPI è una funzione o un oggetto che viene 
# eseguito automaticamente per tutte le rotte di un router o dell'intera 
# applicazione. Questo permette di applicare logica comune (come validazione, 
# autenticazione, logging, ecc.) a tutte le richieste senza doverla specificare 
# manualmente per ogni endpoint.
routes_group = APIRouter(dependencies=[Depends(validate_token)])


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
    try:
        return stats_service.get_student_general_stats_logic(student_id, db)
    except stats_service.NotFoundError as e:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail=str(e))
    except stats_service.BadRequestError as e:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=str(e))
    except Exception as e:  # [AGGIUNTO] fallback errore generico
        raise HTTPException(status_code=status.HTTP_500_INTERNAL_SERVER_ERROR, detail=str(e))


# ----------------------------
# Endpoint: Statistiche per Studenti per categorie selezionate
# ----------------------------
@routes_group.get("/student/{student_id}", summary="Statistiche studente per categorie specifiche")
def get_student_stats_per_categorie(student_id: int, categoria_ids: str, db: Session = Depends(get_db)):
    # Converti la stringa in lista di interi
    try:
        categoria_ids_list: List[int] = [int(id.strip()) for id in categoria_ids.split(",") if id.strip()]
        if not categoria_ids_list:
            raise ValueError("Lista categorie vuota")
    except ValueError:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Formato categoria_ids non valido. Usa il formato: '1,2,3'"
        )
    try:
        return stats_service.get_student_stats_per_categorie_logic(student_id, categoria_ids_list, db)
    except stats_service.NotFoundError as e:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail=str(e))
    except stats_service.BadRequestError as e:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=str(e))
    except Exception as e:  # [AGGIUNTO]
        raise HTTPException(status_code=status.HTTP_500_INTERNAL_SERVER_ERROR, detail=str(e))


# ----------------------------
# Endpoint: Statistiche generali per Docenti
# ----------------------------
@routes_group.get("/teacher/{teacher_id}/general", summary="Statistiche generali per tutti i docenti")
def get_teacher_general_stats(teacher_id: int, db: Session = Depends(get_db)):
    """Restituisce statistiche generali per un docente specifico."""
    try:
        return stats_service.get_teacher_general_stats_logic(teacher_id, db)
    except stats_service.NotFoundError as e:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail=str(e))
    except stats_service.BadRequestError as e:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=str(e))
    except Exception as e:  # [AGGIUNTO]
        raise HTTPException(status_code=status.HTTP_500_INTERNAL_SERVER_ERROR, detail=str(e))


# STATISTICHE DOCENTE PER CATEGORIE SELEZIONATE
@routes_group.get("/teacher/{teacher_id}", summary="Statistiche docente")
def get_teacher_stats_per_categorie(teacher_id: int, categoria_ids: str, db: Session = Depends(get_db)):
    """Restituisce statistiche sui quesiti creati dal docente.

    Le informazioni sono calcolate aggregando le risposte che gli studenti
    hanno fornito nei quiz che includevano tali quesiti.
    """
    # Converti la stringa in lista di interi
    try:
        categoria_ids_list: List[int] = [int(id.strip()) for id in categoria_ids.split(",") if id.strip()]
        if not categoria_ids_list:
            raise ValueError("Lista categorie vuota")
    except ValueError:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Formato categoria_ids non valido. Usa il formato: '1,2,3'"
        )

    try:
        return stats_service.get_teacher_stats_per_categorie_logic(teacher_id, categoria_ids_list, db)
    except stats_service.NotFoundError as e:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail=str(e))
    except stats_service.BadRequestError as e:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=str(e))
    except Exception as e:  # [AGGIUNTO]
        raise HTTPException(status_code=status.HTTP_500_INTERNAL_SERVER_ERROR, detail=str(e))
