using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Stravaig.Configuration.SqlServer.Glue;

namespace Stravaig.Configuration.SqlServer;

public class SqlServerConfigurationProvider : ConfigurationProvider
{
    private ILogger<SqlServerConfigurationProvider> _logger;
    private readonly Lazy<string> _toStringValue;
    private readonly SqlServerConfigurationSource _source;
    private readonly IDataLoader _dataLoader;
    private readonly ISqlServerConfigurationWatcher _watcher;

    public SqlServerConfigurationProvider(SqlServerConfigurationSource source)
        : this (source, new DataLoader(), NullSqlServerConfigurationWatcher.Instance, CreateLogger(source))
    {
        _watcher = CreateWatcher(source, this);
    }

    internal SqlServerConfigurationProvider(
        SqlServerConfigurationSource source,
        IDataLoader dataLoader,
        ISqlServerConfigurationWatcher watcher,
        ILogger<SqlServerConfigurationProvider> logger)
    {
        _toStringValue = new Lazy<string>(BuildToStringValue);
        _source = source;
        _dataLoader = dataLoader;
        _logger = logger;
        _watcher = watcher;
        _watcher.AttachLogger(_logger);
    }

    private static ILogger<SqlServerConfigurationProvider> CreateLogger(SqlServerConfigurationSource source)
    {
        return source.ExpectLogger
            ? new ReplayLogger<SqlServerConfigurationProvider>()
            : NullLogger<SqlServerConfigurationProvider>.Instance;
    }

    private static ISqlServerConfigurationWatcher CreateWatcher(SqlServerConfigurationSource source, SqlServerConfigurationProvider provider)
    {
        return source.RefreshInterval == TimeSpan.Zero
            ? NullSqlServerConfigurationWatcher.Instance
            : new SqlServerConfigurationWatcher(source.RefreshInterval, provider);
    }

    public override void Load()
    {
        try
        {
            _logger.LoadingConfigurationData();
            Data = _dataLoader.RetrieveData(_source);
        }
        catch (Exception ex)
        {
            _logger.FailedToGetConfigurationData(ex, _source.SchemaName, _source.TableName, _source.DatabaseName, _source.ServerName, ex.Message);
        }
        _watcher.EnsureStarted();
    }
    
    internal void Reload()
    {
        // Get existing data
        var oldData = Data;
        Load();

        if (DataDifferent(oldData, Data))
        {
            _logger.DetectedDifferences();
            OnReload();
        }
    }

    private bool DataDifferent(IDictionary<string, string> oldData, IDictionary<string, string> newData)
    {
        if (oldData.Count != newData.Count)
            return true;

        foreach (var oldKey in oldData.Keys)
        {
            if (!newData.ContainsKey(oldKey))
                return true;
            if (oldData[oldKey] != newData[oldKey])
                return true;
        }

        return false;
    }

    public override string ToString()
    {
        return _toStringValue.Value;
    }
    
    private string BuildToStringValue()
    {
        try
        {
            return $"{nameof(SqlServerConfigurationProvider)} ([{_source.ServerName}].[{_source.DatabaseName}].[{_source.SchemaName}].[{_source.TableName}])";
        }
        catch (Exception ex)
        {
            return $"{nameof(SqlServerConfigurationProvider)} ({ex.Message})";
        }
    }

    internal void AttachLogger(ILoggerFactory loggerFactory)
    {
        var replayLogger = _logger as ReplayLogger<SqlServerConfigurationProvider>;
        _logger = loggerFactory.CreateLogger<SqlServerConfigurationProvider>();
        _watcher.AttachLogger(_logger);
        replayLogger?.Replay(_logger);
    }
}