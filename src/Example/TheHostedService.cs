using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stravaig.Configuration.Diagnostics.Logging;
using Timer = System.Timers.Timer;


namespace Example;

public class TheHostedService : IHostedService, IDisposable
{
    private readonly ILogger<TheHostedService> _logger;
    private readonly IConfigurationRoot _configRoot;
    private readonly IOptions<MyFeatureConfiguration> _featureValues;
    private readonly Timer _timer;
    
    public TheHostedService(
        ILogger<TheHostedService> logger,
        IConfigurationRoot configRoot,
        IOptions<MyFeatureConfiguration> featureValues)
    {
        _logger = logger;
        _configRoot = configRoot;
        _featureValues = featureValues;
        _timer = new Timer(10000);
        _timer.Elapsed += TimerOnElapsed;
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        var jsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };
        string json = JsonSerializer.Serialize(_featureValues.Value, jsonOptions);
        Console.Clear();
        _logger.LogInformation(
            "At {Time} the object looks like:\n{Json}",
            e.SignalTime,
            json);
        _logger.LogConfigurationValuesAsInformation(_configRoot.GetSection("MyConfiguration"));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("The hosted service is starting.");
        _logger.LogProvidersAsInformation(_configRoot);
        _timer.Enabled = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Enabled = false;
        _logger.LogInformation("The hosted service is ending.");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}