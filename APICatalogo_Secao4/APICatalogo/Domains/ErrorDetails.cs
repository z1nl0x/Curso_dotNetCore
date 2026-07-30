namespace APICatalogo.Domains;

public class ErrorDetails
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public string? Trace { get; set; }
}