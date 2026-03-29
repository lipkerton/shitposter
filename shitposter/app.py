from fastapi import FastAPI

from auth import basic_auth


app = FastAPI() 
app.include_router(basic_auth.soap)