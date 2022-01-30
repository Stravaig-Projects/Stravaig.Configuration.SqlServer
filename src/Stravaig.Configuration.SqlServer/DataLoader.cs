using System.Collections.Generic;
using System.Data.SqlClient;

namespace Stravaig.Configuration.SqlServer;

public interface IDataLoader
{
    IEnumerable<KeyValuePair<string, string>> RetrieveData(SqlServerConfigurationSource source);
}
public class DataLoader : IDataLoader
{
    private const string RetrieveSqlTemplate = "SELECT [ConfigKey], [ConfigValue] FROM [{0}].[{1}]";
    private const int keyColumnPosition = 0;
    private const int valueColumnPosition = 1;

    public IEnumerable<KeyValuePair<string, string>> RetrieveData(SqlServerConfigurationSource source)
    {
        var sql = RetrieveSql(source.SchemaName, source.TableName);
        using SqlConnection connection = new SqlConnection(source.ConnectionString);
        connection.Open();
        var cmd = new SqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var key = reader.GetString(keyColumnPosition);
            var value = reader.GetString(valueColumnPosition);
            yield return new KeyValuePair<string, string>(key, value);
        }
    }

    private string RetrieveSql(string schemaName, string tableName) =>
        string.Format(RetrieveSqlTemplate, schemaName, tableName);
}