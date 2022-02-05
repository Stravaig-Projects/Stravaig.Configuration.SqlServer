using Microsoft.Extensions.Logging;

namespace Stravaig.Configuration.SqlServer.Glue;

internal class NullSqlServerConfigurationWatcher : ISqlServerConfigurationWatcher
{
    public void EnsureStarted()
    {
    }

    public void AttachLogger(ILogger logger)
    {
    }
}