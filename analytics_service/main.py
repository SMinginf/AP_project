from fastapi import FastAPI
from analytics_service.handlers import handlers  # importa il router per le API di autenticazione

app = FastAPI(
    title="Analytics Service",
    description="Servizio per l'elaborazione e analisi dei dati degli utenti"
)

# Include le rotte
app.include_router(handlers.routes_group, prefix="/stats", tags=["analytics"])


