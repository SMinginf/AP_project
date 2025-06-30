from fastapi import FastAPI
from routers import auth  # importa il router per le API di autenticazione

app = FastAPI(
    title="Auth Service",
    description="Servizio per registrazione e autenticazione degli utenti"
)

# Include le rotte dal modulo routers/auth.py
app.include_router(auth.router, prefix="/auth", tags=["auth"])


