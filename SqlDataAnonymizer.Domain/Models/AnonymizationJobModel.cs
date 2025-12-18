using System.Collections.Concurrent;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SqlDataAnonymizer.Domain.Enums;

namespace SqlDataAnonymizer.Domain.Models;

public class AnonymizationJobModel
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid JobId { get; set; }
    
    [BsonElement("server")]
    public string Server { get; set; } = string.Empty;
    
    [BsonElement("database")]
    public string Database { get; set; } = string.Empty;
    
    [BsonElement("databaseType")]
    [BsonRepresentation(BsonType.String)]
    public DatabaseType DatabaseType { get; set; }
    
    [BsonElement("status")]
    public string Status { get; set; } = "Queued";
    
    [BsonElement("startedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind. Utc)]
    public DateTime StartedAt { get; set; }
    
    [BsonElement("completedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? CompletedAt { get; set; }
    
    [BsonElement("logs")]
    public List<string> Logs { get; set; } = new();
    
    [BsonElement("errorMessage")]
    public string? ErrorMessage { get; set; }
    
    [BsonIgnore]
    public ConcurrentBag<string> LogsBag
    {
        get => new(Logs);
        set => Logs = value.ToList();
    }
}