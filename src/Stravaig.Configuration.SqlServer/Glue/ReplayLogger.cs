using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Stravaig.Configuration.SqlServer.Glue;

internal class ReplayLogger<T> : ReplayLogger, ILogger<T>
{
}

internal class ReplayLogger : ILogger
{
    // This class is taken and modified from
    // https://stackoverflow.com/a/58265030/8152

    private readonly object _lock = new ();
    private readonly Queue<Action<ILogger>> _logs;
    private readonly int _limit = 25;
    private int _limitExcess;

    internal ReplayLogger()
    {
        _logs = new Queue<Action<ILogger>>();
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        void ReplayLog(ILogger logger)
        {
            logger.Log(logLevel, eventId, state, exception, formatter);
        }

        lock (_lock)
        {
            if (_logs.Count >= _limit)
            {
                _limitExcess++;               
                return;
            }
            _logs.Enqueue(ReplayLog);
        }
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable BeginScope<TState>(TState state)
    {
        return new Scope();
    }

    public void Replay(ILogger logger)
    {
        if (logger == null) throw new ArgumentNullException(nameof(logger));

        lock (_lock)
        {
            if (_logs.Count == 0)
                return;

            logger.ReplayStart(_logs.Count);
            foreach (Action<ILogger> replayTo in _logs)
            {
                replayTo(logger);
            }

            if (_limitExcess > 0)
                logger.ReplayLimitExceeded(_limitExcess);
                    
            _logs.Clear();
            _limitExcess = 0;
        }
        logger.ReplayEnd();
    }

    private class Scope : IDisposable
    {
        public void Dispose()
        {
        }
    }
}