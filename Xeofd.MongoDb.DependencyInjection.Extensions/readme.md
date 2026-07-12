# Xeofd.MongoDbExtension

A simple MongoDB client dependency injection package that wraps up the `MongoClient` and `MongoDatabase` setup for .NET applications.

## Installation

```
dotnet add package Xeofd.MongoDbExtension
```

## Configuration

Bind MongoDB settings under the `MongoDb` section in your configuration source (`appsettings.json`, environment variables, etc.):

```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://user@host:27017",
    "DatabaseName": "myDatabase",
    "Password": "optionalPasswordOverride"
  }
}
```

| Property         | Description                                                                                  |
| ---------------- | -------------------------------------------------------------------------------------------- |
| `ConnectionString` | The base MongoDB connection string.                                                       |
| `DatabaseName`     | The database to resolve when injecting `IMongoDatabase`. Falls back to `defaultDatabase`. |
| `Password`         | Optional. When set, overrides the password embedded in the connection string.             |

## Usage in ASP.NET Core

Register the MongoDB services on your `IServiceCollection`:

```csharp
builder.Services.AddMongoDb(builder.Configuration);
```

This injects:

- `IMongoClient` — a singleton `MongoClient` instance.
- `IMongoDatabase` — a transient database scoped to the configured `DatabaseName`.

Inject them wherever needed:

```csharp
public class MyRepository(IMongoDatabase database)
{
    private readonly IMongoCollection<MyDoc> _collection = database.GetCollection<MyDoc>("myCollection");

    public Task<List<MyDoc>> GetAllAsync() => _collection.Find(_ => true).ToListAsync();
}
```

## Customizing MongoClientSettings

Use the optional `configureSettings` callback to tweak the `MongoClientSettings` before the client is built (e.g. timeouts, cluster configuration, authentication):

```csharp
builder.Services.AddMongoDb(builder.Configuration, settings =>
{
    settings.ConnectTimeout = TimeSpan.FromSeconds(10);
    settings.SocketTimeout  = TimeSpan.FromSeconds(30);
    settings.MaxConnectionPoolSize = 200;
});
```

## Using with AWS Authentication

MongoDB supports AWS IAM authentication via the `MONGODB-AWS` auth mechanism. There are a few ways to wire this up with `AddMongoDb`.

### Option 1: Let the driver resolve AWS credentials

Add `authMechanism=MONGODB-AWS` to your connection string and let the MongoDB driver walk its built-in AWS credential provider chain (env vars, EC2/ECS instance metadata, profile, etc.):

```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://host:27017/?authMechanism=MONGODB-AWS&authSource=$external",
    "DatabaseName": "myDatabase"
  }
}
```

No callback is required — register as usual:

```csharp
builder.Services.AddMongoDb(builder.Configuration);
```

### Option 2: Use the MongoDB AWS authentication extension

Install the dedicated [MongoDB.Driver.Authentication.AWS](https://www.nuget.org/packages/MongoDB.Driver.Authentication.AWS) package, which registers an AWS credential provider with the driver. After setting up your AWS configuration (e.g. via the AWS SDK or your own AWS config extension), call `AddAWSAuthentication()` on the client settings inside the `configureSettings` callback:

```csharp
using MongoDB.Driver;
using MongoDB.Driver.Authentication.AWS;

builder.Services.AddMongoDb(builder.Configuration, settings =>
{
    settings.AddAWSAuthentication();
});
```

The connection string still needs the AWS auth mechanism so the driver knows to use it:

```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://host:27017/?authMechanism=MONGODB-AWS&authSource=$external",
    "DatabaseName": "myDatabase"
  }
}
```

### Option 3: Supply AWS credentials explicitly

If you'd rather pass AWS credentials in directly (e.g. from the AWS SDK or a named profile), configure AWS first, then hand the credentials to `MongoClientSettings` via the `configureSettings` callback:

```csharp
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using MongoDB.Driver;

// 1. Resolve AWS credentials via the AWS SDK (or your own AWS config extension)
var chain = new CredentialProfileStoreChain();
chain.TryGetAWSCredentials("my-profile", out var awsCredentials);
var creds = awsCredentials.GetCredentials();

// 2. Register MongoDb and inject the AWS credentials into the client settings
builder.Services.AddMongoDb(builder.Configuration, settings =>
{
    settings.Credential = MongoCredential.CreateFromAwsCredentials(
        accessKeyId:     creds.AccessKey,
        secretAccessKey: creds.SecretKey,
        sessionToken:    creds.Token);
});
```

> The connection string still needs `authMechanism=MONGODB-AWS&authSource=$external` for the credential to be applied against the correct auth source.

## License

See the repository for license information.
