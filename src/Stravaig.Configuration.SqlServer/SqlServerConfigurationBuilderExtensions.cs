using System;
using Microsoft.Extensions.Configuration;

namespace Stravaig.Configuration.SqlServer;

public static class SqlServerConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddSqlServer(this IConfigurationBuilder builder, Action<SqlServerConfigurationOptions>? optionsBuilder = null)
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
        
        return builder.Add(new SqlServerConfigurationSource(
            options.ConnectionString ?? throw new InvalidOperationException("Cannot build a SQL Server Configuration Provider with a null connection string."),
            TimeSpan.FromSeconds(options.RefreshSeconds),
            options.SchemaName,
            options.TableName));
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