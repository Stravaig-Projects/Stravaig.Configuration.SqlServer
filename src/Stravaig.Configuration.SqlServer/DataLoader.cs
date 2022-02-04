using System.Collections.Generic;
using System.Data.SqlClient;

namespace Stravaig.Configuration.SqlServer;

public class DataLoader : IDataLoader
{
    private const string RetrieveSqlTemplate = "SELECT [ConfigKey], [ConfigValue] FROM [{0}].[{1}]";
    private const int KeyColumnPosition = 0;
    private const int ValueColumnPosition = 1;

    public IDictionary<string, string> RetrieveData(SqlServerConfigurationSource source)
    {
        var sql = RetrieveSql(source.SchemaName, source.TableName);
        using SqlConnection connection = new SqlConnection(source.ConnectionString);
        connection.Open();
        var cmd = new SqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();
        return MaterialiseData(reader);
    }

    private static IDictionary<string, string> MaterialiseData(SqlDataReader reader)
    {
        Dictionary<string, string> results = new ();
        while (reader.Read())
        {
            var key = reader.GetString(KeyColumnPosition);
            var value = reader.GetString(ValueColumnPosition);
            results.Add(key,value);
        }

        return results;
    }

    private string RetrieveSql(string schemaName, string tableName) =>
        string.Format(RetrieveSqlTemplate, schemaName, tableName);
}