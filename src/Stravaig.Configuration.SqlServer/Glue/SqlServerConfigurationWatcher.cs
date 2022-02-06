using System;
using System.Timers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Stravaig.Configuration.SqlServer.Glue;

internal class SqlServerConfigurationWatcher : ISqlServerConfigurationWatcher
{
    private readonly SqlServerConfigurationProvider _provider;
    private readonly Timer _timer;
    private ILogger _logger;
    
    public SqlServerConfigurationWatcher(TimeSpan interval, SqlServerConfigurationProvider provider, ILogger logger)
    {
        _logger = logger;
        _provider = provider;
        _timer = new Timer(interval.TotalMilliseconds);
        _timer.Elapsed += TimerOnElapsed;
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            _logger.PollingDatabase();
            _provider.Reload();
        }
        catch (Exception ex)
        {
            _logger.RefreshFailed(ex, ex.Message);
        }
    }

    public void EnsureStarted()
    {
        if (!_timer.Enabled)
        {
            _logger.StartingDbPolling(_timer.Interval / 1000.0);
            _timer.Start();
        }
    }

    public void AttachLogger(ILogger logger)
    {
        _logger = logger;
    }
}