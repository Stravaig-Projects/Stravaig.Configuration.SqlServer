using System;
using Microsoft.Extensions.Logging;

namespace Stravaig.Configuration.SqlServer.Glue;

public static partial class Log
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Polling the database for SQL Server Configuration changes.")]
    public static partial void PollingDatabase(this ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "Refreshing the configuration from the database failed. {exceptionMessage}")]
    public static partial void RefreshFailed(this ILogger logger, Exception ex, string exceptionMessage);
    
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Loading configuration data from SQL Server.")]
    public static partial void LoadingConfigurationData(this ILogger logger);
    
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Warning,
        Message = "Failed to get configuration information from the table [{schemaName}].[{tableName}] in the database [{databaseName}] on the server [{server}]. {exceptionMessage}")]
    public static partial void FailedToGetConfigurationData(this ILogger logger, Exception ex, string schemaName, string tableName, string databaseName, string server, string exceptionMessage);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Debug,
        Message = "Detected differences in configuration data. Propagating changes.")]
    public static partial void DetectedDifferences(this ILogger logger);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Information,
        Message = "Replaying {Count} delayed logs. NOTE: Some replayed logs may not emit if filters prevent them.")]
    public static partial void ReplayStart(this ILogger logger, int count);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Information,
        Message = "End of replay.")]
    public static partial void ReplayEnd(this ILogger logger);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Debug,
        Message = "Starting SQL Server Configuration DB Polling every {frequency} seconds.")]
    public static partial void StartingDbPolling(this ILogger logger, double frequency);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Warning,
        Message = "The replay limit was reached. There are {excessCount} logs that cannot be replayed.")]
    public static partial void ReplayLimitExceeded(this ILogger logger, int excessCount);
}