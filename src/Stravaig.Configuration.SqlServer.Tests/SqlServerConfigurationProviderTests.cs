using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Stravaig.Configuration.SqlServer.Glue;
using Stravaig.Extensions.Logging.Diagnostics;
using Stravaig.Extensions.Logging.Diagnostics.Render;

namespace Stravaig.Configuration.SqlServer.Tests;

public class SqlServerConfigurationProviderTests
{
    private const string DummyConnectionString = "Server=localhost;Database=testing";
    private FakeDataLoader _fakeLoader;
    private FakeSqlServerConfigurationWatcher _fakeWatcher;
    private ILoggerFactory _loggerFactory;
    private TestCaptureLoggerProvider _loggerProvider;
    private SqlServerConfigurationSource _source;

    [SetUp]
    public void Setup()
    {
        _loggerFactory = new LoggerFactory();
        _loggerProvider = new TestCaptureLoggerProvider();
        _loggerFactory.AddProvider(_loggerProvider);
        _source = new SqlServerConfigurationSource(DummyConnectionString);
    }

    [Test]
    public void ValuesAreRetrievable()
    {
        var provider = SetupProvider();
        provider.Load();

        provider.TryGet("A", out var value);
        value.ShouldBe("111");
        
        provider.TryGet("B:A", out value);
        value.ShouldBe("222");

        provider.TryGet("B:B", out value);
        value.ShouldBe("333");

        var logs = _loggerProvider.GetLogEntriesFor<SqlServerConfigurationProvider>();
        logs.RenderLogs(Formatter.SimpleBySequence, Sink.Console);
        logs.Count.ShouldBe(1);
        logs[0].OriginalMessage.ShouldBe("Loading configuration data from SQL Server.");
    }

    [Test]
    public void LogExceptionOnLoadFailure()
    {
        _source = new SqlServerConfigurationSource(
            DummyConnectionString,
            schemaName: "TestSchema",
            tableName: "TestTable");
        var provider = SetupProvider();
        _fakeLoader.ThrowOnRetrieveData = new InvalidOperationException("Dummy Exception.");
        provider.Load();

        var logs = _loggerProvider.GetLogEntriesFor<SqlServerConfigurationProvider>();
        logs.RenderLogs(Formatter.SimpleBySequence, Sink.Console);
        logs.Count.ShouldBe(2);
        logs[0].OriginalMessage.ShouldBe("Loading configuration data from SQL Server.");
        var secondLog = logs[1];
        secondLog.Exception.ShouldBeOfType<InvalidOperationException>();
        secondLog.Exception.Message.ShouldBe("Dummy Exception.");
        secondLog.OriginalMessage.ShouldBe("Failed to get configuration information from the table [{schemaName}].[{tableName}] in the database [{databaseName}] on the server [{server}]. {exceptionMessage}");
        secondLog.PropertyDictionary["schemaName"].ShouldBe("TestSchema");
        secondLog.PropertyDictionary["tableName"].ShouldBe("TestTable");
        secondLog.PropertyDictionary["databaseName"].ShouldBe("testing");
        secondLog.PropertyDictionary["server"].ShouldBe("localhost");
        secondLog.PropertyDictionary["exceptionMessage"].ShouldBe("Dummy Exception.");
    }
    
    [Test]
    public void ReplayLogExceptionOnLoadFailure()
    {
        _source = new SqlServerConfigurationSource(
            DummyConnectionString,
            expectLogger: true,
            schemaName: "TestSchema",
            tableName: "TestTable");
        var provider = SetupProvider();
        _fakeLoader.ThrowOnRetrieveData = new InvalidOperationException("Dummy Exception.");
        provider.Load();
        provider.AttachLogger(_loggerFactory);

        var logs = _loggerProvider.GetLogEntriesFor<SqlServerConfigurationProvider>();
        logs.RenderLogs(Formatter.SimpleBySequence, Sink.Console);
        logs.Count.ShouldBe(4);
        logs[0].FormattedMessage.ShouldBe("Replaying 2 delayed logs. NOTE: Some replayed logs may not emit if filters prevent them.");
        logs[1].OriginalMessage.ShouldBe("Loading configuration data from SQL Server.");
        var thirdLog = logs[2];
        thirdLog.Exception.ShouldBeOfType<InvalidOperationException>();
        thirdLog.Exception.Message.ShouldBe("Dummy Exception.");
        thirdLog.OriginalMessage.ShouldBe("Failed to get configuration information from the table [{schemaName}].[{tableName}] in the database [{databaseName}] on the server [{server}]. {exceptionMessage}");
        thirdLog.PropertyDictionary["schemaName"].ShouldBe("TestSchema");
        thirdLog.PropertyDictionary["tableName"].ShouldBe("TestTable");
        thirdLog.PropertyDictionary["databaseName"].ShouldBe("testing");
        thirdLog.PropertyDictionary["server"].ShouldBe("localhost");
        thirdLog.PropertyDictionary["exceptionMessage"].ShouldBe("Dummy Exception.");
        logs[3].OriginalMessage.ShouldBe("End of replay.");
    }
    
    private SqlServerConfigurationProvider SetupProvider()
    {
        _fakeLoader = new FakeDataLoader()
        {
            FakeData = new[]
            {
                new KeyValuePair<string, string>("A", "111"),
                new KeyValuePair<string, string>("B:A", "222"),
                new KeyValuePair<string, string>("B:B", "333"),
            }
        };

        _fakeWatcher = new FakeSqlServerConfigurationWatcher();
        var logger = _source.ExpectLogger
            ? new ReplayLogger<SqlServerConfigurationProvider>()
            : _loggerFactory.CreateLogger<SqlServerConfigurationProvider>();
        var provider =
            new SqlServerConfigurationProvider(_source, _fakeLoader, _fakeWatcher, logger);
        return provider;
    }
}