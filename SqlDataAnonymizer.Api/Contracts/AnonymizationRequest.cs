using System.ComponentModel.DataAnnotations;

namespace SqlDataAnonymizer.Api.Contracts;

public sealed record AnonymizationRequest
{
    [Required(ErrorMessage = "Servidor é obrigatório")]
    public string Servidor { get; init; } = string.Empty;
    
    [Required(ErrorMessage = "Banco é obrigatório")]
    public string Banco { get; init; } = string.Empty;
    
    [Required(ErrorMessage = "TipoBanco é obrigatório")]
    public string TipoBanco { get; init; } = string.Empty;
}