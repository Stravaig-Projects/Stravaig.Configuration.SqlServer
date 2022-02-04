using Microsoft.Extensions.Logging;

namespace Stravaig.Configuration.SqlServer;

public interface ISqlServerConfigurationWatcher
{
    void EnsureStarted();
    void AttachLogger(ILogger logger);
}