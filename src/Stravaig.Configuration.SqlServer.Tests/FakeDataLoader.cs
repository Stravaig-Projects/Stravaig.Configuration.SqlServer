using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Stravaig.Configuration.SqlServer.Glue;

namespace Stravaig.Configuration.SqlServer.Tests;

public class FakeDataLoader : IDataLoader
{
    public KeyValuePair<string, string>[] FakeData { get; set; } = Array.Empty<KeyValuePair<string, string>>();
    public Exception? ThrowOnRetrieveData { get; set; }
    
    public IDictionary<string, string> RetrieveData(SqlServerConfigurationSource source)
    {
        if (ThrowOnRetrieveData != null)
            throw ThrowOnRetrieveData;
        
        return FakeData.ToDictionary(k => k.Key, v => v.Value);
    }
}

public class FakeSqlServerConfigurationWatcher : ISqlServerConfigurationWatcher
{
    public void EnsureStarted()
    {
    }

    public void AttachLogger(ILogger logger)
    {
    }

    public void AttachProvider(SqlServerConfigurationProvider provider)
    {
    }

    public void Dispose()
    {
    }
}