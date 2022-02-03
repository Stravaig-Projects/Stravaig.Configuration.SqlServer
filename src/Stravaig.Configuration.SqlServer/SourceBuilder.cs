using System;
using Microsoft.Extensions.Configuration;

namespace Stravaig.Configuration.SqlServer;

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
            options.ConnectionString ?? throw new SqlServerConfigurationProviderException("Cannot build a SQL Server Configuration Provider without a connection string."),
            TimeSpan.FromSeconds(options.RefreshSeconds),
            options.SchemaName ?? throw new SqlServerConfigurationProviderException("The schema name is required to use SQL Server Configuration."),
            options.TableName ?? throw new SqlServerConfigurationProviderException("The table name is required to use SQL Server Configuration."));
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