using System;
using Microsoft.Extensions.Configuration;

namespace Stravaig.Configuration.SqlServer;

public static class SqlServerConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddSqlServer(this IConfigurationBuilder builder, Action<SqlServerConfigurationOptions>? optionsBuilder = null)
    {
        var options = new SqlServerConfigurationOptions();
        optionsBuilder?.Invoke(options);

        if (!string.IsNullOrWhiteSpace(options.ConfigurationSection))
            SetOptionsFromConfiguration(builder.Build(), options);
        
        return builder.Add(new SqlServerConfigurationSource(
            options.ConnectionString ?? throw new InvalidOperationException("Cannot build a SQL Server Configuration Provider with a null connection string."),
            options.SchemaName,
            options.TableName));
    }

    private static void SetOptionsFromConfiguration(IConfigurationRoot configRoot, SqlServerConfigurationOptions options)
    {
        var section = configRoot.GetSection(options.ConfigurationSection);
        section.Bind(options);
    }
}