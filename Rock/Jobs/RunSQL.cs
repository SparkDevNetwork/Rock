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
using System.ComponentModel;
using System.Web;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Jobs
{
    /// <summary>
    /// This job runs quick SQL queries on a schedule.
    /// </summary>
    [DisplayName( "Run SQL" )]
    [Description( "This job runs quick SQL queries on a schedule." )]

    [CodeEditorField( "SQL Query", "SQL query to run", CodeEditorMode.Sql, CodeEditorTheme.Rock, 200, true, "", "General", 0, "SQLQuery" )]
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for the SQL Query to complete. Leave blank to use the SQL default (30 seconds).", false, 180, "General", 1, "CommandTimeout")]
    public class RunSQL : RockJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public RunSQL()
        {
        }

        /// <summary>
        /// Job that will run quick SQL queries on a schedule.
        /// </summary>
        public override void Execute()
        {
            
            // run a SQL query to do something
            string query = GetAttributeValue( "SQLQuery" );
            int? commandTimeout = GetAttributeValue( "CommandTimeout").AsIntegerOrNull();
            try
            {
                int rows = DbService.ExecuteCommand( query, System.Data.CommandType.Text, null, commandTimeout );
            }
            catch ( System.Exception ex )
            {
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( ex, context2 );
                throw;
            }
        }

    }
}
