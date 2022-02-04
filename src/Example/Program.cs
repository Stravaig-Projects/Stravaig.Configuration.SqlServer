using Example;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Stravaig.Configuration.SqlServer;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddUserSecrets<Program>();
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
        IConfigurationRoot configRoot = (IConfigurationRoot) ctx.Configuration;
        configRoot.AttachLoggerToSqlServerProvider(services.BuildServiceProvider().GetService<ILoggerFactory>());
        services.AddTransient<IHostedService, TheHostedService>();
        services.AddSingleton(configRoot);
        services.Configure<MyFeatureConfiguration>(configRoot.GetSection("MyConfiguration"));
        services.AddFeatureManagement(configRoot.GetSection("FeatureManager"));
    })
    .RunConsoleAsync();
