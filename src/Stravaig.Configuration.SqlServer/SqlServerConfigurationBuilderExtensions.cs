using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Stravaig.Configuration.SqlServer;

public static class SqlServerConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddSqlServer(this IConfigurationBuilder configBuilder, Action<SqlServerConfigurationOptions>? optionsBuilder = null)
    {
        var options = SourceBuilder.BuildSource(configBuilder, optionsBuilder);
        
        return configBuilder.Add(options);
    }
}