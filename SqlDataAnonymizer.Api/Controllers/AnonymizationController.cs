using Microsoft.AspNetCore.Mvc;
using SqlDataAnonymizer.Api.Contracts;
using SqlDataAnonymizer.Application.Services;
using SqlDataAnonymizer.Domain.Enums;
using SqlDataAnonymizer.Domain.Interfaces;

namespace SqlDataAnonymizer.Api.Controllers;

[ApiController]
[Route("api/anonimizar")]
[Produces("application/json")]
public sealed class AnonymizationController : ControllerBase
{
    private readonly IAnonymizationService _service;
    private readonly ILogger<AnonymizationController> _logger;

    public AnonymizationController(
        AnonymizationService service,
        ILogger<AnonymizationController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Inicia um processo de anonimização de dados
    /// </summary>
    /// <param name="request">Dados da requisição (Servidor, Banco, TipoBanco)</param>
    /// <returns>Informações do job criado</returns>
    /// <response code="202">Job criado com sucesso</response>
    /// <response code="400">Requisição inválida</response>
    [HttpPost]
    [ProducesResponseType(typeof(AnonymizationResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Start([FromBody] AnonymizationRequest request)
    {
        try
        {
            _logger.LogInformation("Requisição de anonimização recebida - Servidor: {Server}, Banco: {Database}, Tipo: {Type}", 
                request. Servidor, request.Banco, request.TipoBanco);

            if (!Enum.TryParse<DatabaseType>(request.TipoBanco, true, out var dbType))
            {
                _logger.LogWarning("Tipo de banco inválido: {Type}", request.TipoBanco);
                return BadRequest(new ErrorResponse
                {
                    Error = $"Tipo de banco '{request.TipoBanco}' não suportado",
                    SupportedTypes = new[] { nameof(DatabaseType.SqlServer), nameof(DatabaseType.Oracle), nameof(DatabaseType.MySql) }
                });
            }

            var jobId = await _service.StartAnonymizationAsync(
                request.Servidor, 
                request.Banco, 
                dbType);

            var response = new AnonymizationResponse
            {
                JobId = jobId,
                Status = "Processing",
                Message = $"Anonimização iniciada com sucesso no {dbType}",
                DatabaseType = dbType.ToString(),
                Server = request.Servidor,
                Database = request.Banco
            };

            _logger.LogInformation("Job {JobId} criado com sucesso", jobId);

            return AcceptedAtAction(
                nameof(GetStatus), 
                new { jobId }, 
                response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar anonimização");
            return BadRequest(new ErrorResponse 
            { 
                Error = ex.Message 
            });
        }
    }

    /// <summary>
    /// Consulta o status de um job de anonimização
    /// </summary>
    /// <param name="jobId">ID do job</param>
    /// <returns>Status detalhado do job</returns>
    /// <response code="200">Status do job</response>
    /// <response code="404">Job não encontrado</response>
    [HttpGet("{jobId}/status")]
    [ProducesResponseType(typeof(JobStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult GetStatus(Guid jobId)
    {
        _logger.LogDebug("Consultando status do job {JobId}", jobId);

        var job = _service.GetJobStatus(jobId);

        if (job == null)
        {
            _logger.LogWarning("Job {JobId} não encontrado", jobId);
            return NotFound(new ErrorResponse 
            { 
                Error = "Job não encontrado" 
            });
        }

        var response = new JobStatusResponse
        {
            JobId = job.JobId,
            Status = job.Status,
            DatabaseType = job.DatabaseType.ToString(),
            Server = job.Server,
            Database = job.Database,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            Logs = job.Logs.TakeLast(50).ToList(),
            ErrorMessage = job.ErrorMessage
        };

        return Ok(response);
    }
    
    /// <summary>
    /// Health check da API
    /// </summary>
    /// <returns>Status da API</returns>
    /// <response code="200">API está saudável</response>
    [HttpGet("/health")]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        return Ok(new HealthResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            SupportedDatabases = new[] { nameof(DatabaseType.SqlServer), nameof(DatabaseType.Oracle), nameof(DatabaseType.MySql) }
        });
    }
}