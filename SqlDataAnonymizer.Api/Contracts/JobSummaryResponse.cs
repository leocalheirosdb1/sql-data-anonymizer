namespace SqlDataAnonymizer.Api.Contracts;

public sealed record JobSummaryResponse
{
    public Guid JobId { get; init; }
    public string Status { get; init; } = string. Empty;
    public string DatabaseType { get; init; } = string.Empty;
    public string Server { get; init; } = string.Empty;
    public string Database { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}