using SqlDataAnonymizer.Domain.DTO;

namespace SqlDataAnonymizer.Domain.Interfaces;

public interface ISensitiveColumnRepository
{
    Task<List<SensitiveColumnDto>> GetSensitiveColumnsAsync(
        string connectionString,
        IDatabaseProvider provider);
}