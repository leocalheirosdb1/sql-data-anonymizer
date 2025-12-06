using SqlDataAnonymizer.Domain.Interfaces;

namespace SqlDataAnonymizer.Infrastructure.Strategies;

public sealed class CpfAnonymizationStrategy : IAnonymizationStrategy
{
    private static readonly Random Random = new();
    private const int CpfBaseLength = 9;

    public string Type => "cpf";

    public string Anonymize(string value)
    {
        var numbers = GenerateBaseNumbers();
        var digit1 = CalculateFirstDigit(numbers);
        var digit2 = CalculateSecondDigit(numbers, digit1);

        return FormatCpf(numbers, digit1, digit2);
    }

    private static int[] GenerateBaseNumbers()
    {
        var numbers = new int[CpfBaseLength];
        
        for (var i = 0; i < CpfBaseLength; i++)
        {
            numbers[i] = Random.Next(0, 10);
        }
        
        if (numbers.Distinct().Count() == 1)
        {
            numbers[0] = (numbers[0] + 1) % 10;
        }

        return numbers;
    }

    private static int CalculateFirstDigit(int[] numbers)
    {
        var sum = 0;
        for (var i = 0; i < CpfBaseLength; i++)
        {
            sum += numbers[i] * (10 - i);
        }

        var remainder = sum % 11;
        return remainder < 2 ?  0 : 11 - remainder;
    }

    private static int CalculateSecondDigit(int[] numbers, int firstDigit)
    {
        var sum = 0;
        for (var i = 0; i < CpfBaseLength; i++)
        {
            sum += numbers[i] * (11 - i);
        }
        sum += firstDigit * 2;

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    private static string FormatCpf(int[] numbers, int digit1, int digit2)
    {
        return $"{numbers[0]}{numbers[1]}{numbers[2]}." +
               $"{numbers[3]}{numbers[4]}{numbers[5]}." +
               $"{numbers[6]}{numbers[7]}{numbers[8]}-" +
               $"{digit1}{digit2}";
    }
}