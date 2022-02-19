using System;
using Microsoft.Extensions.Logging;

namespace Stravaig.Configuration.SqlServer.Glue;

internal sealed class NullSqlServerConfigurationWatcher : ISqlServerConfigurationWatcher
{
    internal static readonly NullSqlServerConfigurationWatcher Instance = new ();
    public void EnsureStarted()
    {
    }

    public void AttachLogger(ILogger logger)
    {
    }

    public void Dispose()
    {
    }
}