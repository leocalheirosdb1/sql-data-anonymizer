using Bogus;
using SqlDataAnonymizer.Domain.Interfaces;

namespace SqlDataAnonymizer.Application.Strategies;

public sealed class TelefoneAnonymizationStrategy : IAnonymizationStrategy
{
    private readonly Faker _faker;

    public TelefoneAnonymizationStrategy()
    {
        _faker = new Faker("pt_BR");
    }

    public string Type => "telefone";

    public string Anonymize()
    {
        return _faker.Phone.PhoneNumber("(##) 9####-####");
    }
}