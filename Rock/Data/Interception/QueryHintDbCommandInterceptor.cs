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
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using Rock.Logging;

namespace Rock.Data
{
    /// <summary>
    /// Used with Rock.Data.HintScope, appends a Query Hint to SQL statements executed within the HintScope
    /// some of this comes from http://stackoverflow.com/a/26762756/1755417
    /// </summary>
    public class QueryHintDbCommandInterceptor : DbCommandInterceptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryHintDbCommandInterceptor"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="hint">The hint.</param>
        public QueryHintDbCommandInterceptor( Rock.Data.DbContext dbContext, string hint )
        {
            this.DbContext = dbContext;
            this.Hint = GetSafeQueryHint( hint );

            if ( this.Hint == null && !string.IsNullOrWhiteSpace( hint ) )
            {
                RockLogger.LoggerFactory.CreateLogger( typeof( QueryHintDbCommandInterceptor ).FullName )
                    .LogWarning( "Query hint '{QueryHint}' is not an allowed query hint and was ignored.", hint );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryHintDbCommandInterceptor"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="hintType">Type of the hint.</param>
        public QueryHintDbCommandInterceptor( Rock.Data.DbContext dbContext, QueryHintType hintType )
            : this( dbContext, hintType.ConvertToString().Replace( "_", " " ) )
        {
            // intentionally blank
        }

        /// <summary>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        /// <inheritdoc />
        public override void ReaderExecuting( System.Data.Common.DbCommand command, DbCommandInterceptionContext<System.Data.Common.DbDataReader> interceptionContext )
        {
            if ( interceptionContext.DbContexts.Any( db => db == this.DbContext ) )
            {
                if ( !string.IsNullOrWhiteSpace( this.Hint ) )
                {
                    command.CommandText += string.Format( " OPTION ({0})", this.Hint );
                }
            }

            base.ReaderExecuting( command, interceptionContext );
        }

        /// <summary>
        /// Gets or sets the database context.
        /// </summary>
        /// <value>
        /// The database context.
        /// </value>
        private System.Data.Entity.DbContext DbContext { get; set; }

        /// <summary>
        /// Gets or sets the hint.
        /// </summary>
        /// <value>
        /// The hint.
        /// </value>
        private string Hint { get; set; }

        /// <summary>
        /// The query hints that are allowed to be appended to a SQL statement, built from <see cref="QueryHintType"/>.
        /// </summary>
        private static readonly HashSet<string> AllowedHints = new HashSet<string>(
            Enum.GetNames( typeof( QueryHintType ) ).Select( n => n.Replace( "_", " " ) ) );

        /// <summary>
        /// The MAXDOP query hint, which is allowed with a processor count of up to three digits.
        /// </summary>
        private static readonly Regex MaxDopPattern = new Regex( "^MAXDOP [0-9]{1,3}$" );

        /// <summary>
        /// Gets the normalized query hint, or <c>null</c> if it is empty or contains a hint that is not allowed.
        /// </summary>
        /// <param name="hint">The hint, which may contain multiple comma separated hints.</param>
        /// <returns>The normalized query hint or <c>null</c>.</returns>
        public static string GetSafeQueryHint( string hint )
        {
            if ( string.IsNullOrWhiteSpace( hint ) )
            {
                return null;
            }

            var hints = hint.Split( ',' )
                .Select( h => Regex.Replace( h.Trim(), @"\s+", " " ).ToUpperInvariant() )
                .ToList();

            if ( hints.Any( h => !AllowedHints.Contains( h ) && !MaxDopPattern.IsMatch( h ) ) )
            {
                return null;
            }

            return string.Join( ", ", hints );
        }
    }

    /// <summary>
    ///
    /// </summary>
    public enum QueryHintType
    {
        /// <summary>
        /// Use this to force SQL Server to recalculate the Query Plan for a Query. Can be handy in rare situations where SQL Server uses a cached plan that isn't optimal for the query.
        /// </summary>
        RECOMPILE,

        /// <summary>
        /// Instructs the query optimizer to use statistical data instead of the initial values of all parameter variables when determining the best execution plan. This sometimes helps queries that end up calling UDF functions and SQL Server gets confused :)
        /// </summary>
        OPTIMIZE_FOR_UNKNOWN,

        /// <summary>
        /// Instructs the query optimizer to keep the join order exactly as written in the query.
        /// </summary>
        FORCE_ORDER
    }
}
