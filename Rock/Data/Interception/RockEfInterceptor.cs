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
using System.Diagnostics;
using System.Linq;
using System.Text;

using Rock.Observability;
using Rock.Web.Cache.NonEntities;

namespace Rock.Data.Interception
{

    /// <summary>
    /// Rock's core command interceptor
    /// This DB interceptor was created to provide a central place for Rock to extend EF with new capabilities.
    /// </summary>
    /// <seealso cref="System.Data.Entity.Infrastructure.Interception.DbCommandInterceptor" />
    public sealed class RockEfInterceptor : IDbCommandInterceptor
    {
        /*
            7/11/2021 - JME

            This DB interceptor was created to provide a central place for Rock to extend EF
            with new capabilities.

            # Metrics
            The first of these capabilities was to provide a metrics collection of timings and
            details for queries in a context. Many different attempts were made to measure the
            timings. DateTime.Now and DateTime.UtcNow were ruled out as they are not very performant
            nor reliable. Many queries were measured to take 0 ticks. Landed on using a shared
            Stopwatch. Considered making a Stopwatch per command (saved in the Concurrent Dictionary)
            but decided on a single shared one.

            There are three levels of metrics that can be configured for each RockContext:
            - None:  No metrics are collected (but the interceptor is still running and having to check
                     the configuration of the RockContext.
            - Count: This mode will only count the number of queries being executed.
            - Full:  This mode included counts as well as the actual SQL that was run. The SQL is evaluated
                     and the parameters are inserted into the SQL.

            # Future
            TagWith - https://github.com/VariableNotFound/ef6-tagwith
            Note that this could break SQL Server's query plan cache as the cache is keyed off of the
            entire SQL string (including comments). So if the comments were to change this would trigger
            multiple cached plans. I don't think this would lead a lot 'extra' plans since the tags would
            be consistent. Should warn people not to make the tags dynamic though.

            # Performance
            Performance testing was done to view the impact of running this interceptor. The results of
            which are below.

            Two different queries were used for testing:
            - Simple:   Queries for a person by Id
                        var person = new PersonService( rockContext ).Get( 1 );

            - Typical:  Queries for groups active groups of type 'Small Group' that have more than 2 members
                        returning the group name, description and list of member records.
                        var groups = new GroupService( rockContext ).Queryable()
                                    .Where( g =>
                                        g.GroupTypeId == 25
                                        && g.Members.Count() > 2
                                        && g.IsActive == true
                                    )
                                    .Select( g => new { g.Members, g.Name, g.Description } )
                                    .ToList();

            Tests results show the time it took to run the queries 1000 times. The times shown are the average
            of running these 1000 query tests 200 times.

            +-----------------------------------------------------------------------------+
            | CONFIGURATION                 | SIMPLE QUERY         | TYPICAL QUERY        |
            +-------------------------------|----------------------|----------------------+
            | No Interceptor Configured     |  814ms               | 3016ms               |
            | (baseline original behavior)  |                      |                      |
            +-------------------------------|----------------------|----------------------+
            | QueryMetricDetailLevel:       |  815ms               | 3114ms               |
            | None                          |  + .1%               | + 3%                 |
            +-------------------------------|----------------------|----------------------+
            | QueryMetricDetailLevel:       |  824ms               | 3018ms               |
            | Count                         |  + 1.2%              | 0%                   |
            +-------------------------------|----------------------|----------------------+
            | QueryMetricDetailLevel:       |  885ms               | 3340ms               |
            | Full                          |  + 8.7%              | + 10.7%              |
            +-------------------------------|----------------------|----------------------+
            
            As you can see the differences are minor in terms of ms. Results also are not
            consistent which leads us to note that measuring these types of small units on
            a developer laptop are difficult.

            Notes
            NonQuery -  Performs catalog operations (for example, querying the structure of a database or
                        creating database objects such as tables), or to change the data in a database by
                        executing UPDATE, INSERT, or DELETE statements.

            Scalar -    Retrieves a single value (for example, an aggregate value) from a database. This
                        requires less code than using the ExecuteReader method and performing the operations
                        necessary to generate the single value using the data returned by a DbDataReader.

            Reader -    Basic data retrieval.
        */

        // List of database types that needed to be quoted when merged into SQL
        static private List<System.Data.DbType> _quoteRequiredFieldTypes = new List<System.Data.DbType>() {
                                    System.Data.DbType.String,
                                    System.Data.DbType.DateTime,
                                    System.Data.DbType.Guid,
                                    System.Data.DbType.AnsiString,
                                    System.Data.DbType.AnsiStringFixedLength,
                                    System.Data.DbType.DateTime2,
                                    System.Data.DbType.DateTimeOffset,
                                    System.Data.DbType.Xml };

        // Dictionary to store the start times of each command to create the timings
        ConcurrentDictionary<DbCommand, long> _commandStartTimes = new ConcurrentDictionary<DbCommand, long>();

        // Shared stopwatch for timings
        Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// Initializes a new instance of the <see cref="RockEfInterceptor"/> class.
        /// </summary>
        public RockEfInterceptor()
        {
            _stopwatch.Start();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="RockEfInterceptor"/> class. Probably overkill...
        /// </summary>
        ~RockEfInterceptor()
        {
            _stopwatch.Stop();
        }

        #region Interceptor Methods
        /// <summary>
        /// This method is called after a call to <see cref="M:System.Data.Common.DbCommand.ExecuteNonQuery" />  or
        /// one of its async counterparts is made. The result used by Entity Framework can be changed by setting
        /// <see cref="P:System.Data.Entity.Infrastructure.Interception.DbCommandInterceptionContext`1.Result" />.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <remarks>
        /// For async operations this method is not called until after the async task has completed
        /// or failed.
        /// </remarks>
        public void NonQueryExecuted( DbCommand command, DbCommandInterceptionContext<int> interceptionContext )
        {
            EndTiming( command, interceptionContext );
        }

        /// <summary>
        /// This method is called before a call to <see cref="M:System.Data.Common.DbCommand.ExecuteNonQuery" /> or
        /// one of its async counterparts is made.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        public void NonQueryExecuting( DbCommand command, DbCommandInterceptionContext<int> interceptionContext )
        {
            StartTiming( command, interceptionContext );
        }

        /// <summary>
        /// This method is called after a call to <see cref="M:System.Data.Common.DbCommand.ExecuteReader(System.Data.CommandBehavior)" /> or
        /// one of its async counterparts is made. The result used by Entity Framework can be changed by setting
        /// <see cref="P:System.Data.Entity.Infrastructure.Interception.DbCommandInterceptionContext`1.Result" />.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <remarks>
        /// For async operations this method is not called until after the async task has completed
        /// or failed.
        /// </remarks>
        public void ReaderExecuted( DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext )
        {
            EndTiming( command, interceptionContext );
        }

        /// <summary>
        /// This method is called before a call to <see cref="M:System.Data.Common.DbCommand.ExecuteReader(System.Data.CommandBehavior)" /> or
        /// one of its async counterparts is made.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        public void ReaderExecuting( DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext )
        {
            StartTiming( command, interceptionContext );
        }

        /// <summary>
        /// This method is called before a call to <see cref="M:System.Data.Common.DbCommand.ExecuteScalar" /> or
        /// one of its async counterparts is made.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        public void ScalarExecuting( DbCommand command, DbCommandInterceptionContext<object> interceptionContext )
        {
            StartTiming( command, interceptionContext );
        }

        /// <summary>
        /// This method is called after a call to <see cref="M:System.Data.Common.DbCommand.ExecuteScalar" /> or
        /// one of its async counterparts is made. The result used by Entity Framework can be changed by setting
        /// <see cref="P:System.Data.Entity.Infrastructure.Interception.DbCommandInterceptionContext`1.Result" />.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <remarks>
        /// For async operations this method is not called until after the async task has completed
        /// or failed.
        /// </remarks>
        public void ScalarExecuted( DbCommand command, DbCommandInterceptionContext<object> interceptionContext )
        {
            EndTiming( command, interceptionContext );
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Starts the timing.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="interceptionContext">The interception context.</param>
        private void StartTiming( DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext )
        {
            StartTiming( command, interceptionContext.DbContexts.FirstOrDefault() );
        }

        /// <summary>
        /// Starts the timing.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="interceptionContext">The interception context.</param>
        private void StartTiming( DbCommand command, DbCommandInterceptionContext<object> interceptionContext )
        {
            StartTiming( command, interceptionContext.DbContexts.FirstOrDefault() );
        }

        /// <summary>
        /// Starts the timing.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="interceptionContext">The interception context.</param>
        private void StartTiming( DbCommand command, DbCommandInterceptionContext<int> interceptionContext )
        {
            StartTiming( command, interceptionContext.DbContexts.FirstOrDefault() );
        }

        /// <summary>
        /// Starts the timing.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="context">The context.</param>
        private void StartTiming( DbCommand command, System.Data.Entity.DbContext context )
        {
            /*  
                6/16/2023 JME
                The activity kind must be client and the db.system attribute is required to flag this as a 'database' activity.
                Other attributes like connection string are recommended, but left off to reduce the size.
                https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/database.md
            */
                        
            // Create observability activity
            var activity = ObservabilityHelper.StartActivity( "Database Command", ActivityKind.Client );

            // Check if there is an activity before we make calls to get observability information. If there is no observability configuration
            // then there will not be an activity.
            if ( activity != null )
            {
                var observabilityInfo = DbCommandObservabilityCache.Get( command.CommandText );

                activity.DisplayName = $"DB: {observabilityInfo.Prefix} ({observabilityInfo.CommandHash})";
                activity.AddTag( "db.system", "mssql" );
                activity.AddTag( "db.query", command.CommandText.Truncate( ObservabilityHelper.MaximumAttributeLength, false ) );
                activity.AddTag( "rock-otel-type", "rock-db" );
                activity.AddTag( "rock-db-hash", observabilityInfo.CommandHash );

                // Check if this query should get additional observability telemetry
                if ( DbCommandObservabilityCache.TargetedQueryHashes.Contains( observabilityInfo.CommandHash ) )
                {
                    // Append stack trace
                    var stackTrace = TrimInfrastructureFromStackTrace( new StackTrace( true ).ToString() );

                    activity.AddTag( "rock-db-stacktrace", stackTrace.Truncate( ObservabilityHelper.MaximumAttributeLength ) );

                    // Append parameters
                    var parameters = new StringBuilder();
                    foreach ( DbParameter parm in command.Parameters )
                    {
                        var keyValue = GetSqlParameterKeyValue( parm );

                        parameters.Append( $"{keyValue.Key}: {keyValue.Value}{Environment.NewLine}" );
                    }

                    activity.AddTag( "rock-db-parameters", parameters.ToString().Truncate( ObservabilityHelper.MaximumAttributeLength ) );
                }

                // Add observability metric
                var tags = RockMetricSource.CommonTags;
                tags.Add( "operation", observabilityInfo.CommandType );
                RockMetricSource.DatabaseQueriesCounter.Add( 1, tags );
            }

            if ( context is RockContext rockContext )
            {
                if ( rockContext.QueryMetricDetailLevel != QueryMetricDetailLevel.Off )
                {
                    _commandStartTimes.TryAdd( command, _stopwatch.ElapsedTicks );
                }
            }
        }

        /// <summary>
        /// Ends the timing.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="interceptionContext">The interception context.</param>
        private void EndTiming( DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext )
        {
            EndTiming( command, interceptionContext.DbContexts.FirstOrDefault() );
        }

        /// <summary>
        /// Ends the timing.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="interceptionContext">The interception context.</param>
        private void EndTiming( DbCommand command, DbCommandInterceptionContext<object> interceptionContext )
        {
            EndTiming( command, interceptionContext.DbContexts.FirstOrDefault() );
        }

        /// <summary>
        /// Ends the timing.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="interceptionContext">The interception context.</param>
        private void EndTiming( DbCommand command, DbCommandInterceptionContext<int> interceptionContext )
        {
            EndTiming( command, interceptionContext.DbContexts.FirstOrDefault() );
        }

        /// <summary>
        /// Ends the timing.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="context">The context.</param>
        private void EndTiming( DbCommand command, System.Data.Entity.DbContext context )
        {
            if ( context is RockContext rockContext )
            {
                var queryHash = command.CommandText.XxHash();
                var activity = Activity.Current;

                // Complete the observability activity if it is the correct
                // activity.
                if ( activity != null && activity.GetTagItem( "rock-db-hash" ) is string activityHash && queryHash == activityHash )
                {
                    activity.Dispose();
                }

                if ( rockContext.QueryMetricDetailLevel != QueryMetricDetailLevel.Off )
                {
                    long startTick;
                    _commandStartTimes.TryRemove( command, out startTick );

                    // Increment the count of the number of queries
                    rockContext.QueryCount++;

                    // If metric detail level is full then also record the SQL that ran.
                    if ( rockContext.QueryMetricDetailLevel == QueryMetricDetailLevel.Full )
                    {
                        var commandDuration = _stopwatch.ElapsedTicks - startTick;

                        rockContext.QueryMetricDetails.Add( new QueryMetricDetail
                        {
                            Sql = MergeSqlCommand( command ),
                            Duration = commandDuration,
                            Server = command.Connection.DataSource,
                            Database = command.Connection.Database
                        } );
                    }
                }
            }
        }

        /// <summary>
        /// Resolves the SQL in the command with parameters.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        private string MergeSqlCommand( DbCommand command )
        {
            string sql = command.CommandText;

            // Merge each parameter into the SQL string
            foreach ( DbParameter parm in command.Parameters )
            {
                var keyValue = GetSqlParameterKeyValue( parm );

                sql = sql.Replace( keyValue.Key, keyValue.Value );
            }

            return sql;
        }

        /// <summary>
        /// Parses a DbParameter into a key value pair.
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private (string Key, string Value) GetSqlParameterKeyValue( DbParameter parm )
        {
            var key = parm.ParameterName.AddStringAtBeginningIfItDoesNotExist( "@" );

            var value = parm.Value.ToString();

            // Check if the value needs to be quoted
            if ( _quoteRequiredFieldTypes.Contains( parm.DbType ) )
            {
                value = "'" + value.Replace( "'", "''" ) + "'";
            }

            return (key, value);
        }

        /// <summary>
        /// Trims the Entity Framework Infrastructure calls from stack trace.
        /// The stack trace will have a few frames from RockEfInterceptor and
        /// then a bunch of System.Data.Entity.* calls. After those it goes
        /// into user code for what caused the SQL command to execute. Because
        /// space is limited we remove these inner 8 or so calls which gives us
        /// an additional 8 or so stack traces of user code to diagnose where
        /// the real problem lies.
        /// </summary>
        /// <param name="stackTrace">The stack trace to be trimmed.</param>
        /// <returns>A string that contains the trimmed stack trace information.</returns>
        private static string TrimInfrastructureFromStackTrace( string stackTrace )
        {
            var stackTraceLines = stackTrace.Split( new string[] { Environment.NewLine }, StringSplitOptions.None );
            var trimmedStackTraceLines = new List<string>( stackTraceLines.Length );
            var trimming = false;

            for ( int i = 0; i < stackTraceLines.Length; i++ )
            {
                if ( !trimming )
                {
                    if ( stackTraceLines[i].StartsWith( "   at Rock.Data.Interception." ) )
                    {
                        trimmedStackTraceLines.Add( stackTraceLines[i] );
                    }
                    else
                    {
                        trimming = true;
                    }
                }
                else
                {
                    if ( stackTraceLines[i].StartsWith( "   at System.Data.Entity." ) )
                    {
                        continue;
                    }
                    else
                    {
                        trimmedStackTraceLines.Add( "   [trimmed]" );
                        trimmedStackTraceLines.AddRange( stackTraceLines.Skip( i ) );

                        return string.Join( Environment.NewLine, trimmedStackTraceLines );
                    }
                }
            }

            // Shouldn't ever really get here, but just in case return the
            // original stack trace.
            return stackTrace;
        }

        #endregion
    }
}
