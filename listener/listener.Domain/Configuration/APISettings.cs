namespace listener.Domain.Configuration;

public class APISettings
{
    public APICallSettings OpenSession { get; set; } = new APICallSettings()
    {
        APIUrl = "http://localhost:8000/IFXService.svc/OpenSession",
        XMLReq = "OpenSession.xml",
        ReqInterval = 1440
    };
    public APICallSettings GetRealtimeNewsByProduct { get; set; } = new APICallSettings()
    {
        APIUrl = "http://localhost:8000/IFXService.svc/GetRealtimeNewsByProduct",
        XMLReq = "GetRealtimeNewsByProduct.xml",
        ReqInterval = 60
    };
    public APICallSettings GetEntireNewsByID { get; set; } = new APICallSettings()
    {
        APIUrl = "http://localhost:8000/IFXService.svc/GetEntireNewsByID",
        XMLReq = "GetEntireNewsByID.xml",
        ReqInterval = 0
    };
    public string RequestPath { get; set; } = "Requests";
    public int CleanupInterval { get; set; } = 2880;
}

public class APICallSettings
{
    public string APIUrl { get; set; } = string.Empty;
    public string XMLReq { get; set; } = string.Empty;
    public int ReqInterval { get; set; }
}