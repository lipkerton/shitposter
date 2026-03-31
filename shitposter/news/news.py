import random
from datetime import datetime
from typing import Annotated

from passlib.context import CryptContext
from fastapi_soap import SoapRouter, XMLBody, SoapResponse
from fastapi_soap.models import BodyContent

from news.schemas import (
    NewsItem,
    NewsList,
    GetRealtimeNewsByProductRequest,
    GetRealtimeNewsByProductResponse
)

soap = SoapRouter(
    name="GetRealtimeNewsByProduct",
    prefix="/IFXService.svc"
)


@soap.operation(
    name="GetRealtimeNewsByProduct",
    request_model=GetRealtimeNewsByProductRequest,
    response_model=GetRealtimeNewsByProductResponse
)
def get_realtime_news_by_product(
    body: GetRealtimeNewsByProductRequest = XMLBody(
        GetRealtimeNewsByProductRequest
    )
):
    now_t = datetime.now()
    news_objs = [
        NewsItem(
            h =str(random.random()),
            i =datetime.strftime(now_t, "%Y%m%d%H%M%S%f"),
            pd=datetime.strftime(now_t, "%Y-%m-%dT%H:%M:%S")
        ) for _ in range(random.randrange(1, 100))
    ]
    news_list = NewsList(
        c_nwli=news_objs
    )
    response = SoapResponse(
        GetRealtimeNewsByProductResponse(
            maxcount=len(news_objs),
            mbnl=news_list,
            mbnup=datetime.strftime(now_t, "%Y%m%d%H%M%S%f")
        )
    )
    return response

