using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Stravaig.Configuration.SqlServer;

public static class ConfigurationRootExtensions
{
    public static IConfigurationRoot AttachLoggerToSqlServerProvider(this IConfigurationRoot configRoot, ILoggerFactory? loggerFactory)
    {
        var theLoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;

        var providers = configRoot.Providers
            .OfType<SqlServerConfigurationProvider>();
        foreach (var provider in providers)
        {
            provider.AttachLogger(theLoggerFactory);
        }

        return configRoot;
    }
}