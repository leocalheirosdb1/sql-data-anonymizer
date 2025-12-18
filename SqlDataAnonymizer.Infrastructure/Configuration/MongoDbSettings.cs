namespace SqlDataAnonymizer.Infrastructure.Configuration;

public sealed class MongoDbSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string JobsCollectionName { get; set; }
}