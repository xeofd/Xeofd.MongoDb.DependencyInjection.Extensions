namespace Xeofd.MongoDb.DependencyInjection.Extensions;

public class MongoDbOptions
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string? Password { get; set; }
}