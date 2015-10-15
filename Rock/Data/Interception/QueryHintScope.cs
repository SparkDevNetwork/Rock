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
using System;
using System.Data.Entity.Infrastructure.Interception;

namespace Rock.Data
{
    /// <summary>
    /// QueryHintScope allows a developer to specify a SQL Server Query Hint to apply to all queries executed within the scope.
    /// NOTE: Make sure to use a USING { } pattern to prevent the query hint from getting added to other queries that fire off later
    /// </summary>
    /// <example>
    /// using ( new QueryHintScope( rockContext, QueryHintType.RECOMPILE ) )
    /// {
    ///     gTransactions.SetLinqDataSource( qry.AsNoTracking() );
    /// }
    /// </example>
    public class QueryHintScope : IDisposable
    {
        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        private Rock.Data.DbContext Context { get; set; }

        /// <summary>
        /// The _hint interceptor
        /// </summary>
        private QueryHintDbCommandInterceptor _hintInterceptor;

        /// <summary>
        /// QueryHintScope allows a developer to specify a SQL Server Query Hint to apply to all queries executed within the scope.
        /// NOTE: Make sure to use a USING { } pattern to prevent the query hint from getting added to other queries that fire off later
        /// </summary>
        /// <example>
        /// using ( new QueryHintScope( rockContext, QueryHintType.RECOMPILE ) )
        /// {
        ///     gTransactions.SetLinqDataSource( qry.AsNoTracking() );
        /// }
        /// </example>
        /// <param name="context">The context.</param>
        /// <param name="hintType">The hint.</param>
        public QueryHintScope( Rock.Data.DbContext context, QueryHintType hintType ) 
            : this( context, hintType.ConvertToString().Replace( "_", " " ) ) 
        { 
            // intentionally blank
        }

        /// <summary>
        /// QueryHintScope allows a developer to specify a SQL Server Query Hint to apply to all queries executed within the scope.
        /// NOTE: Make sure to use a USING { } pattern to prevent the query hint from getting added to other queries that fire off later
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="hint">The hint.</param>
        public QueryHintScope( Rock.Data.DbContext context, string hint )
        {
            _hintInterceptor = new QueryHintDbCommandInterceptor( context, hint);
            DbInterception.Add( _hintInterceptor );
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            DbInterception.Remove( _hintInterceptor );
        }
    }

    
}
