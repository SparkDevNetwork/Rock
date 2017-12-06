// <copyright>
// Copyright by Central Christian Church
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
using System.Text;
using System.Data.Entity;
using System.Linq;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Data;
using Rock.Communication;

namespace com.centralaz.GeneralJobs.Jobs
{
    /// <summary>
    /// This job will send the output of your SQL as formatted HTML (via Lava) to all active group members 
    /// of the specified group, with the option to also send it to members of descendant groups. 
    /// If a person is a member of multiple groups in the tree they will receive an email for each group.
    /// </summary>
    [CodeEditorField( "SQL Script", "The Structured Query Language (SQL) script to execute.", Rock.Web.UI.Controls.CodeEditorMode.Sql, required: true )]
    [CodeEditorField( "Formatted Output", "Optional formatting for the returned results.  If left blank, a simple table will be generated. Example: {% for row in rows %} {{ row.FirstName }}<br/> {% endfor %} <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        Rock.Web.UI.Controls.CodeEditorMode.Lava, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false )]
    [CodeEditorField( "Output Results Description", "If provided, this description (HTML) will be added at the top of the email, before the results. <span class='tip tip-html'></span>", Rock.Web.UI.Controls.CodeEditorMode.Html, required: false )]

    [EmailField("Email From", "The email address to use as the From: field of the email that will be sent." )]
    [TextField( "Email Subject", "The Subject: field for the email that will be sent." )]
    [GroupField( "Group", "The group the email will be sent to." )]
    [BooleanField( "Send To Descendant Groups", "Determines if the email will be sent to descendant groups." )]
    [BooleanField( "Include Link To Group", "If checked, a link will be added (at the bottom of the report) so the group distribution list can be edited.", true )]
    [IntegerField("Timeout", "The timeout for execution of the SQL.", false, defaultValue: 180 )]
    [DisallowConcurrentExecution]
    public class SqlToLavaEmail : IJob
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlToLavaEmail"/> class.
        /// </summary>
        public SqlToLavaEmail()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            var sql = dataMap.Get( "SQLScript" ).ToString();
            var formattedOutput = dataMap.Get( "FormattedOutput" ).ToStringSafe();
            string outputResultsDescription = dataMap.Get( "OutputResultsDescription" ).ToStringSafe();
            string emailFrom = dataMap.Get( "EmailFrom" ).ToStringSafe();
            string emailSubject = dataMap.Get( "EmailSubject" ).ToStringSafe();
            var timeout = dataMap.Get( "Timeout" ).ToStringSafe().AsIntegerOrNull();
            var groupGuid = dataMap.Get( "Group" ).ToString().AsGuid();
            var sendToDescendants = dataMap.Get( "SendToDescendantGroups" ).ToString().AsBoolean();
            var includeLinkToGroup = dataMap.Get( "IncludeLinkToGroup" ).ToString().AsBoolean();

            var rockContext = new RockContext();
            var group = new GroupService( rockContext ).Get( groupGuid );
            if ( group != null )
            {
                List<int> groupIds = new List<int>();
                GetGroupIds( groupIds, sendToDescendants, group );

                var recipients = new List<string>();

                var groupMemberList = new GroupMemberService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( gm => groupIds.Contains( gm.GroupId ) &&
                        gm.GroupMemberStatus == GroupMemberStatus.Active )
                    .ToList();

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                string message = BuildOutput( sql, formattedOutput, mergeFields, timeout );

                // Prefix the message with the output result description if one was given
                if ( ! string.IsNullOrWhiteSpace( outputResultsDescription ) )
                {
                    message = outputResultsDescription + message;
                }

                if ( includeLinkToGroup )
                {
                    message = message + BuildLinkToGroup( rockContext, group, sendToDescendants );
                }

                foreach ( GroupMember groupMember in groupMemberList )
                {
                    recipients.Add( groupMember.Person.Email );
                }

                var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "PublicApplicationRoot" );
                Email.Send( emailFrom, emailSubject, recipients, message, appRoot );
                context.Result = string.Format( "{0} emails sent", recipients.Count() );
            }
        }

        /// <summary>
        /// Builds the output for the given SQL, format and merge fields.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="formattedOutput">The formatted output (Lava).</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        private string BuildOutput( string sql, string formattedOutput, Dictionary<string, object> mergeFields, int? timeout )
        {
            string errorMessage = string.Empty;

            // load merge objects if needed by either for formatted output OR page title
            var dataSet = GetData( sql, timeout, out errorMessage );

            if ( !string.IsNullOrWhiteSpace( formattedOutput ) )
            {
                int i = 1;

                // Formatted output needs all the rows, so get the data regardless of the setData parameter
                foreach ( DataTable dataTable in dataSet.Tables )
                {
                    var dropRows = new List<DataRowDrop>();
                    foreach ( DataRow row in dataTable.Rows )
                    {
                        dropRows.Add( new DataRowDrop( row ) );
                    }

                    if ( dataSet.Tables.Count > 1 )
                    {
                        var tableField = new Dictionary<string, object>();
                        tableField.Add( "rows", dropRows );
                        mergeFields.Add( "table" + i.ToString(), tableField );
                    }
                    else
                    {
                        mergeFields.Add( "rows", dropRows );
                    }
                    i++;
                }

                formattedOutput = formattedOutput.ResolveMergeFields( mergeFields );
            }
            else
            {
                foreach ( DataTable dataTable in dataSet.Tables )
                {
                    formattedOutput += ConvertDataTableToHTML( dataTable );
                }
            }

            return formattedOutput;
        }

        /// <summary>
        /// Builds the link to group details.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="group">The group.</param>
        /// <param name="sendToDescendants">if set to <c>true</c> [send to descendants].</param>
        /// <returns></returns>
        private string BuildLinkToGroup( RockContext rockContext, Group group, bool sendToDescendants )
        {
            var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "InternalApplicationRoot" );

            string html = string.Format( "<p><i>This report was sent to the members of the '{0}' group{1}. <a href='{2}Group/{3}'>Click to add or remove people</a> to receive this report.</i></p>",
                group.Name,
                ( sendToDescendants ) ? " or one of the child groups of that group" : string.Empty,
                appRoot,
                group.Id );
            return html;
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="sql">The SQL to execute.</param>
        /// <param name="timeout">The timeout for the execution.</param>
        /// <param name="errorMessage">The error message (if any).</param>
        /// <returns></returns>
        private DataSet GetData( string sql, int? timeout, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( !string.IsNullOrWhiteSpace( sql ) )
            {
                try
                {
                    return DbService.GetDataSet( sql, CommandType.Text, null, timeout );
                }
                catch ( System.Exception ex )
                {
                    errorMessage = ex.Message;
                }
            }

            return null;
        }

        /// <summary>
        /// Converts the data table to HTML.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <returns>a string containing the table HTML.</returns>
        public static string ConvertDataTableToHTML( DataTable dataTable )
        {
            StringBuilder sb = new StringBuilder();
            var altRowStyle = "style='background-color: #f9f9f9;'";

            sb.Append( "<table rules='all' style='border: 1px solid #dddddd; font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif;' cellpadding='8'><tr>" );
            for ( int i = 0; i < dataTable.Columns.Count; i++ )
            {
                sb.Append( "<th style='border-bottom: 2px solid #dddddd; font-weight: bold;'>" + dataTable.Columns[i].ColumnName + "</th>" );
            }
            sb.Append( "</tr>" );

            for ( int i = 0; i < dataTable.Rows.Count; i++ )
            {
                sb.Append( "<tr " + ( i % 2 == 0 ? altRowStyle : "" ) + ">" );
                for ( int j = 0; j < dataTable.Columns.Count; j++ )
                {
                    sb.Append( "<td>" + dataTable.Rows[i][j].ToString() + "</td>" );
                }
                sb.Append( "</tr>" );
            }

            sb.Append( "</table>" );
            return sb.ToString();
        }

        /// <summary>
        /// Gets the group ids.
        /// </summary>
        /// <param name="groupIds">The group ids.</param>
        /// <param name="sendToDescendants">if set to <c>true</c> [send to descendants].</param>
        /// <param name="group">The group.</param>
        private void GetGroupIds( List<int> groupIds, bool sendToDescendants, Group group )
        {
            groupIds.Add( group.Id );

            if ( sendToDescendants )
            {
                foreach ( var childGroup in group.Groups )
                {
                    GetGroupIds( groupIds, sendToDescendants, childGroup );
                }
            }
        }

        /// <summary>
        /// A DataRowDrop for use with Lava merge fields.
        /// </summary>
        /// <seealso cref="DotLiquid.Drop" />
        private class DataRowDrop : DotLiquid.Drop
        {
            private readonly DataRow _dataRow;

            public DataRowDrop( DataRow dataRow )
            {
                _dataRow = dataRow;
            }

            public override object BeforeMethod( string method )
            {
                if ( _dataRow.Table.Columns.Contains( method ) )
                {
                    return _dataRow[method];
                }

                return null;
            }
        }
    }
}