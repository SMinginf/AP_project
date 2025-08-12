from fastapi import FastAPI
<<<<<<< HEAD
from routers import auth  # importa il router per le API di autenticazione
=======
from routers import handlers  # importa il router per le API di autenticazione
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba

app = FastAPI(
    title="Auth Service",
    description="Servizio per registrazione e autenticazione degli utenti"
)

<<<<<<< HEAD
# Include le rotte dal modulo routers/auth.py
app.include_router(auth.router, prefix="/auth", tags=["auth"])
=======
# Include le rotte dal modulo routers/handlers.py
app.include_router(handlers.router, prefix="/auth", tags=["auth"])
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba


