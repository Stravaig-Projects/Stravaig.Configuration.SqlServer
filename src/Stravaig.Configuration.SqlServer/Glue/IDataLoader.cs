using System.Collections.Generic;

namespace Stravaig.Configuration.SqlServer.Glue;

internal interface IDataLoader
{
    IDictionary<string, string> RetrieveData(SqlServerConfigurationSource source);
}