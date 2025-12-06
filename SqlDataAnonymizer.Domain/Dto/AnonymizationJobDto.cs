using SqlDataAnonymizer.Domain.Enums;

namespace SqlDataAnonymizer.Domain.DTO;

public class AnonymizationJobDto
{
    public Guid JobId { get; set; }
    public string Server { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public DatabaseType DatabaseType { get; set; }
    public string Status { get; set; } = "Queued";
    public DateTime StartedAt { get; set; }
    public DateTime?  CompletedAt { get; set; }
    public List<string> Logs { get; set; } = new();
    public string?  ErrorMessage { get; set; }
}