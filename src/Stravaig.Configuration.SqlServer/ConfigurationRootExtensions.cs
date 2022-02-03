using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Stravaig.Configuration.SqlServer;

public static class ConfigurationRootExtensions
{
    public static IConfigurationRoot AttachLoggerToSqlServerProvider(this IConfigurationRoot configRoot, ILoggerFactory? loggerFactory)
    {
        loggerFactory ??= NullLoggerFactory.Instance;

        var providers = configRoot.Providers
            .OfType<SqlServerConfigurationProvider>();
        foreach (var provider in providers)
        {
            provider.AttachLogger(loggerFactory);
        }

        return configRoot;
    }
}