from fastapi import FastAPI
from handlers.handlers import routes_group  # importa il router per le API di autenticazione
import uvicorn

app = FastAPI(
    title="Analytics Service",
    description="Servizio per l'elaborazione e analisi dei dati degli utenti"
)

# Include le rotte
app.include_router(routes_group, prefix="/stats", tags=["analytics"])


# Avvia il server specificando la porta
if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=8005, reload=True)