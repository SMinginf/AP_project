from jose import JWTError, jwt
from datetime import datetime, timedelta, timezone
from typing import Optional

# Chiave segreta per firmare il token
SECRET_KEY = "supersegreta123"  
ALGORITHM = "HS256"
ACCESS_TOKEN_EXPIRE_MINUTES = 60  # 1 ora

def create_access_token(data: dict, expires_delta: Optional[timedelta] = None):
    # Faccio una copia del dizionario per evitare di modificarlo direttamente
    to_encode = data.copy()
    expire = datetime.now(timezone.utc) + (expires_delta or timedelta(minutes=ACCESS_TOKEN_EXPIRE_MINUTES))
    
    # Aggiunge al token il campo "exp" richiesto da JWT (expiration time)
    to_encode.update({"exp": expire})
    encoded_jwt = jwt.encode(to_encode, SECRET_KEY, algorithm=ALGORITHM)
    return encoded_jwt

