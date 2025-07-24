from pydantic import BaseModel
from typing import List

# Modello per il request body
class StudentStatsRequest(BaseModel):
    categoria_ids: List[int]