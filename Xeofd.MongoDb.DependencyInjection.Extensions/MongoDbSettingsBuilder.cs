using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Xeofd.MongoDb.DependencyInjection.Extensions;

public class MongoDbSettingsBuilder(IOptions<MongoDbOptions> options)
{
    public MongoClientSettings Build(Action<MongoClientSettings>? configureSettings = null)
    {
        var buildFormsDbMongoUri = new MongoUrlBuilder(options.Value.ConnectionString);

        if (options.Value.Password is { Length: > 0 } password)
        {
            buildFormsDbMongoUri.Password = password;
        }

        var settings = MongoClientSettings.FromUrl(buildFormsDbMongoUri.ToMongoUrl());

        configureSettings?.Invoke(settings);

        return settings;
    }
}