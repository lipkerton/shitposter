namespace listener.Domain.Configuration;

public class APISettings
{
    public string XMLRequestsFolder { get; set; } = string.Empty;
    public EndpointSettings openSessionSettings { get; set; } = new();
    public EndpointSettings getRealtimeNewsByProduct { get; set; } = new();
    public EndpointSettings getEntireNewsByID { get; set; } = new();
}

public class EndpointSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string XML { get; set; } = string.Empty;
    public int Interval { get; set; }
}