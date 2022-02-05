using Microsoft.Extensions.Logging;

namespace Stravaig.Configuration.SqlServer.Glue;

internal interface ISqlServerConfigurationWatcher
{
    void EnsureStarted();
    void AttachLogger(ILogger logger);
}