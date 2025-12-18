namespace SqlDataAnonymizer.Api.Contracts;

public sealed record HealthResponse
{
    public string Status { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public string Version { get; init; } = string.Empty;
    public string[] SupportedDatabases { get; init; } = [];
}