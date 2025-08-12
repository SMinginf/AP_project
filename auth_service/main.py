from fastapi import FastAPI
from handlers import auth_controller  # importa il router per le API di autenticazione

app = FastAPI(
    title="Auth Service",
    description="Servizio per registrazione e autenticazione degli utenti"
)

# Include le rotte dal modulo routers/handlers.py
app.include_router(auth_controller.router, prefix="/auth", tags=["auth"])


