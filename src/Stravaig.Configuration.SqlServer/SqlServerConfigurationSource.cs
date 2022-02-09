using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Stravaig.Configuration.SqlServer.Glue;

namespace Stravaig.Configuration.SqlServer;

public class SqlServerConfigurationSource : IConfigurationSource
{
    Regex SqlNameRegex => new Regex(@"^[\p{L}_][\p{L}\p{N}@$#_]{0,127}$", RegexOptions.None, TimeSpan.FromMilliseconds(1000));
    public string ConnectionString { get; private set; }
    public string SchemaName { get; private set; }
    public string TableName { get; private set; }
    public TimeSpan RefreshInterval { get; private set; }
    public bool ExpectLogger { get; private set; }
    
    public TimeSpan CommandTimeout { get; private set; }

    private readonly Lazy<string> _serverName;
    public string ServerName => _serverName.Value;

    private readonly Lazy<string> _databaseName;
    public string DatabaseName => _databaseName.Value;

    private readonly Lazy<TimeSpan> _connectionTimeout;
    public TimeSpan ConnectionTimeout => _connectionTimeout.Value;

    public SqlServerConfigurationSource(
        string connectionString,
        bool expectLogger = false,
        TimeSpan? commandTimeout = null,
        TimeSpan? refreshInterval = null,
        string schemaName = "Stravaig",
        string tableName = "AppConfiguration")
    {
        ValidateConnectionString(connectionString);
        ValidateSchemaName(schemaName);
        ValidateTableName(tableName);
        ExpectLogger = expectLogger;
        ConnectionString = connectionString;
        SchemaName = schemaName;
        TableName = tableName;
        RefreshInterval = refreshInterval ?? TimeSpan.Zero;
        CommandTimeout = commandTimeout ?? TimeSpan.FromSeconds(DefaultValues.CommandTimeout);

        _serverName = new Lazy<string>(() => new SqlConnectionStringBuilder(connectionString).DataSource);
        _databaseName = new Lazy<string>(() => new SqlConnectionStringBuilder(connectionString).InitialCatalog);
        _connectionTimeout = new Lazy<TimeSpan>(() => TimeSpan.FromSeconds(new SqlConnectionStringBuilder(connectionString).ConnectTimeout));
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