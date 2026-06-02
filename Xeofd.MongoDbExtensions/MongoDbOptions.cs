namespace Xeofd.MongoDbExtension;

public class MongoDbOptions
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string? Password { get; set; }
}