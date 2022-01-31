using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Stravaig.Configuration.SqlServer;

public class SqlServerConfigurationProvider : ConfigurationProvider
{
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
        var data = _dataLoader.RetrieveData(_source);
        Data = new Dictionary<string, string>(data, StringComparer.OrdinalIgnoreCase);
        _watcher.EnsureStarted();
    }
    
    internal void Reload()
    {
        Load();
        OnReload();
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
}