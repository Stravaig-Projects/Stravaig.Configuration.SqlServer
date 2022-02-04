using Microsoft.Extensions.Logging;

namespace Stravaig.Configuration.SqlServer;

public class NullSqlServerConfigurationWatcher : ISqlServerConfigurationWatcher
{
    public void EnsureStarted()
    {
    }

    public void AttachLogger(ILogger logger)
    {
    }
}