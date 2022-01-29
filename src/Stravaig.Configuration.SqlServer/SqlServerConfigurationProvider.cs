using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Stravaig.Configuration.SqlServer;

public class SqlServerConfigurationProvider : ConfigurationProvider
{
    private readonly SqlServerConfigurationSource _source;
    private readonly IDataLoader _dataLoader;

    public SqlServerConfigurationProvider(SqlServerConfigurationSource source)
        : this (source, new DataLoader())
    {
    }

    internal SqlServerConfigurationProvider(SqlServerConfigurationSource source, IDataLoader dataLoader)
    {
        _source = source;
        _dataLoader = dataLoader;
    }
    
    public override void Load()
    {
        var data = _dataLoader.RetrieveData(_source);
        Data = new Dictionary<string, string>(data, StringComparer.OrdinalIgnoreCase);
    }
}