# Stravaig Configuration SQL Server Provider

A configuration provider that uses SQL Server as its backing store.

* [![Stravaig Configuration SQL Server Provider](https://github.com/Stravaig-Projects/Stravaig.Configuration.SqlServer/actions/workflows/build.yml/badge.svg)](https://github.com/Stravaig-Projects/Stravaig.Configuration.SqlServer/actions/workflows/build.yml)
* ![Nuget](https://img.shields.io/nuget/v/Stravaig.Configuration.SqlServer?color=004880&label=nuget%20stable&logo=nuget) [View on NuGet](https://www.nuget.org/packages/Stravaig.Configuration.SqlServer)
* ![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Stravaig.Configuration.SqlServer?color=ffffff&label=nuget%20latest&logo=nuget) [View on NuGet](https://www.nuget.org/packages/Stravaig.ConfigurationSqlServer)

---

## Set up

The simplest way is to configure the SQL Server configuration from the existing configuration up to that point. Any configuration providers added after the call to `AddSqlServer` will not have their data alter the way that the SQL Server configuration provider is set up.

```csharp
await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddSqlServer(opts =>
        {
            // Will pull connection string, schema and table names from the 
            // configuration system up to this point.
            opts.FromExistingConfiguration();
        });
    })
```

with the corresponding information in `appsettings.json`, e.g.:

```json
{
    "ConnectionStrings": {
        "ConfigDB": "*** Found in Secret Store ***"
    },
    "Stravaig": {
        "AppConfiguration": {
            "SchemaName": "Stravaig",
            "TableName": "AppConfiguration",
            "RefreshSeconds": 90,
            "ConnectionStringName": "ConfigDB",
            "CommandTimeout": 10
        }
    }
}
```

The default configuration section used is `Stravaig.AppSettings`, however, this can be changed to what ever you prefer by passing in the path to the configuration section you prefer. e.g.

```csharp
builder.AddSqlServer(opts =>
{
    // Will pull connection string, schema and table names from the 
    // configuration system at the specified config section.
    opts.FromExistingConfiguration("MyApp:SqlConfiguration");
});
```

### Adding logging

The configuration provider is set up too early in the pipeline to be able to accept logging from the start. However you can attach a logger after it has been set up. The configuration provider will also hold on to any log messages and replay them once a logger it attached. It works like this:

* When calling `AddSqlServer` on the configuration builder. Make sure you call `ExpectLogger()`. This sets up the replay mechanism.
* Then set up your loggers. The SqlServerConfigurationProvider logs from Debug up.
* By the time you get to configure services, you should be able to get a `ILoggerFactory`. There is an extension method, `AttachLoggerToSqlServerProvider` on `IConfigurationRoot` that will attach the logger.

```csharp
await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddSqlServer(opts =>
        {
            opts.FromExistingConfiguration()
                .ExpectLogger();
        });
    })
    .ConfigureLogging(builder =>
    {
        builder.AddConsole();
        builder.AddDebug();
        builder.SetMinimumLevel(LogLevel.Debug);
    })
    .ConfigureServices((ctx, services) =>
    {
        // Attach the logger to the SQL Server Configuration Provider
        IConfigurationRoot configRoot = (IConfigurationRoot) ctx.Configuration;
        configRoot.AttachLoggerToSqlServerProvider(services.BuildServiceProvider().GetService<ILoggerFactory>());
    })

```

CAUTION: If you call `ExpectLogger` and never attach a logger, it will just hold on to the log messages for the lifetime of the application in memory.

## Contributing / Getting Started

* Ensure you have PowerShell 7.1.x or higher installed
* At a PowerShell prompt
    * Navigate to the root of this repository
    * Run `./Install-GitHooks.ps1`
