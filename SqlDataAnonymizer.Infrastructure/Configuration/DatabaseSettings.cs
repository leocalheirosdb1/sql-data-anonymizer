namespace SqlDataAnonymizer.Infrastructure.Configuration;

public sealed class DatabaseSettings
{
    public string UserId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int ConnectionTimeout { get; set; } = 30;
    public bool TrustServerCertificate { get; set; } = true;
    public int BatchSize { get; set; } = 5000;
    public int Port { get; set; }
}