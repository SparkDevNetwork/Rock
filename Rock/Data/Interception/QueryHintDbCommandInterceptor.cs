// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;

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
            this.Hint = hint;
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
        private Rock.Data.DbContext DbContext { get; set; }

        /// <summary>
        /// Gets or sets the hint.
        /// </summary>
        /// <value>
        /// The hint.
        /// </value>
        private string Hint { get; set; }
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
        OPTIMIZE_FOR_UNKNOWN
    }
}
