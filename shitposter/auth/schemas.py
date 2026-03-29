from pydantic_xml import element
from fastapi_soap.models import BodyContent


class OpenSessionRequest(
    BodyContent,
    tag="osmreq",
    nsmap={
        '': 'http://ifx.ru/IFXWebService',
        'soap': 'http://ifx.ru/IFXWebService'  
    }
):
    mbci: str = element(tag="mbci")    # тип клиента.
    mbcv: float = element(tag="mbcv")  # версия клиента.
    mbh: str = element(tag="mbh")      # состав атрибутов новости.
    mbi: str = element(tag="mbi")      # логин пользователя.
    mbla: str = element(tag="mbla")    # язык интерфейса пользователя.
    mbo: str = element(tag="mbo")      # операционная система.
    mbp: str = element(tag="mbp")      # пароль пользователя.
    mbt: str = element(tag="mbt")      # хз.

class OpenSessionResponse(BodyContent, tag="osmresp"):
    mbr: bool = element(tag="mbr")     # хз.
    mbsid: str = element(tag="mbsid")  # идентификатор открытой сессии.

