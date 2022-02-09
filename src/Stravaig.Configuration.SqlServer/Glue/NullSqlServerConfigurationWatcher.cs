using Microsoft.Extensions.Logging;

namespace Stravaig.Configuration.SqlServer.Glue;

internal class NullSqlServerConfigurationWatcher : ISqlServerConfigurationWatcher
{
    internal static readonly NullSqlServerConfigurationWatcher Instance = new ();
    public void EnsureStarted()
    {
    }

    public void AttachLogger(ILogger logger)
    {
    }
}