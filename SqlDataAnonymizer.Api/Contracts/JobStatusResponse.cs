namespace SqlDataAnonymizer.Api.Contracts;

/// <summary>
/// Status detalhado de um job
/// </summary>
public sealed record JobStatusResponse
{
    /// <summary>
    /// ID do job
    /// </summary>
    public Guid JobId { get; init; }

    /// <summary>
    /// Status atual
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Tipo do banco de dados
    /// </summary>
    public string DatabaseType { get; init; } = string.Empty;

    /// <summary>
    /// Servidor
    /// </summary>
    public string Server { get; init; } = string.Empty;

    /// <summary>
    /// Banco de dados
    /// </summary>
    public string Database { get; init; } = string.Empty;

    /// <summary>
    /// Data/hora de início
    /// </summary>
    public DateTime StartedAt { get; init; }

    /// <summary>
    /// Data/hora de conclusão
    /// </summary>
    public DateTime? CompletedAt { get; init; }
    
    /// <summary>
    /// Logs do processamento (últimos 50)
    /// </summary>
    public List<string> Logs { get; init; } = new();
    
    /// <summary>
    /// Mensagem de erro (se houver)
    /// </summary>
    public string? ErrorMessage { get; init; }
}