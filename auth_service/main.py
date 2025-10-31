from fastapi import FastAPI
from handlers import auth_controller  # importa il router per le API di autenticazione
import uvicorn

app = FastAPI(
    title="Auth Service",
    description="Servizio per registrazione e autenticazione degli utenti"
)

# Include le rotte dal modulo routers/handlers.py
app.include_router(auth_controller.router, prefix="/auth", tags=["auth"])


# Avvia il server specificando la porta
if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)