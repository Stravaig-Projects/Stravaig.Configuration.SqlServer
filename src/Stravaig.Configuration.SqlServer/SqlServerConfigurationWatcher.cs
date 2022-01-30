using System;
using System.Timers;

namespace Stravaig.Configuration.SqlServer;

public class SqlServerConfigurationWatcher : ISqlServerConfigurationWatcher
{
    private readonly SqlServerConfigurationProvider _provider;
    private readonly Timer _timer;
    
    public SqlServerConfigurationWatcher(TimeSpan interval, SqlServerConfigurationProvider provider)
    {
        _provider = provider;
        _timer = new Timer(interval.TotalMilliseconds);
        _timer.Elapsed += TimerOnElapsed;
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            Console.WriteLine("Timer elapsed");
            _provider.Load();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            // How do we log this? How do we get the logger in here?
        }
    }

    public void EnsureStarted()
    {
        if (!_timer.Enabled)
            _timer.Start();
    }
    
}