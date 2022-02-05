using System;
using Microsoft.Extensions.Configuration;
using Stravaig.Configuration.SqlServer.Glue;

namespace Stravaig.Configuration.SqlServer;

public static class SqlServerConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddSqlServer(this IConfigurationBuilder configBuilder, Action<SqlServerConfigurationOptions>? optionsBuilder = null)
    {
        var options = SourceBuilder.BuildSource(configBuilder, optionsBuilder);
        
        return configBuilder.Add(options);
    }
}