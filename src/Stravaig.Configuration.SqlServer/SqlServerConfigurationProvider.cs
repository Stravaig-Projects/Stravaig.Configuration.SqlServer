using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Stravaig.Configuration.SqlServer;

public class SqlServerConfigurationProvider : ConfigurationProvider
{
    private ILogger<SqlServerConfigurationProvider> _logger = NullLogger<SqlServerConfigurationProvider>.Instance;
    private readonly Lazy<string> _toStringValue;
    private readonly SqlServerConfigurationSource _source;
    private readonly IDataLoader _dataLoader;
    private readonly ISqlServerConfigurationWatcher _watcher;

    public SqlServerConfigurationProvider(SqlServerConfigurationSource source)
        : this (source, new DataLoader())
    {
    }

    internal SqlServerConfigurationProvider(SqlServerConfigurationSource source, IDataLoader dataLoader)
    {
        _source = source;
        _dataLoader = dataLoader;
        _toStringValue = new Lazy<string>(BuildToStringValue);
        _watcher = source.RefreshInterval == TimeSpan.Zero
            ? new NullSqlServerConfigurationWatcher()
            : new SqlServerConfigurationWatcher(source.RefreshInterval, this);
    }
    
    public override void Load()
    {
        try
        {
            var data = _dataLoader.RetrieveData(_source);
            Data = new Dictionary<string, string>(data, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to get configuration information from the database. {ExceptionMessage}",
                ex.Message);
        }
        _watcher.EnsureStarted();
    }
    
    internal void Reload()
    {
        // Get existing data
        var oldData = Data;
        Load();

        if (DataDifferent(oldData, Data))
            OnReload();
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
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(_source.ConnectionString);
            return $"{nameof(SqlServerConfigurationProvider)} ([{builder.DataSource}].[{builder.InitialCatalog}].[{_source.SchemaName}].[{_source.TableName}])";
        }
        catch (Exception ex)
        {
            return $"{nameof(SqlServerConfigurationProvider)} ({ex.Message})";
        }
    }

    internal void AttachLogger(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<SqlServerConfigurationProvider>();
    }
}