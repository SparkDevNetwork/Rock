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
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Rock.Plugin
{
    /// <summary>
    /// Class for defining a plugin migration
    /// </summary>
    public abstract class Migration : Rock.Data.IMigration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public abstract void Up();

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public abstract void Down();

        /// <summary>
        /// Gets the rock migration helper.
        /// </summary>
        /// <value>
        /// The rock migration helper.
        /// </value>
        public Rock.Data.MigrationHelper RockMigrationHelper
        {
            get
            {
                if ( _migrationHelper == null )
                {
                    _migrationHelper = new Rock.Data.MigrationHelper( this );
                }
                return _migrationHelper;
            }
        }
        private Rock.Data.MigrationHelper _migrationHelper = null;

        /// <summary>
        /// Executes a sql statement
        /// </summary>
        /// <param name="sql">The SQL.</param>
        public void Sql( string sql )
        {
            var configConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["RockContext"];
            if ( configConnectionString != null )
            {
                string connectionString = configConnectionString.ConnectionString;
                if ( !string.IsNullOrWhiteSpace( connectionString ) )
                {
                    using ( SqlConnection con = new SqlConnection( connectionString ) )
                    {
                        con.Open();
                        using ( SqlCommand sqlCommand = new SqlCommand( sql, con ) )
                        {
                            sqlCommand.CommandType = CommandType.Text;
                            sqlCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
