using Bogus;
using Bogus.Extensions.Brazil;
using SqlDataAnonymizer.Domain.Interfaces;

namespace SqlDataAnonymizer.Application.Strategies;

public sealed class CpfAnonymizationStrategy : IAnonymizationStrategy
{
    private readonly Faker _faker;

    public CpfAnonymizationStrategy()
    {
        _faker = new Faker("pt_BR");
    }

    public string Type => "cpf";

    public string Anonymize()
    {
        return _faker.Person.Cpf(includeFormatSymbols: true);
    }
}