using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Stravaig.Configuration.Diagnostics.Logging;
using Timer = System.Timers.Timer;


namespace Example;

public class TheHostedService : IHostedService, IDisposable
{
    private readonly ILogger<TheHostedService> _logger;
    private readonly IConfigurationRoot _configRoot;
    private readonly IFeatureManager _featureManager;
    private readonly IOptionsMonitor<MyFeatureConfiguration> _featureValues;
    private readonly Timer _timer;
    
    public TheHostedService(
        ILogger<TheHostedService> logger,
        IConfigurationRoot configRoot,
        IFeatureManager featureManager,
        IOptionsMonitor<MyFeatureConfiguration> featureValues)
    {
        _logger = logger;
        _configRoot = configRoot;
        _featureManager = featureManager;
        _featureValues = featureValues;
        _timer = new Timer(10000);
        _timer.Elapsed += TimerOnElapsed;
        featureValues.OnChange((_, s) =>
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Change Detected. {s}");
        });
    }

    private async void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        var jsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };
        string json = JsonSerializer.Serialize(_featureValues.CurrentValue, jsonOptions);
        Console.Clear();
        _logger.LogInformation(
            "At {Time} the object looks like:\n{Json}",
            e.SignalTime,
            json);
        await foreach (string feature in _featureManager.GetFeatureNamesAsync())
        {
            var state = await _featureManager.IsEnabledAsync(feature);
            _logger.LogInformation("{Feature} : {State}", feature, state);
        }
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