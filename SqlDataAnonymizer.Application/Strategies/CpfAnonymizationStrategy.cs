using Bogus;
using Bogus.Extensions.Brazil;
using SqlDataAnonymizer.Domain.Interfaces;

namespace SqlDataAnonymizer.Application.Strategies;

public sealed class CpfAnonymizationStrategy : IAnonymizationStrategy
{
    public string Type => "cpf";
    
    public string Anonymize()
    {
        var faker = new Faker("pt_BR");
        faker.Random = new Randomizer(Guid.NewGuid().GetHashCode());

        return faker.Person.Cpf(includeFormatSymbols: true);
    }
}