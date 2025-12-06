using System.ComponentModel.DataAnnotations;

namespace SqlDataAnonymizer.Api.Contracts;

/// <summary>
/// Requisição para iniciar anonimização
/// </summary>
public sealed record AnonymizationRequest
{
    /// <summary>
    /// Nome ou IP do servidor do banco de dados
    /// </summary>
    /// <example>localhost</example>
    [Required(ErrorMessage = "Servidor é obrigatório")]
    public string Servidor { get; init; } = string.Empty;

    /// <summary>
    /// Nome do banco de dados
    /// </summary>
    /// <example>MeuBancoDados</example>
    [Required(ErrorMessage = "Banco é obrigatório")]
    public string Banco { get; init; } = string.Empty;

    /// <summary>
    /// Tipo do banco de dados
    /// </summary>
    /// <example>SqlServer</example>
    [Required(ErrorMessage = "TipoBanco é obrigatório")]
    public string TipoBanco { get; init; } = string.Empty;
}