using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Xeofd.MongoDb.DependencyInjection.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration,
        Action<MongoClientSettings>? configureSettings = null)
    {
        services.Configure<MongoDbOptions>(configuration.GetSection("MongoDb"));
        services.AddTransient<MongoDbSettingsBuilder>();
        services.AddSingleton<IMongoClient>(provider =>
            new MongoClient(provider.GetRequiredService<MongoDbSettingsBuilder>().Build(configureSettings)));
        services.AddTransient<IMongoDatabase>(s =>
            s.GetRequiredService<IMongoClient>()
                .GetDatabase(configuration.GetValue<string>("MongoDb:DatabaseName", "defaultDatabase")));

        return services;
    }
}