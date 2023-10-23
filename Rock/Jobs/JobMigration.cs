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
    internal class JobMigration : IMigration
    {
        private readonly int _commandTimeout;
        private readonly RockContext _rockContext;

        public JobMigration( int commandTimeout )
        {
            _commandTimeout = commandTimeout;
        }

        /// <summary>
        /// Create a Job Migration Class out of a RockContext
        /// </summary>
        /// <param name="rockContext">The RockContext to be used to make the SQL commands. Please ensure that the rockContext has the commandTimeout set.</param>
        public JobMigration( RockContext rockContext)
        {
            _rockContext = rockContext;
        }

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

        public object SqlScalar( string sql )
        {
            return DbService.ExecuteScalar( sql );
        }
    }
}
