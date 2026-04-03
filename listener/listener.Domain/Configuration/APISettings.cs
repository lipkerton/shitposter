namespace listener.listener.Domain.Configuration;

public class APISettings
{
    public APICallSettings openSessionSettings { get; set; }
    public APICallSettings getRealtimeNewsByProduct { get; set; }
    public APICallSettings getEntireNewsByID { get; set; }
    public string requestPath { get; set; }
    public int cleanupInterval { get; set; }
}

public class APICallSettings
{
    public string APIUrl { get; set; }
    public string XMLReq { get; set; }
    public int ReqInterval { get; set; }
}