using System;
using System.Collections.Generic;

namespace Stravaig.Configuration.SqlServer.Tests;

public class FakeDataLoader : IDataLoader
{
    public KeyValuePair<string, string>[] FakeData { get; set; } = Array.Empty<KeyValuePair<string, string>>();
    public IEnumerable<KeyValuePair<string, string>> RetrieveData(SqlServerConfigurationSource source)
    {
        return FakeData;
    }
}