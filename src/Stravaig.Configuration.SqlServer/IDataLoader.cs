using System.Collections.Generic;

namespace Stravaig.Configuration.SqlServer;

public interface IDataLoader
{
    IDictionary<string, string> RetrieveData(SqlServerConfigurationSource source);
}