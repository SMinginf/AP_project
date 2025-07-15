from fastapi import FastAPI
from routers import handlers  # importa il router per le API di autenticazione

app = FastAPI(
    title="Auth Service",
    description="Servizio per registrazione e autenticazione degli utenti"
)

# Include le rotte dal modulo routers/handlers.py
app.include_router(handlers.router, prefix="/auth", tags=["auth"])


