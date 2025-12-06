using SqlDataAnonymizer.Domain.Interfaces;

namespace SqlDataAnonymizer.Infrastructure.Strategies;

public sealed class TelefoneAnonymizationStrategy : IAnonymizationStrategy
{
    private static readonly Random Random = new();
    private static readonly int[] ValidDDDs = 
    { 
        11, 12, 13, 14, 15, 16, 17, 18, 19,
        21, 22, 24,
        27, 28,
        31, 32, 33, 34, 35, 37, 38,
        41, 42, 43, 44, 45, 46,
        47, 48, 49,
        51, 53, 54, 55,
        61,
        62, 64,
        63,
        65, 66,
        67,
        68,
        69,
        71, 73, 74, 75, 77,
        79,
        81, 87,
        82,
        83,
        84,
        85, 88,
        86, 89,
        91, 93, 94,
        92, 97,
        95,
        96,
        98, 99
    };

    public string Type => "telefone";

    public string Anonymize(string value)
    {
        var ddd = ValidDDDs[Random.Next(ValidDDDs. Length)];
        var isMobile = Random.Next(2) == 0;

        if (isMobile)
        {
            return GenerateMobileNumber(ddd);
        }

        return GenerateLandlineNumber(ddd);
    }

    private static string GenerateMobileNumber(int ddd)
    {
        var firstDigit = 9;
        var block1 = Random.Next(1000, 9999);
        var block2 = Random.Next(1000, 9999);

        return $"({ddd:D2}) {firstDigit}{block1:D4}-{block2:D4}";
    }

    private static string GenerateLandlineNumber(int ddd)
    {
        var firstDigit = Random.Next(2, 6);
        var block1 = Random.Next(100, 999);
        var block2 = Random.Next(1000, 9999);

        return $"({ddd:D2}) {firstDigit}{block1:D3}-{block2:D4}";
    }
}