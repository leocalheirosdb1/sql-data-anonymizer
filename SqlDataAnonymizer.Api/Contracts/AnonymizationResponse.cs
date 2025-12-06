namespace SqlDataAnonymizer.Api.Contracts;

/// <summary>
/// Resposta ao iniciar anonimização
/// </summary>
public sealed record AnonymizationResponse
{
    /// <summary>
    /// ID único do job
    /// </summary>
    public Guid JobId { get; init; }

    /// <summary>
    /// Status atual do job
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Mensagem descritiva
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Tipo do banco de dados
    /// </summary>
    public string DatabaseType { get; init; } = string. Empty;

    /// <summary>
    /// Servidor do banco
    /// </summary>
    public string Server { get; init; } = string.Empty;

    /// <summary>
    /// Nome do banco
    /// </summary>
    public string Database { get; init; } = string.Empty;
}