using System;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Shouldly;
using Stravaig.Configuration.SqlServer.Glue;
using Stravaig.Extensions.Logging.Diagnostics;
using Stravaig.Extensions.Logging.Diagnostics.Render;

namespace Stravaig.Configuration.SqlServer.Tests;

[TestFixture]
public class SqlServerConfigurationSourceExtensionTests
{
    private const string DummyConnectionString = "Server=localhost;Database=testing;Connection Timeout=5";

    [Test]
    public void CreateLoggerWhenNotExpectingALoggerIsNullLogger()
    {
        var source = new SqlServerConfigurationSource(DummyConnectionString, expectLogger: false);

        source.CreateLogger().ShouldBe(NullLogger<SqlServerConfigurationProvider>.Instance);
    }
    
    [Test]
    public void CreateLoggerWhenExpectingALoggerIsReplayLogger()
    {
        var source = new SqlServerConfigurationSource(DummyConnectionString, expectLogger: true);

        source.CreateLogger().ShouldBeOfType<ReplayLogger<SqlServerConfigurationProvider>>();
    }
    
    [Test]
    public void CreateLoggerIndicatesHowTheProviderIsSetup()
    {
        var source = new SqlServerConfigurationSource(DummyConnectionString, expectLogger: true, refreshInterval: TimeSpan.FromSeconds(120), commandTimeout: TimeSpan.FromSeconds(10));
        var replayLogger = (ReplayLogger<SqlServerConfigurationProvider>)source.CreateLogger();
        var captureLogger = new TestCaptureLogger<SqlServerConfigurationProvider>();
        replayLogger.Replay(captureLogger);

        var logs = captureLogger.GetLogs();
        logs.RenderLogs(Formatter.SimpleBySequence, Sink.Console);

        var descriptionLog = logs[1];
        descriptionLog.OriginalMessage.ShouldBe("SQL Server Configuration Provider will retrieve from [{serverName}].[{databaseName}].[{schemaName}].[{tableName}] {frequency}. Will wait {connectionTimeout} seconds to connect, and {commandTimeout} seconds to retrieve data.");
        descriptionLog.PropertyDictionary["serverName"].ShouldBe("localhost");
        descriptionLog.PropertyDictionary["databaseName"].ShouldBe("testing");
        descriptionLog.PropertyDictionary["schemaName"].ShouldBe("Stravaig");
        descriptionLog.PropertyDictionary["tableName"].ShouldBe("AppConfiguration");
        descriptionLog.PropertyDictionary["frequency"].ShouldBe("every 120 seconds");
        descriptionLog.PropertyDictionary["connectionTimeout"].ShouldBe(5);
        descriptionLog.PropertyDictionary["commandTimeout"].ShouldBe(10);
    }
}