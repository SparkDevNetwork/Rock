// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

using Rock.Data;

namespace Rock
{
    /// <summary>
    /// Helper class that will output EF SQL calls to the Debug Output Window
    /// </summary>
    public static class DebugHelper
    {
        /// <summary>
        /// The _call counts
        /// </summary>
        public static int _callCounts = 0;

        /// <summary>
        /// The call ms total
        /// </summary>
        public static double CallMSTotal => Interlocked.Read( ref _callMicrosecondsTotal ) / 1000.0;

        private static long _callMicrosecondsTotal = 0;

        private static StringBuilder _sqlOutput = new StringBuilder();

        /// <summary>
        /// Just output timings, don't include the SQL or Stack trace
        /// </summary>
        public static bool TimingsOnly = false;

        /// <summary>
        /// Returns true if there Logging is actively enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool IsEnabled { get; private set; } = false;

        private static string SessionId = null;

        /// <summary>
        /// Limits Debug Output to the current asp.net SessionId
        /// </summary>
        /// <param name="enable">if set to <c>true</c> [enable].</param>
        public static void LimitToSessionId( bool enable = true )
        {
            if ( enable )
            {
                SessionId = System.Web.HttpContext.Current?.Session?.SessionID;
            }
            else
            {
                SessionId = null;
            }
        }

        /// <summary>
        /// The summary only
        /// </summary>
        public static bool SummaryOnly = false;

        private class DebugHelperUserState
        {
            public int CallNumber { get; set; }

            public Stopwatch Stopwatch { get; set; }

            public double CommandExecutedStopwatchMS { get; internal set; }

            public double CommandExecutedSqlExecutionTimeMS { get; internal set; }

            public double StatementCompletedStopwatchMS { get; internal set; }

            public double StatementCompletedSqlExecutionTimeMS { get; internal set; }
        }

        /// <summary>
        /// 
        /// </summary>
        private class DebugLoggingDbCommandInterceptor : DbCommandInterceptor
        {
            /// <summary>
            /// Gets or sets the rock context to show the SQL Output for.  Leave null to show SQL for all rockContexts
            /// </summary>
            /// <value>
            /// The rock context.
            /// </value>
            internal List<System.Data.Entity.DbContext> DbContextList { get; set; } = new List<System.Data.Entity.DbContext>();

            /// <summary>
            /// Gets or sets a value indicating whether [enable for all database contexts].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [enable for all database contexts]; otherwise, <c>false</c>.
            /// </value>
            internal bool EnableForAllDbContexts { get; set; } = false;

            private const string UserStateKey = "DebugLoggingDbCommandInterceptorKey";

            /// <summary>
            /// </summary>
            /// <param name="command"></param>
            /// <param name="interceptionContext"></param>
            /// <inheritdoc />
            public override void NonQueryExecuting( DbCommand command, DbCommandInterceptionContext<int> interceptionContext )
            {
                object userState;
                this.CommandExecuting( command, interceptionContext, out userState );
                interceptionContext.SetUserState( UserStateKey, userState );
            }

            /// <summary>
            /// </summary>
            /// <param name="command"></param>
            /// <param name="interceptionContext"></param>
            /// <inheritdoc />
            public override void NonQueryExecuted( DbCommand command, DbCommandInterceptionContext<int> interceptionContext )
            {
                this.CommandExecuted( command, interceptionContext, interceptionContext.FindUserState( UserStateKey ) );
            }

            /// <summary>
            /// </summary>
            /// <param name="command"></param>
            /// <param name="interceptionContext"></param>
            /// <inheritdoc />
            public override void ScalarExecuting( DbCommand command, DbCommandInterceptionContext<object> interceptionContext )
            {
                object userState;
                this.CommandExecuting( command, interceptionContext, out userState );
                interceptionContext.SetUserState( UserStateKey, userState );
            }

            /// <summary>
            /// </summary>
            /// <param name="command"></param>
            /// <param name="interceptionContext"></param>
            /// <inheritdoc />
            public override void ScalarExecuted( DbCommand command, DbCommandInterceptionContext<object> interceptionContext )
            {
                this.CommandExecuted( command, interceptionContext, interceptionContext.FindUserState( UserStateKey ) );
            }

            /// <summary>
            /// </summary>
            /// <param name="command"></param>
            /// <param name="interceptionContext"></param>
            /// <inheritdoc />
            public override void ReaderExecuting( System.Data.Common.DbCommand command, DbCommandInterceptionContext<System.Data.Common.DbDataReader> interceptionContext )
            {
                object userState;
                this.CommandExecuting( command, interceptionContext, out userState );
                interceptionContext.SetUserState( UserStateKey, userState );
            }

            /// <summary>
            /// </summary>
            /// <param name="command"></param>
            /// <param name="interceptionContext"></param>
            /// <inheritdoc />
            public override void ReaderExecuted( DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext )
            {
                this.CommandExecuted( command, interceptionContext, interceptionContext.FindUserState( UserStateKey ) );
            }

            /// <summary>
            /// Commands the executing.
            /// </summary>
            /// <param name="command">The command.</param>
            /// <param name="interceptionContext">The interception context.</param>
            /// <param name="userState">State of the user.</param>
            /// <inheritdoc />
            private void CommandExecuting( DbCommand command, DbCommandInterceptionContext interceptionContext, out object userState )
            {
                userState = null;
                if ( !interceptionContext.DbContexts.Any( a => this.DbContextList.Contains( a ) || this.EnableForAllDbContexts ) )
                {
                    return;
                }

                if ( !( command is System.Data.SqlClient.SqlCommand ) )
                {
                    // not a SQL command. Nothing is going to the Database. Probably interception.
                    return;
                }

                if ( SessionId.IsNotNullOrWhiteSpace() )
                {
                    if ( System.Web.HttpContext.Current?.Session?.SessionID != SessionId )
                    {
                        return;
                    }
                }

                var incrementedCallCount = Interlocked.Increment( ref DebugHelper._callCounts );

                if ( !TimingsOnly && !SummaryOnly )
                {
                    StringBuilder sbDebug = GetSQLBlock( command, incrementedCallCount );
                    _sqlOutput.Append( sbDebug );
                    System.Diagnostics.Debug.Write( sbDebug.ToString() );
                }

                var sqlConnection = command.Connection as System.Data.SqlClient.SqlConnection;

                sqlConnection.StatisticsEnabled = true;
                sqlConnection.ResetStatistics();

                if ( userState == null )
                {
                    userState = new DebugHelperUserState { CallNumber = incrementedCallCount, Stopwatch = Stopwatch.StartNew() };
                }
            }

            private static StringBuilder GetSQLBlock( DbCommand command, int incrementedCallCount )
            {
                StringBuilder sbDebug = new StringBuilder();

                sbDebug.AppendLine( "\n" );

                StackTrace st = new StackTrace( 2, true );
                var frames = st.GetFrames().Where( a => a.GetFileName() != null && !a.GetFileName().Contains( "DebugHelper.cs" ) );
                var stackTraceMessage = frames.ToList().AsDelimited( "" );

                sbDebug.Append( $@"
/*
Call# {incrementedCallCount}
StackTrace:
{stackTraceMessage}
*/" );

                sbDebug.AppendLine( "BEGIN\n" );

                var declares = command.Parameters.OfType<System.Data.SqlClient.SqlParameter>()
                    .Select( p =>
                    {
                        if ( p.SqlDbType == System.Data.SqlDbType.NVarChar )
                        {
                            var sqlString = ( SqlString ) p.SqlValue;
                            string sqlValue = sqlString.IsNull ? "null" : sqlString.Value?.Truncate( 255 );

                            return string.Format( "@{0} {1}({2}) = '{3}'", p.ParameterName, p.SqlDbType, p.Size, sqlValue?.Replace( "'", "''" ) );
                        }

                        if ( p.SqlDbType == System.Data.SqlDbType.Int )
                        {
                            Type valueType = p.Value?.GetType();
                            object numericValue;

                            // if this is a nullable enum, we'll have to look at p.Value instead of p.SqlValue to see what is really getting passed to SQL
                            if ( valueType?.IsEnum == true && p.Value != null )
                            {
                                /* If this is an enum (for example GroupMemberStatus.Active), show the numeric 
                                 * value getting passed to SQL
                                 * p.Value will be the Enum, so convert it to int;
                                 */
                                numericValue = ( int ) p.Value;
                            }
                            else
                            {
                                numericValue = p.SqlValue;
                            }

                            return $"@{p.ParameterName} {p.SqlDbType} = {numericValue.ToString() ?? "null" }";
                        }
                        else if ( p.SqlDbType == System.Data.SqlDbType.Udt )
                        {
                            return string.Format( "@{0} {1} = '{2}'", p.ParameterName, p.UdtTypeName, p.SqlValue );
                        }
                        else if ( p.SqlDbType == System.Data.SqlDbType.Bit )
                        {
                            return string.Format( "@{0} {1} = {2}", p.ParameterName, p.SqlDbType, ( ( System.Data.SqlTypes.SqlBoolean ) p.SqlValue ).ByteValue );
                        }
                        else if ( p.SqlDbType == System.Data.SqlDbType.Decimal )
                        {
                            return string.Format( "@{0} {1} = {2}", p.ParameterName, p.SqlDbType, p.SqlValue ?? "null" );
                        }
                        else
                        {
                            return string.Format( "@{0} {1} = '{2}'", p.ParameterName, p.SqlDbType, p.SqlValue );
                        }
                    } ).ToList().AsDelimited( ",\n" );

                if ( !string.IsNullOrEmpty( declares ) )
                {
                    sbDebug.AppendLine( "DECLARE\n" + declares + "\n\n" );
                }

                sbDebug.AppendLine( command.CommandText );

                sbDebug.AppendLine( "\nEND\nGO\n\n" );
                return sbDebug;
            }

            /// <summary>
            /// Readers the executed.
            /// </summary>
            /// <param name="command">The command.</param>
            /// <param name="interceptionContext">The interception context.</param>
            /// <param name="userState">State of the user.</param>
            public void CommandExecuted( System.Data.Common.DbCommand command, DbCommandInterceptionContext interceptionContext, object userState )
            {
                var sqlReaderContext = interceptionContext as System.Data.Entity.Infrastructure.Interception.DbCommandInterceptionContext<System.Data.Common.DbDataReader>;
                var sqlCommand = command as System.Data.SqlClient.SqlCommand;

                var debugHelperUserState = userState as DebugHelperUserState;
                if ( debugHelperUserState != null )
                {
                    bool outputOnStatementCompleted = sqlCommand != null;
                    if ( outputOnStatementCompleted )
                    {
                        sqlCommand.StatementCompleted += ( object sender, System.Data.StatementCompletedEventArgs e ) =>
                        {
                            HandleStatementCompleted( sqlCommand, sender, debugHelperUserState );
                        };
                    }

                    debugHelperUserState.Stopwatch.Stop();

                    var sqlConnection = command.Connection as System.Data.SqlClient.SqlConnection;
                    var stats = sqlConnection.RetrieveStatistics();

                    var commandExecutionTimeInMs = ( long ) stats["ExecutionTime"];
                    debugHelperUserState.CommandExecutedStopwatchMS = debugHelperUserState.Stopwatch.Elapsed.TotalMilliseconds;
                    debugHelperUserState.CommandExecutedSqlExecutionTimeMS = commandExecutionTimeInMs;
                    var totalMicroSeconds = ( long ) Math.Round( debugHelperUserState.Stopwatch.Elapsed.TotalMilliseconds * 1000 );
                    Interlocked.Add( ref _callMicrosecondsTotal, totalMicroSeconds );

                    if ( !SummaryOnly )
                    {
                        string commandExecutionTimeText = $"{debugHelperUserState.CommandExecutedSqlExecutionTimeMS,6:0}     ms";
                        var commandExecutedElaspedTimeMS = debugHelperUserState.CommandExecutedStopwatchMS;

                        // SQL server rounds to the nearest millisecond, which means it'll be meaningless compared to our ElapsedTime if it is less than 2ms
                        if ( debugHelperUserState.CommandExecutedSqlExecutionTimeMS < 2 )
                        {
                            commandExecutionTimeText = "-   ".PadLeft( 13 );
                        }

                        var statsMessage = $@"
-- Call# {debugHelperUserState.CallNumber} Timings:
--  [{commandExecutionTimeText}] ExecutionTime (CommandExecuted)
--  [{commandExecutedElaspedTimeMS,10:0.000} ms] ElapsedTime   (CommandExecuted)".Trim();

                        _sqlOutput.Append( statsMessage );
                        System.Diagnostics.Debug.WriteLine( statsMessage );
                    }

                    debugHelperUserState.Stopwatch.Start();
                }
            }

            private static void HandleStatementCompleted( System.Data.SqlClient.SqlCommand sqlCommand, object sender, DebugHelperUserState debugHelperUserState )
            {
                // handle StatementCompleted Event
                debugHelperUserState.Stopwatch.Stop();
                var eventSqlConnection = sqlCommand.Connection as System.Data.SqlClient.SqlConnection;
                var eventStats = eventSqlConnection.RetrieveStatistics();

                var eventCommandExecutionTimeInMs = ( long ) eventStats["ExecutionTime"];
                debugHelperUserState.StatementCompletedStopwatchMS = debugHelperUserState.Stopwatch.Elapsed.TotalMilliseconds;
                debugHelperUserState.StatementCompletedSqlExecutionTimeMS = eventCommandExecutionTimeInMs - debugHelperUserState.CommandExecutedSqlExecutionTimeMS;
                eventSqlConnection.StatisticsEnabled = false;
                string statementExecutionTimeText = $"{debugHelperUserState.StatementCompletedSqlExecutionTimeMS,6:0}     ms";

                if ( debugHelperUserState.StatementCompletedSqlExecutionTimeMS < 2 )
                {
                    statementExecutionTimeText = "-   ".PadLeft( 13 );
                }

                var statementCompletedElapsedTimeMS = debugHelperUserState.StatementCompletedStopwatchMS - debugHelperUserState.CommandExecutedStopwatchMS;

                if ( !SummaryOnly )
                {
                    var summary = $@"
--  [{statementExecutionTimeText}] ExecutionTime (StatementCompleted)
--  [{statementCompletedElapsedTimeMS,10:0.000} ms] ElapsedTime   (StatementCompleted)
--  [{debugHelperUserState.StatementCompletedStopwatchMS,10:0.000} ms] Total".Trim();

                    System.Diagnostics.Debug.WriteLine( summary );
                }
            }
        }

        /// <summary>
        /// The _debug logging database command interceptor
        /// </summary>
        private static DebugLoggingDbCommandInterceptor _debugLoggingDbCommandInterceptor = new DebugLoggingDbCommandInterceptor();

        /// <summary>
        /// SQLs the logging start.
        /// </summary>
        public static void SQLLoggingStart()
        {
            SQLLoggingStart( null );
        }

        /// <summary>
        /// Starts logging all EF SQL Calls to the Debug Output Window as T-SQL Blocks
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        public static void SQLLoggingStart( RockContext rockContext )
        {
            SQLLoggingStart( ( DbContext ) rockContext );
        }

        /// <summary>
        /// Starts logging all EF SQL Calls to the Debug Output Window as T-SQL Blocks
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public static void SQLLoggingStart( System.Data.Entity.DbContext dbContext )
        {
            _callCounts = 0;
            _callMicrosecondsTotal = 0;
            SQLLoggingStop();
            IsEnabled = true;
            _sqlOutput.Clear();

            if ( dbContext != null )
            {
                _debugLoggingDbCommandInterceptor.DbContextList.Add( dbContext );
            }
            else
            {
                _debugLoggingDbCommandInterceptor.EnableForAllDbContexts = true;
            }

            DbInterception.Add( _debugLoggingDbCommandInterceptor );
        }

        /// <summary>
        /// Gets the SQL output generated since SqlLoggingStart was called
        /// </summary>
        /// <returns></returns>
        public static string GetSqlOutput()
        {
            return _sqlOutput.ToString();
        }

        /// <summary>
        /// Stops logging all EF SQL Calls to the Debug Output Window
        /// </summary>
        public static void SQLLoggingStop()
        {
            IsEnabled = false;
            if ( _callCounts != 0 )
            {
                Debug.WriteLine( $"/* ####SQLLogging Summary: _callCounts:{_callCounts}, _callMSTotal:{CallMSTotal}, _callMSTotal/_callCounts:{CallMSTotal / _callCounts}#### */" );
            }

            if ( _debugLoggingDbCommandInterceptor != null )
            {
                _debugLoggingDbCommandInterceptor.EnableForAllDbContexts = false;
                _debugLoggingDbCommandInterceptor.DbContextList.Clear();
                DbInterception.Remove( _debugLoggingDbCommandInterceptor );
            }
        }

        /// <summary>
        /// Enables or Disables SqlLogging
        /// </summary>
        /// <param name="dbContext">The database context to filter logs to.</param>
        /// <param name="enable">if set to <c>true</c> [enable].</param>
        public static void SqlLogging( this System.Data.Entity.DbContext dbContext, bool enable )
        {
            if ( enable )
            {
                DebugHelper.SQLLoggingStart( dbContext );
            }
            else
            {
                if ( dbContext == null )
                {
                    _debugLoggingDbCommandInterceptor.EnableForAllDbContexts = false;
                }
                else
                {
                    _debugLoggingDbCommandInterceptor.DbContextList.Remove( dbContext );
                }

                if ( !_debugLoggingDbCommandInterceptor.DbContextList.Any() && !_debugLoggingDbCommandInterceptor.EnableForAllDbContexts )
                {
                    DebugHelper.SQLLoggingStop();
                }
            }
        }

        #region Trace Logging

        private const string _DateTimeFormat = "HH:mm:ss.fff";

        private static ConcurrentDictionary<string, TaskInfo> _tasks = new ConcurrentDictionary<string, TaskInfo>();

        private class TaskInfo
        {
            public string TaskIdentifier;
            public DateTime StartDateTime;
        }

        /// <summary>
        /// Start a named task and emit an optional log message.
        /// </summary>
        /// <param name="taskIdentifier"></param>
        /// <param name="logMessage"></param>
        public static void StartTask( string taskIdentifier, string logMessage = null )
        {
            if ( string.IsNullOrWhiteSpace( taskIdentifier ) )
            {
                taskIdentifier = "(default)";
            }

            if ( _tasks.ContainsKey(taskIdentifier) )
            {
                // If the task is already started, ignore this call.
                return;
            }

            var newTask = new TaskInfo()
            {
                TaskIdentifier = taskIdentifier,
                StartDateTime = DateTime.Now
            };
            _tasks.TryAdd( taskIdentifier, newTask );

            var msg = $"<TASK-START> {taskIdentifier}";
            if ( !string.IsNullOrWhiteSpace( logMessage ) )
            {
                msg += $": {logMessage}";
            }
            msg = msg.Trim();

            Log( msg );
        }

        /// <summary>
        /// End a named task and emit a log message showing elapsed time.
        /// </summary>
        /// <param name="taskIdentifier"></param>
        /// <param name="logMessage"></param>
        public static void StopTask( string taskIdentifier, string logMessage = null )
        {
            if ( string.IsNullOrWhiteSpace( taskIdentifier ) )
            {
                taskIdentifier = "(default)";
            }

            if ( _tasks.TryGetValue( taskIdentifier, out var task ) )
            {
                var msg = $"<TASK--STOP> {taskIdentifier} [Elapsed={DateTime.Now.Subtract( task.StartDateTime ).TotalSeconds:N2}s]";
                if ( !string.IsNullOrWhiteSpace( logMessage ) )
                {
                    msg += $": {logMessage}";
                }
                msg = msg.Trim();

                Log( msg );

                _tasks.TryRemove( taskIdentifier, out _ );
            }
        }

        /// <summary>
        /// Log an information message to trace output.
        /// </summary>
        /// <param name="message"></param>
        public static void Log( string message )
        {
            var msg = $@"[{DateTime.Now.ToString( _DateTimeFormat )}] {message}";

            Trace.WriteLine( msg );
        }

        /// <summary>
        /// Log a warning message to trace output.
        /// </summary>
        /// <param name="message"></param>
        public static void LogWarning( string message )
        {
            var msg = $"[{DateTime.Now.ToString( _DateTimeFormat )}] {message}";

            Trace.TraceWarning( msg );
        }

        /// <summary>
        /// Log an error message to trace output.
        /// </summary>
        /// <param name="message"></param>
        public static void LogError( string message )
        {
            var msg = $"[{DateTime.Now.ToString( _DateTimeFormat )}] {message}";

            Trace.TraceError( msg );
        }

        /// <summary>
        /// Log an exception to trace output.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        public static void LogError( Exception ex, string message = null )
        {
            var msg = $"[{DateTime.Now.ToString( _DateTimeFormat )}] {message}";
            msg += "\n" + ex.ToString();

            Trace.TraceError( msg );
        }

        #endregion
    }
}