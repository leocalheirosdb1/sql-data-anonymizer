using System.Text;
using SqlDataAnonymizer.Domain.Interfaces;

namespace SqlDataAnonymizer.Infrastructure.Strategies;

public sealed class EmailAnonymizationStrategy : IAnonymizationStrategy
{
    private static readonly Random Random = new();
    private const string AllowedChars = "abcdefghijklmnopqrstuvwxyz0123456789";

    public string Type => "email";

    public string Anonymize(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || ! value.Contains('@'))
            return value;

        var parts = value.Split('@');
        if (parts.Length != 2)
            return value;

        var localLength = Math.Max(5, parts[0].Length);
        var anonymizedLocal = GenerateRandomString(localLength);

        return $"{anonymizedLocal}@{parts[1]}";
    }

    private static string GenerateRandomString(int length)
    {
        var sb = new StringBuilder(length);
        for (var i = 0; i < length; i++)
        {
            sb.Append(AllowedChars[Random.Next(AllowedChars.Length)]);
        }
        return sb. ToString();
    }
}