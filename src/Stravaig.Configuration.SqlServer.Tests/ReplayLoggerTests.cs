using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Stravaig.Configuration.SqlServer.Glue;
using Stravaig.Extensions.Logging.Diagnostics;
using Stravaig.Extensions.Logging.Diagnostics.Render;

namespace Stravaig.Configuration.SqlServer.Tests;

[TestFixture]
public class ReplayLoggerTests
{
    [Test]
    public void LogsThatReplayLimitWasReached()
    {
        ReplayLogger replayLogger = new ();
        for (int counter = 0; counter < 30; counter++)
        {
            replayLogger.LogInformation("This is log number {Counter}", counter);
        }

        TestCaptureLogger<ReplayLoggerTests> captureLogger = new ();
        replayLogger.Replay(captureLogger);

        var logs = captureLogger.GetLogs();
        logs.RenderLogs(Formatter.SimpleBySequence, Sink.Console);
        
        logs.Count.ShouldBe(28);
        
        logs[0].OriginalMessage.ShouldBe("Replaying {Count} delayed logs. NOTE: Some replayed logs may not emit if filters prevent them.");
        logs[0].PropertyDictionary["Count"].ShouldBe(25);
        
        logs[^2].OriginalMessage.ShouldBe("The replay limit was reached. There are {excessCount} logs that cannot be replayed.");
        logs[^2].PropertyDictionary["excessCount"].ShouldBe(5);
        
        logs[^1].OriginalMessage.ShouldBe("End of replay.");
    }
    
    [Test]
    public void ReplayLogsHappyPath()
    {
        ReplayLogger replayLogger = new ();
        for (int counter = 0; counter < 10; counter++)
        {
            replayLogger.LogInformation("This is log number {Counter}", counter);
        }

        TestCaptureLogger<ReplayLoggerTests> captureLogger = new ();
        replayLogger.Replay(captureLogger);

        var logs = captureLogger.GetLogs();
        logs.RenderLogs(Formatter.SimpleBySequence, Sink.Console);
        
        logs.Count.ShouldBe(12);
        
        logs[0].OriginalMessage.ShouldBe("Replaying {Count} delayed logs. NOTE: Some replayed logs may not emit if filters prevent them.");
        logs[0].PropertyDictionary["Count"].ShouldBe(10);
        
        logs[^1].OriginalMessage.ShouldBe("End of replay.");
    }
}