namespace SqlDataAnonymizer.Infrastructure.Repositories.Models;

internal sealed record BatchData(List<IDictionary<string, object>> Records)
{
    public int Count => Records.Count;
    public bool IsEmpty => Count == 0;
}