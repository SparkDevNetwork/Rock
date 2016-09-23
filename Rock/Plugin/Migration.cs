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
using System.Data.Entity.Migrations.Builders;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace Rock.Plugin
{
    /// <summary>
    /// Class for defining a plugin migration
    /// </summary>
    public abstract class Migration : Rock.Data.IMigration
    {
        /// <summary>
        /// Gets or sets the SQL connection.
        /// </summary>
        /// <value>
        /// The SQL connection.
        /// </value>
        public virtual SqlConnection SqlConnection { get; set; }

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
        ///     Adds an operation to create a new table.  This is a wrapper for the default DBMigration CreateTable.
        /// </summary>
        /// <typeparam name="TColumns"> The columns in this create table operation. You do not need to specify this type, it will be inferred from the columnsAction parameter you supply. </typeparam>
        /// <param name="name"> The name of the table. Schema name is optional, if no schema is specified then dbo is assumed. </param>
        /// <param name="columnsAction"> An action that specifies the columns to be included in the table. i.e. t => new { Id = t.Int(identity: true), Name = t.String() } </param>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        /// <returns> An object that allows further configuration of the table creation operation. </returns>
        public TableBuilder<TColumns> CreateTable<TColumns>( string name, Func<ColumnBuilder, TColumns> columnsAction, object anonymousArguments = null )
        {
            DbMigration dbMigration = new DbMigration();
            var table = dbMigration.CreateTable( name, columnsAction, anonymousArguments );
            Sql( dbMigration.GetMigrationSql( SqlConnection ) );
            return table;
        }

        /// <summary>
        ///     Adds an operation to add a column to an existing table.  This is a wrapper for the default DBMigration AddColumn.
        /// </summary>
        /// <param name="table"> The name of the table to add the column to. Schema name is optional, if no schema is specified then dbo is assumed. </param>
        /// <param name="name"> The name of the column to be added. </param>
        /// <param name="columnAction"> An action that specifies the column to be added. i.e. c => c.Int(nullable: false, defaultValue: 3) </param>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        public void AddColumn( string table, string name, Func<ColumnBuilder, ColumnModel> columnAction, object anonymousArguments = null )
        {
            DbMigration dbMigration = new DbMigration();
            dbMigration.AddColumn( table, name, columnAction, anonymousArguments );
            Sql( dbMigration.GetMigrationSql( SqlConnection ) );
        }

        /// <summary>
        ///     Adds an operation to drop an existing column.  This is a wrapper for the default DBMigration DropColumn.
        /// </summary>
        /// <param name="table"> The name of the table to drop the column from. Schema name is optional, if no schema is specified then dbo is assumed. </param>
        /// <param name="name"> The name of the column to be dropped. </param>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        public void DropColumn( string table, string name, object anonymousArguments = null )
        {
            DbMigration dbMigration = new DbMigration();
            dbMigration.DropColumn( table, name, anonymousArguments );
            Sql( dbMigration.GetMigrationSql( SqlConnection ) );
        }

        /// <summary>
        ///     Adds an operation to drop an index based on its name.  This is a wrapper for the default DBMigration DropIndex.
        /// </summary>
        /// <param name="table"> The name of the table to drop the index from. Schema name is optional, if no schema is specified then dbo is assumed. </param>
        /// <param name="name"> The name of the index to be dropped. </param>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        public void DropIndex( string table, string name, object anonymousArguments = null )
        {
            DbMigration dbMigration = new DbMigration();
            dbMigration.DropIndex( table, name, anonymousArguments );
            Sql( dbMigration.GetMigrationSql( SqlConnection ) );
        }

        /// <summary>
        ///     Adds an operation to drop a foreign key constraint based on its name.  This is a wrapper for the default DBMigration DropForeignKey.
        /// </summary>
        /// <param name="dependentTable"> The table that contains the foreign key column. Schema name is optional, if no schema is specified then dbo is assumed. </param>
        /// <param name="name"> The name of the foreign key constraint in the database. </param>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        public void DropForeignKey( string dependentTable, string name, object anonymousArguments = null )
        {
            DbMigration dbMigration = new DbMigration();
            dbMigration.DropForeignKey( dependentTable, name, anonymousArguments );
            Sql( dbMigration.GetMigrationSql( SqlConnection ) );
        }

        /// <summary>
        /// Adds an operation to drop an index based on its name.  This is a wrapper for the default DBMigration DropIndex.
        /// </summary>
        /// <param name="table">The name of the table to drop the index from. Schema name is optional, if no schema is specified then dbo is assumed.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="anonymousArguments">Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.</param>
        public void DropIndex( string table, string[] columns, object anonymousArguments = null )
        {
            DbMigration dbMigration = new DbMigration();
            dbMigration.DropIndex( table, columns, anonymousArguments );
            Sql( dbMigration.GetMigrationSql( SqlConnection ) );
        }

        /// <summary>
        /// Adds an operation to drop an index based on the columns it targets.  This is a wrapper for the default DBMigration DropTable.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="anonymousArguments">Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.</param>
        public void DropTable( string name, object anonymousArguments = null )
        {
            DbMigration dbMigration = new DbMigration();
            dbMigration.DropTable( name, anonymousArguments );
            Sql( dbMigration.GetMigrationSql( SqlConnection ) );
        }
        
        /// <summary>
        /// This is a private instance of DBMigration which exposes the base DBMigration methods for plugins to use.
        /// </summary>
        private class DbMigration : System.Data.Entity.Migrations.DbMigration
        {
            public override void Up()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Wrapper for the base CreateTable method to expose it to the parent class.
            /// </summary>
            internal new TableBuilder<TColumns> CreateTable<TColumns>( string name, Func<ColumnBuilder, TColumns> columnsAction, object anonymousArguments = null )
            {
                return base.CreateTable( name, columnsAction, anonymousArguments );
            }

            /// <summary>
            /// Wrapper for the base AddColumn method to expose it to the parent class.
            /// </summary>
            internal new void AddColumn( string table, string name, Func<ColumnBuilder, ColumnModel> columnAction, object anonymousArguments = null )
            {
                base.AddColumn( table, name, columnAction, anonymousArguments );
            }

            /// <summary>
            /// Wrapper for the base DropColumn method to expose it to the parent class.
            /// </summary>
            internal new void DropColumn( string table, string name, object anonymousArguments = null )
            {
                base.DropColumn( table, name, anonymousArguments );
            }

            /// <summary>
            /// Wrapper for the base DropForeignKey method to expose it to the parent class.
            /// </summary>
            internal new void DropForeignKey( string dependentTable, string name, object anonymousArguments = null )
            {
                base.DropForeignKey( dependentTable, name, anonymousArguments );
            }

            /// <summary>
            /// Wrapper for the base DropIndex method to expose it to the parent class.
            /// </summary>
            internal new void DropIndex( string table, string name, object anonymousArguments = null )
            {
                base.DropIndex( table, name, anonymousArguments );
            }

            /// <summary>
            /// Wrapper for the base DropIndex method to expose it to the parent class.
            /// </summary>
            internal new void DropIndex( string table, string[] columns, object anonymousArguments = null )
            {
                base.DropIndex( table, columns, anonymousArguments );
            }

            /// <summary>
            /// Wrapper for the base DropTable method to expose it to the parent class.
            /// </summary>
            internal new void DropTable( string name, object anonymousArguments = null )
            {
                base.DropTable( name, anonymousArguments );
            }

            /// <summary>
            /// Get the migration operation's Sql.  This iterates through the DbMigrations operations list and pulls out the sql.
            /// </summary>
            /// <param name="SqlConnection">The SqlConnection object from the Plugin Migration class.</param>
            /// <returns>A string containing the SQL generated from the current migration.</returns>
            internal string GetMigrationSql( SqlConnection SqlConnection )
            {
                StringBuilder sql = new StringBuilder();
                var prop = this.GetType().GetProperty( "Operations", BindingFlags.NonPublic | BindingFlags.Instance );
                if ( prop != null )
                {
                    IEnumerable<MigrationOperation> operations = prop.GetValue( this ) as IEnumerable<MigrationOperation>;
                    foreach ( var operation in operations )
                    {
                        if ( operation is AddForeignKeyOperation && ( ( AddForeignKeyOperation ) operation ).PrincipalColumns.Count == 0 )
                        {
                            // In Rock, the principal column should always be the Id.  This isn't always the case . . . .
                            ( ( AddForeignKeyOperation ) operation ).PrincipalColumns.Add( "Id" );
                        }
                    }
                    var generator = new SqlServerMigrationSqlGenerator();
                    var statements = generator.Generate( operations, SqlConnection.ServerVersion.AsInteger() > 10 ? "2008" : "2005" );
                    foreach ( MigrationStatement item in statements )
                    {
                        sql.Append( item.Sql );
                    }
                }
                return sql.ToString();
            }

        }
    }
}