using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Stravaig.Configuration.SqlServer;

public class SqlServerConfigurationSource : IConfigurationSource
{
    Regex SqlNameRegex => new Regex(@"^[\p{L}_][\p{L}\p{N}@$#_]{0,127}$", RegexOptions.None, TimeSpan.FromMilliseconds(1000));
    public string ConnectionString { get; private set; }
    public string SchemaName { get; private set; }
    public string TableName { get; private set; }
    public TimeSpan RefreshInterval { get; private set; }
    public bool ExpectLogger { get; private set; }

    public SqlServerConfigurationSource(string connectionString, bool expectLogger = false, TimeSpan? refreshInterval = null, string schemaName = "Stravaig", string tableName = "AppConfiguration")
    {
        ValidateConnectionString(connectionString);
        ValidateSchemaName(schemaName);
        ValidateTableName(tableName);
        ExpectLogger = expectLogger;
        ConnectionString = connectionString;
        SchemaName = schemaName;
        TableName = tableName;
        RefreshInterval = refreshInterval ?? TimeSpan.Zero;
    }

    private void ValidateTableName(string tableName)
    {
        if (!SqlNameRegex.IsMatch(tableName))
            throw new ArgumentException(
                "The table name does not appear to be a valid SQL Server Table Name",
                nameof(tableName));
    }

    private void ValidateSchemaName(string schemaName)
    {
        if (!SqlNameRegex.IsMatch(schemaName))
            throw new ArgumentException(
                "The schema name does not appear to be a valid SQL Server Schema Name",
                nameof(schemaName));
    }

    private void ValidateConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("The connection string cannot be null, empty or white space.",
                nameof(connectionString));

        _ = new SqlConnectionStringBuilder(connectionString);
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new SqlServerConfigurationProvider(this);
    }
}