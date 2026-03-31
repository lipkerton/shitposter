from fastapi import FastAPI

from auth import auth
from news import news


app = FastAPI() 
app.include_router(auth.soap)
app.include_router(news.soap)