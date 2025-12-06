namespace SqlDataAnonymizer.Api.Contracts;

/// <summary>
/// Resposta de erro
/// </summary>
public sealed record ErrorResponse
{
    /// <summary>
    /// Mensagem de erro
    /// </summary>
    public string Error { get; init; } = string.Empty;

    /// <summary>
    /// Tipos de banco suportados (opcional)
    /// </summary>
    public string[]? SupportedTypes { get; init; }
}