using System;
using Microsoft.Extensions.Logging;

namespace Stravaig.Configuration.SqlServer.Glue;

internal interface ISqlServerConfigurationWatcher : IDisposable
{
    void EnsureStarted();
    void AttachLogger(ILogger logger);
}