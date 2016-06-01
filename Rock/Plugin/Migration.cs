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
using System.Data;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Reflection;

namespace Rock.Plugin
{
    /// <summary>
    /// Class for defining a plugin migration
    /// </summary>
    public abstract class Migration : DbMigration, Rock.Data.IMigration
    {
        /// <summary>
        /// Gets or sets the SQL connection.
        /// </summary>
        /// <value>
        /// The SQL connection.
        /// </value>
        public virtual SqlConnection SqlConnection { get; set;}

        /// <summary>
        /// Gets or sets the SQL transaction.
        /// </summary>
        /// <value>
        /// The SQL transaction.
        /// </value>
        public virtual SqlTransaction SqlTransaction { get; set; }

        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override abstract void Up();

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override abstract void Down();

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
            if ( SqlConnection != null || SqlTransaction != null )
            {
                using ( SqlCommand sqlCommand = new SqlCommand( sql, SqlConnection, SqlTransaction ) )
                {
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.ExecuteNonQuery();
                }
            }
            else
            {
                throw new NullReferenceException( "The Plugin Migration requires valid SqlConnection and SqlTransaction values when executing SQL" );
            }
        }

        /// <summary>
        /// Run the migration operations.  This iterates through the DbMigrations operations list and actually applies them.
        /// </summary>
        public void RunMigration()
        {
            var prop = this.GetType().GetProperty("Operations", BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null)
            {
                IEnumerable<MigrationOperation> operations = prop.GetValue(this) as IEnumerable<MigrationOperation>;
                foreach (var operation in operations)
                {
                    if (operation is AddForeignKeyOperation && ((AddForeignKeyOperation)operation).PrincipalColumns.Count == 0)
                    {
                        // In Rock, the principal column should always be the Id.  This isn't always the case . . . .
                        ((AddForeignKeyOperation)operation).PrincipalColumns.Add("Id");
                    }
                }
                var generator = new SqlServerMigrationSqlGenerator();
                var statements = generator.Generate(operations, SqlConnection.ServerVersion.AsInteger() > 10 ? "2008" : "2005");
                foreach (MigrationStatement item in statements)
                {
                    Sql(item.Sql);
                }
            }
        }
    }
}
