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
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Diagnostics;
using System.Linq;
using System.Text;

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
        public static double _callMSTotal = 0.00;

        private class DebugHelperUserState
        {
            public int CallNumber { get; set; }
            public Stopwatch Stopwatch { get; set; }
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
            internal System.Data.Entity.DbContext DbContext { get; set; }

            /// <summary>
            /// </summary>
            /// <param name="command"></param>
            /// <param name="interceptionContext"></param>
            /// <inheritdoc />
            public override void NonQueryExecuting( DbCommand command, DbCommandInterceptionContext<int> interceptionContext )
            {
                object userState;
                this.CommandExecuting( command, interceptionContext, out userState );
                interceptionContext.UserState = userState;
            }

            /// <summary>
            /// </summary>
            /// <param name="command"></param>
            /// <param name="interceptionContext"></param>
            /// <inheritdoc />
            public override void NonQueryExecuted( DbCommand command, DbCommandInterceptionContext<int> interceptionContext )
            {
                this.CommandExecuted( command, interceptionContext, interceptionContext.UserState );
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
                interceptionContext.UserState = userState;
            }

            /// <summary>
            /// </summary>
            /// <param name="command"></param>
            /// <param name="interceptionContext"></param>
            /// <inheritdoc />
            public override void ScalarExecuted( DbCommand command, DbCommandInterceptionContext<object> interceptionContext )
            {
                this.CommandExecuted( command, interceptionContext, interceptionContext.UserState );
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
                interceptionContext.UserState = userState;
            }

            /// <summary>
            /// </summary>
            /// <param name="command"></param>
            /// <param name="interceptionContext"></param>
            /// <inheritdoc />
            public override void ReaderExecuted( DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext )
            {
                this.CommandExecuted( command, interceptionContext, interceptionContext.UserState );
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
                if ( this.DbContext != null && !interceptionContext.DbContexts.Any( a => a == this.DbContext ) )
                {
                    return;
                }

                DebugHelper._callCounts++;

                StringBuilder sbDebug = new StringBuilder();

                sbDebug.AppendLine( "\n" );

                StackTrace st = new StackTrace( 2, true );
                var frames = st.GetFrames().Where( a => a.GetFileName() != null );

                sbDebug.AppendLine( string.Format( "/* Call# {0}*/", DebugHelper._callCounts ) );

                sbDebug.AppendLine( string.Format( "/*\n{0}*/", frames.ToList().AsDelimited( "" ) ) );

                sbDebug.AppendLine( "BEGIN\n" );

                var declares = command.Parameters.OfType<System.Data.SqlClient.SqlParameter>()
                    .Select( p =>
                    {
                        if ( p.SqlDbType == System.Data.SqlDbType.NVarChar )
                        {
                            return string.Format( "@{0} {1}({2}) = '{3}'", p.ParameterName, p.SqlDbType, p.Size, p.SqlValue.ToString().Replace( "'", "''" ) );
                        }

                        if ( p.SqlDbType == System.Data.SqlDbType.Int )
                        {
                            return string.Format( "@{0} {1} = {2}", p.ParameterName, p.SqlDbType, p.SqlValue ?? "null" );
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

                System.Diagnostics.Debug.Write( sbDebug.ToString() );

                var sqlConnection = command.Connection as System.Data.SqlClient.SqlConnection;

                sqlConnection.StatisticsEnabled = true;
                sqlConnection.ResetStatistics();

                if ( userState == null )
                {
                    userState = new DebugHelperUserState { CallNumber = DebugHelper._callCounts, Stopwatch = Stopwatch.StartNew() };
                }
            }

            /// <summary>
            /// Readers the executed.
            /// </summary>
            /// <param name="command">The command.</param>
            /// <param name="interceptionContext">The interception context.</param>
            /// <param name="userState">State of the user.</param>
            public void CommandExecuted( System.Data.Common.DbCommand command, DbCommandInterceptionContext interceptionContext, object userState )
            {
                var debugHelperUserState = userState as DebugHelperUserState;
                if ( debugHelperUserState != null )
                {
                    debugHelperUserState.Stopwatch.Stop();

                    var sqlConnection = command.Connection as System.Data.SqlClient.SqlConnection;

                    var stats = sqlConnection.RetrieveStatistics();
                    sqlConnection.StatisticsEnabled = false;
                    var commandExecutionTimeInMs = ( long ) stats["ExecutionTime"];

                    System.Diagnostics.Debug.Write( $"\n/* Call# {debugHelperUserState.CallNumber}: ElapsedTime [{debugHelperUserState.Stopwatch.Elapsed.TotalMilliseconds}ms], SQLConnection.Statistics['ExecutionTime'] = [{commandExecutionTimeInMs}ms] */\n" );
                    _callMSTotal += debugHelperUserState.Stopwatch.Elapsed.TotalMilliseconds;
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
            _callMSTotal = 0.00;
            SQLLoggingStop();
            _debugLoggingDbCommandInterceptor.DbContext = dbContext;
            DbInterception.Add( _debugLoggingDbCommandInterceptor );
        }

        /// <summary>
        /// Stops logging all EF SQL Calls to the Debug Output Window
        /// </summary>
        public static void SQLLoggingStop()
        {
            if ( _callCounts != 0 )
            {
                Debug.WriteLine( $"####SQLLogging Summary: _callCounts:{_callCounts}, _callMSTotal:{_callMSTotal}, _callMSTotal/_callCounts:{_callMSTotal / _callCounts}####" );
            }

            DbInterception.Remove( _debugLoggingDbCommandInterceptor );
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
                DebugHelper.SQLLoggingStop();
            }

        }
    }
}
