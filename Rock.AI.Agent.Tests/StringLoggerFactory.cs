using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

namespace Rock.AI.Agent.Tests;

/// <summary>
/// A custom logger factory that captures log messages in a list of strings.
/// </summary>
class StringLoggerFactory : ILoggerFactory
{
    /// <summary>
    /// The collection that we will add new log messages into.
    /// </summary>
    private readonly List<string> _messages;

    /// <summary>
    /// Creates a new instance of <see cref="StringLoggerFactory"/>.
    /// </summary>
    /// <param name="logs">The collection that we will add new log messages into.</param>
    public StringLoggerFactory( List<string> messages )
    {
        _messages = messages;
    }

    /// <inheritdoc/>
    public void AddProvider( ILoggerProvider provider )
    {
    }

    /// <inheritdoc/>
    public ILogger CreateLogger( string categoryName )
    {
        return new StringLogger( _messages );
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }

    /// <summary>
    /// Internal logger implementation that captures log messages.
    /// </summary>
    private class StringLogger : ILogger
    {
        /// <summary>
        /// The collection that we will add new log messages into.
        /// </summary>
        private readonly List<string> _messages;

        /// <summary>
        /// Creates a new instance of <see cref="StringLogger"/>.
        /// </summary>
        /// <param name="logs">The collection that we will add new log messages into.</param>
        public StringLogger( List<string> messages )
        {
            _messages = messages;
        }

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>( TState state ) => throw new NotImplementedException();

        /// <inheritdoc/>
        public bool IsEnabled( LogLevel logLevel ) => true;

        /// <inheritdoc/>
        public void Log<TState>( LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter )
        {
            Console.WriteLine( formatter( state, exception ) );
            lock ( _messages )
            {
                _messages.Add( formatter( state, exception ) );
            }
        }
    }
}
