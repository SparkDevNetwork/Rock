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
using System.Collections.Generic;

using Rock.Data;

namespace Rock.Jobs
{
    /// <summary>
    /// A helper class to execute SQL commands within Rock jobs.
    /// </summary>
    internal class JobMigration : IMigration
    {
        private readonly int _commandTimeout = 14400; // Default: 4 hours.
        private readonly RockContext _rockContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobMigration"/> class to use <see cref="DbService"/> along with
        /// the specified command timeout for all SQL commands.
        /// </summary>
        /// <param name="commandTimeout">The command timeout to use for all SQL commands.</param>
        public JobMigration( int commandTimeout )
        {
            _commandTimeout = commandTimeout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobMigration"/> class to use the specified <see cref="RockContext"/>
        /// for all SQL commands.
        /// </summary>
        /// <param name="rockContext">The <see cref="RockContext"/> to be used for all SQL commands.</param>
        /// <remarks>
        /// Ensure <paramref name="rockContext"/> has the appropriate command timeout set before executing any commands.
        /// </remarks>
        public JobMigration( RockContext rockContext )
        {
            _rockContext = rockContext;
        }

        /// <summary>
        /// Executes the specified SQL command.
        /// </summary>
        /// <param name="sql">The SQL command.</param>
        public void Sql( string sql )
        {
            if ( _rockContext == null )
            {
                DbService.ExecuteCommand( sql, commandTimeout: _commandTimeout );
            }
            else
            {
                _rockContext.Database.ExecuteSqlCommand( sql );
            }
        }

        /// <summary>
        /// Executes the specified SQL command with the specified parameters.
        /// </summary>
        /// <param name="sql">The SQL command.</param>
        /// <param name="parameters">The parameters to use in the SQL command.</param>
        public void Sql( string sql, Dictionary<string, object> parameters )
        {
            if ( _rockContext == null )
            {
                DbService.ExecuteCommand( sql, parameters: parameters, commandTimeout: _commandTimeout );
            }
            else
            {
                _rockContext.Database.ExecuteSqlCommand( sql, parameters );
            }
        }

        /// <summary>
        /// Executes the specified SQL query and returns the first column of the first row in the result set returned by the query.
        /// </summary>
        /// <param name="sql">The SQL query.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        /// <remarks>
        /// EF6 doesn't provide an easy way to perform a simple scalar read, so all queries of this type will be run directly
        /// through <see cref="DbService"/> using the <see cref="JobMigration"/>'s command timeout (default 4 hours).
        /// </remarks>
        public object SqlScalar( string sql )
        {
            return DbService.ExecuteScalar( sql, commandTimeout: _commandTimeout );
        }
    }
}
