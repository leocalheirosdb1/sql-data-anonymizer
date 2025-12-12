using Bogus;
using SqlDataAnonymizer.Domain.Interfaces;

namespace SqlDataAnonymizer.Application.Strategies;

public sealed class EmailAnonymizationStrategy : IAnonymizationStrategy
{
    private readonly Faker _faker;

    public EmailAnonymizationStrategy()
    {
        _faker = new Faker("pt_BR");
    }

    public string Type => "email";

    public string Anonymize()
    {
        return _faker.Internet.Email().ToLower();
    }
}