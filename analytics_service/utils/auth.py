from jose import JWTError, jwt
from datetime import datetime, timedelta, timezone
from typing import Optional
from fastapi import HTTPException, Header


SECRET_KEY = "supersegreta123"  
ALGORITHM = "HS256"


def validate_token(authorization: str = Header(None)) -> dict:
    """
    Dependency per validare il token JWT direttamente dall'header Authorization.
    
    Args:
        authorization (str): Header Authorization contenente il token JWT
        
    Returns:
        dict: I dati utente decodificati dal token
    
    Raises:
        HTTPException: Se il token è non valido o scaduto
    """
    if not authorization or not authorization.startswith("Bearer "):
        raise HTTPException(status_code=401, detail="Token non fornito o malformato")
    
    # Estrae il token dall'header Authorization.
    # Se il token non inizia con "Bearer ", restituisce un errore 401.
    token = authorization.split("Bearer ")[1]
    user_data = get_data_from_token(token)
    if not user_data:
        raise HTTPException(status_code=401, detail="Token non valido o scaduto")
    return user_data


def get_data_from_token(token: str) -> Optional[dict]:
    """
    Decodifica il token JWT e restituisce i dati utente.

    Args:
        token (str): Il token JWT da decodificare.

    Returns:
        dict: I dati utente decodificati dal token, oppure None se il token è non valido.

    Raises:
        HTTPException: Se il token non può essere decodificato.
    """
    try:
        # Decodifica il token usando la chiave segreta e l'algoritmo
        payload = jwt.decode(token, SECRET_KEY, algorithms=[ALGORITHM])
        return payload
    except JWTError:
        # Restituisce None se il token non è valido
        return None