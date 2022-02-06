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
    private readonly Stack<IDisposable> _scopes;
    private readonly int _limit = 25;

    public ReplayLogger()
    {
        this._logs = new Queue<Action<ILogger>>();
        this._scopes = new Stack<IDisposable>();
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
                _logs.Dequeue();
            _logs.Enqueue(ReplayLog);
        }
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable BeginScope<TState>(TState state)
    {
        lock (_lock)
        {
            _logs.Enqueue(logger =>
            {
                IDisposable scope = logger.BeginScope(state);

                _scopes.Push(scope);
            });

            return new Scope(this);
        }
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

            _logs.Clear();
        }
        logger.ReplayEnd();
    }

    private class Scope : IDisposable
    {
        private readonly ReplayLogger _logger;

        public Scope(ReplayLogger logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            lock (_logger._lock)
            {
                _logger._logs.Enqueue(_ => this._logger._scopes.Pop());
            }
        }
    }
}