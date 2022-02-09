using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Stravaig.Configuration.SqlServer.Glue;

internal static class SqlServerConfigurationSourceExtensions
{
    public static ILogger<SqlServerConfigurationProvider> CreateLogger(this SqlServerConfigurationSource source)
    {
        if (!source.ExpectLogger) 
            return NullLogger<SqlServerConfigurationProvider>.Instance;

        var logger = new ReplayLogger<SqlServerConfigurationProvider>();
        logger.InitialProviderDescription(
            source.ServerName,
            source.DatabaseName,
            source.SchemaName,
            source.TableName,
            frequency: source.RefreshInterval == TimeSpan.Zero ? "once only" : $"every {source.RefreshInterval.TotalSeconds} seconds",
            (int)source.ConnectionTimeout.TotalSeconds,
            (int)source.CommandTimeout.TotalSeconds);
        
        if (source.ConnectionTimeout + source.CommandTimeout > source.RefreshInterval)
            logger.WarnOfCycleInterleave(
                (int)source.RefreshInterval.TotalSeconds,
                (int)source.ConnectionTimeout.TotalSeconds,
                (int)source.CommandTimeout.TotalSeconds);
        return logger;

    }
}