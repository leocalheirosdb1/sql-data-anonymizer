namespace SqlDataAnonymizer.Domain.Models;

public sealed record BatchData(List<IDictionary<string, object>> Records)
{
    public int Count => Records.Count;
    public bool IsEmpty => Count == 0;
}