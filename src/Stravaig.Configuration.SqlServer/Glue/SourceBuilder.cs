using System;
using Microsoft.Extensions.Configuration;

namespace Stravaig.Configuration.SqlServer.Glue;

internal static class SourceBuilder
{
    public static SqlServerConfigurationSource BuildSource(IConfigurationBuilder builder, Action<SqlServerConfigurationOptions>? optionsBuilder)
    {
        var options = new SqlServerConfigurationOptions();
        optionsBuilder?.Invoke(options);
        
        Lazy<IConfigurationRoot> configRoot = new (builder.Build);
        if (!string.IsNullOrWhiteSpace(options.ConfigurationSection))
            SetOptionsFromConfiguration(configRoot.Value, options);

        // If a connection string has been supplied directly, it takes
        // precedent over a connection string reference.
        if (string.IsNullOrWhiteSpace(options.ConnectionString) && 
            !string.IsNullOrWhiteSpace(options.ConnectionStringName))
            ApplyFromConnectionStringsSection(options, configRoot.Value);

        return new SqlServerConfigurationSource(
            connectionString: options.ConnectionString ?? throw new SqlServerConfigurationProviderException("Cannot build a SQL Server Configuration Provider without a connection string."),
            expectLogger: options.IsLoggerExpected,
            commandTimeout: TimeSpan.FromSeconds(options.CommandTimeout),
            refreshInterval: TimeSpan.FromSeconds(options.RefreshSeconds),
            schemaName: options.SchemaName ?? throw new SqlServerConfigurationProviderException("The schema name is required to use SQL Server Configuration."),
            tableName: options.TableName ?? throw new SqlServerConfigurationProviderException("The table name is required to use SQL Server Configuration."));
    }

    private static void ApplyFromConnectionStringsSection(SqlServerConfigurationOptions options, IConfigurationRoot configRoot)
    {
        var connectionString = configRoot.GetConnectionString(options.ConnectionStringName);
        if (!string.IsNullOrWhiteSpace(connectionString))
            options.ConnectionString = connectionString;
    }

    private static void SetOptionsFromConfiguration(IConfigurationRoot configRoot, SqlServerConfigurationOptions options)
    {
        var section = configRoot.GetSection(options.ConfigurationSection);
        section.Bind(options);
    }
}