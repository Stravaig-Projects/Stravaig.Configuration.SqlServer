using System;
using System.Collections.Generic;
using System.Linq;

namespace Stravaig.Configuration.SqlServer.Tests;

public class FakeDataLoader : IDataLoader
{
    public KeyValuePair<string, string>[] FakeData { get; set; } = Array.Empty<KeyValuePair<string, string>>();
    
    public IDictionary<string, string> RetrieveData(SqlServerConfigurationSource source)
    {
        return FakeData.ToDictionary(k => k.Key, v => v.Value);
    }
}