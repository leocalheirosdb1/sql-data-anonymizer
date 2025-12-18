namespace SqlDataAnonymizer.Api.Contracts;

public sealed record ErrorResponse
{
    public string Error { get; init; } = string.Empty;
    
    public string[]? SupportedTypes { get; init; }
}