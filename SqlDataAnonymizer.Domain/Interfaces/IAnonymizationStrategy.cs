namespace SqlDataAnonymizer.Domain.Interfaces;

public interface IAnonymizationStrategy
{
    string Type { get; }
    string Anonymize(string value);
}