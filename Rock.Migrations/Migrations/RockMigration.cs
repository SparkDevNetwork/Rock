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
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;

using Rock;
using Rock.Model;


namespace Rock.Migrations
{
    /// <summary>
    /// Custom Migration methods
    /// </summary>
    public abstract class RockMigration : DbMigration, Rock.Data.IMigration
    {
        /// <summary>
        /// Gets the migration helper.
        /// </summary>
        /// <value>
        /// The migration helper.
        /// </value>
        public Rock.Data.MigrationHelper RockMigrationHelper
        {
            get
            {
                if (_migrationHelper == null)
                {
                    _migrationHelper = new Rock.Data.MigrationHelper( this );
                }
                return _migrationHelper;
            }
        }
        private Rock.Data.MigrationHelper _migrationHelper = null;

        /// <summary>
        /// Adds an operation to execute a SQL command.  Entity Framework Migrations
        /// APIs are not designed to accept input provided by untrusted sources (such
        /// as the end user of an application). If input is accepted from such sources
        /// it should be validated before being passed to these APIs to protect against
        /// SQL injection attacks etc.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        public void Sql(string sql)
        {
            Sql(sql, false, null);
        }
   }
}