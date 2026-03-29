from typing import Annotated

from passlib.context import CryptContext
from fastapi_soap import SoapRouter, XMLBody, SoapResponse
from fastapi_soap.models import BodyContent

from auth.schemas import OpenSessionRequest, OpenSessionResponse


soap = SoapRouter(
    name="OpenSession",
    prefix="/IFXService.svc"
)
pwd_context = CryptContext(
    schemes=["bcrypt", "sha256_crypt"], deprecated="auto"
)

my_user = {
    "lipkerton": {
        "username": "lipkerton",
        "hashed_password": pwd_context.hash("super-secret")
    }
}

def check_password(username: str, password: str):
    return pwd_context.verify_and_update(password, my_user[username]["hashed_password"])


@soap.operation(
    name="OpenSession",
    request_model=OpenSessionRequest,
    response_model=OpenSessionResponse
)
def open_session(body: OpenSessionRequest = XMLBody(OpenSessionRequest)):
    if body.mbi not in my_user:
        return
    if not check_password(body.mbi, body.mbp):
        return
    response = SoapResponse(
        OpenSessionResponse(
            mbr="false",
            mbsid=""
        )
    )
    response.set_cookie(key="fakesession", value="fake-cookie-session-value")
    return response
