using Dapper;
using Microsoft.Extensions.Logging;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Interfaces;

namespace SqlDataAnonymizer.Infrastructure.Repositories;

public sealed class SensitiveColumnRepository : ISensitiveColumnRepository
{
    private readonly ILogger<SensitiveColumnRepository> _logger;

    public SensitiveColumnRepository(ILogger<SensitiveColumnRepository> logger)
    {
        _logger = logger;
    }

    public async Task<List<SensitiveColumnDto>> GetSensitiveColumnsAsync(
        string connectionString,
        IDatabaseProvider provider)
    {
        _logger.LogInformation("Iniciando detecção de colunas sensíveis usando {Provider}", provider.Type);
        
        using var connection = provider.CreateConnection(connectionString);
        var query = provider.GetSensitiveColumnsQuery();
        var results = await connection.QueryAsync<SensitiveColumnDto>(query);
        
        _logger.LogInformation("Encontradas {Count} colunas sensíveis", results.Count());
        
        return results.ToList();
    }
}