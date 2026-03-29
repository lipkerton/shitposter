from fastapi import FastAPI
from pydantic_xml import element
from fastapi_soap import SoapRouter, XMLBody, SoapResponse
from fastapi_soap.models import BodyContent


class Operands(BodyContent, tag="Operands"):
    operands: list[float] = element(tag="Operand")

class Result(BodyContent, tag="Result"):
    value: float

soap = SoapRouter(name='Calculator', prefix='/Calculator')

@soap.operation(
    name="SumOperation",
    request_model=Operands,
    response_model=Result
)
def sum_operation(body: Operands = XMLBody(Operands)):

    result =  sum(body.operands)
    return SoapResponse(
        Result(value=body.operands[1])
    )


app = FastAPI()
app.include_router(soap)