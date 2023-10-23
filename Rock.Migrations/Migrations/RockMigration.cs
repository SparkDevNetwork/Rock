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
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using Rock.Data;


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
                if ( _migrationHelper == null )
                {
                    _migrationHelper = new Rock.Data.MigrationHelper( this );
                }
                return _migrationHelper;
            }
        }
        private Rock.Data.MigrationHelper _migrationHelper = null;

        /// <summary>
        ///  Contains embedded SQL files that be used in migrations
        ///  to Add SQL files, name the SQL files so it starts with the Migration name, 
        ///  put them in the migrations folder, then add it to RockMigrationSQL.resx
        /// </summary>
        /// <value>
        /// The resources.
        /// </value>
        public class MigrationSQL : Rock.Migrations.Migrations.RockMigrationSQL { };

        /// <summary>
        /// Adds an operation to execute a SQL command.  Entity Framework Migrations
        /// APIs are not designed to accept input provided by untrusted sources (such
        /// as the end user of an application). If input is accepted from such sources
        /// it should be validated before being passed to these APIs to protect against
        /// SQL injection attacks etc.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        public void Sql( string sql )
        {
            Sql( sql, false, null );
        }

        /// <summary>
        /// Runs the SQL found in a file.
        /// </summary>
        /// <param name="sqlFile">The file the SQL can be found it relative to the application path.</param>
        public void SqlFile( string sqlFile )
        {
            // append application root
            sqlFile = EfMapPath( sqlFile );

            string script = File.ReadAllText( sqlFile );
            using ( var rockContext = new RockContext() )
            {
                Sql( script );

                // delete file if being run in 'production'
                if ( HttpContext.Current != null )
                {
                    File.Delete( sqlFile );

                    // delete directory if it's empty
                    if ( Directory.GetFiles( Path.GetDirectoryName( sqlFile ) ).Length == 0 )
                    {
                        Directory.Delete( Path.GetDirectoryName( sqlFile ) );
                    }
                }
            }
        }

        /// <summary>
        /// Efs the map path.
        /// </summary>
        /// <param name="seedFile">The seed file.</param>
        /// <returns></returns>
        private string EfMapPath( string seedFile )
        {
            if ( HttpContext.Current != null )
            {
                return HostingEnvironment.MapPath( seedFile );
            }

            var absolutePath = new Uri( Assembly.GetExecutingAssembly().CodeBase ).AbsolutePath;
            var directoryName = Path.GetDirectoryName( absolutePath ).Replace( "Rock.Migrations\\bin", "RockWeb" );
            var path = Path.Combine( directoryName, ".." + seedFile.TrimStart( '~' ).Replace( '/', '\\' ) );

            return path;
        }

        /// <summary>
        /// Adds an operation to execute a SQL command that returns a scalar value.  Entity Framework Migrations
        /// APIs are not designed to accept input provided by untrusted sources (such
        /// as the end user of an application). If input is accepted from such sources
        /// it should be validated before being passed to these APIs to protect against
        /// SQL injection attacks etc.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        public object SqlScalar( string sql )
        {
            return DbService.ExecuteScalar( sql );
        }

        /// <summary>
        /// Launches the Visual Studio debugger and set a code break point so an individual migration can be debugged.
        /// </summary>
        [DebuggerStepThrough]
        public void SetDebuggerBreakPoint()
        {
            if ( System.Diagnostics.Debugger.IsAttached == false )
            {
                System.Diagnostics.Debugger.Launch();
            }

            if ( System.Diagnostics.Debugger.IsAttached )
            {
                System.Diagnostics.Debugger.Break();
            }
        }
    }
}