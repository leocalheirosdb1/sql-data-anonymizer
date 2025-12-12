namespace SqlDataAnonymizer.Infrastructure.Repositories.Models;

/// <summary>
/// Value Object que encapsula os dados de um batch de registros
/// </summary>
internal sealed record BatchData(List<IDictionary<string, object>> Records)
{
    public int Count => Records.Count;
    public bool IsEmpty => Count == 0;
}