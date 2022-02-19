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
            _logger.LogInformation("Change Detected. {S}", s);
        });
    }

    private async void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        await LogCurrentStateAsync();
    }

    private async Task LogCurrentStateAsync()
    {
        var jsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };
        string json = JsonSerializer.Serialize(_featureValues.CurrentValue, jsonOptions);
        Console.WriteLine(Environment.NewLine);
        _logger.LogInformation(
            "The object looks like:\n{Json}",
            json);
        await foreach (string feature in _featureManager.GetFeatureNamesAsync())
        {
            var state = await _featureManager.IsEnabledAsync(feature);
            _logger.LogInformation("{Feature} : {State}", feature, state);
        }

        _logger.LogConfigurationValuesAsInformation(_configRoot.GetSection("MyConfiguration"));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("The hosted service is starting.");
        _logger.LogProvidersAsInformation(_configRoot);

        await LogCurrentStateAsync();
        _timer.Enabled = true;
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