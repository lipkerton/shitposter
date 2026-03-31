from datetime import datetime
from typing import Optional

from pydantic_xml import element
from fastapi_soap.models import BodyContent


class GetRealtimeNewsByProductRequest(
    BodyContent,
    tag="grnbpmreq",
    nsmap={
        '': 'http://ifx.ru/IFXWebService',
        'soap': 'http://ifx.ru/IFXWebService'
    }
):
    direction: int = element(tag="direction")
    mbcid: None = element(
        tag="mbcid",
        default=None,
        nsmap = {
            'i': 'http://www.w3.org/2001/XMLSchema-instance'
        }
    )
    mblnl: int = element(tag="mblnl")
    mbsup: None = element(
        tag="mbsup",
        default=None,
        nsmap = {
            'i': 'http://www.w3.org/2001/XMLSchema-instance'
        }
    )

class NewsItem(
    BodyContent,
    tag="c_nwli"
):
    h: str = element(tag="h")
    i: int = element(tag="i")
    pd: datetime = element(tag="pd")

class NewsList(
    BodyContent,
    tag="mbnl",
    nsmap = {
        'i': 'http://www.w3.org/2001/XMLSchema-instance'
    }
):
    c_nwli: list[NewsItem] = element(tag="c_nwli")

class GetRealtimeNewsByProductResponse(
    BodyContent,
    tag="grnbpmresp",
    nsmap={
        '': 'http://ifx.ru/IFXWebService',
        'soap': 'http://ifx.ru/IFXWebService'
    }
):
    maxcount: int = element(tag="maxcount")
    mbnl: NewsList = element(
        tag="mbnl",
        nsmap = {
            'i': 'http://www.w3.org/2001/XMLSchema-instance'
        }
    )
