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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;

namespace Rock.Jobs
{
    /// <summary>
    /// This job synchronizes the members of a group with the people in a Rock data view based on
    /// the configuration of data view and role found in the group. It is also responsible for
    /// sending any ExitSystemEmail or WelcomeSystemEmail as well as possibly creating any
    /// user login for the person.
    ///
    /// It should adhere to the following truth table:
    ///
    ///     In         In Group   In Group
    ///     DataView   Archived   !Archived   Result
    ///     --------   --------   ---------   ----------------------------
    ///            0          0           0   do nothing
    ///            0          0           1   remove from group
    ///            0          1           0   do nothing
    ///            1          0           0   add to group
    ///            1          0           1   do nothing
    ///            1          1           0   change IsArchived to false, unless they are also already in the group as Unarchived
    ///
    /// NOTE: It should do this regardless of the person's IsDeceased flag.
    /// NOTE: The job can sync new people at about 45/sec or 2650/minute.
    /// </summary>
    [DisplayName( "Group Sync" )]
    [Description( "Processes groups that are marked to be synced with a data view." )]

    [BooleanField( "Require Password Reset On New Logins", "Determines if new logins will require the individual to reset their password on the first log in.", Key = "RequirePasswordReset" )]
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for each operation to complete. Leave blank to use the default for this job (180).", false, 3 * 60, "General", 1, "CommandTimeout" )]
    public class GroupSync : RockJob
    {
        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public GroupSync()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            // Get the job setting(s)   
            bool requirePasswordReset = GetAttributeValue( "RequirePasswordReset" ).AsBoolean();
            var commandTimeout = GetAttributeValue( "CommandTimeout" ).AsIntegerOrNull() ?? 180;

            // Counters for displaying results
            int groupsSynced = 0;
            int groupsChanged = 0;
            string groupName = string.Empty;
            string dataViewName = string.Empty;
            var errors = new List<string>();

            try
            {
                // get groups set to sync
                var activeSyncList = new List<GroupSyncInfo>();
                using ( var rockContextReadOnly = new RockContextReadOnly() )
                {
                    // Get groups that are not archived and are still active.
                    activeSyncList = new GroupSyncService( rockContextReadOnly )
                        .Queryable()
                        .AsNoTracking()
                        .AreNotArchived()
                        .AreActive()
                        .NeedToBeSynced()
                        .Select( x => new GroupSyncInfo { SyncId = x.Id, GroupName = x.Group.Name } )
                        .ToList();
                }

                GroupSyncService.SyncGroups( activeSyncList, commandTimeout, requirePasswordReset, out errors, out groupsChanged, out groupsSynced, UpdateLastStatusMessage );

                // Format the result message
                var resultMessage = string.Empty;
                if ( groupsSynced == 0 )
                {
                    resultMessage = "No groups to sync";
                }
                else if ( groupsSynced == 1 )
                {
                    resultMessage = "1 group was synced";
                }
                else
                {
                    resultMessage = string.Format( "{0} groups were synced", groupsSynced );
                }

                resultMessage += string.Format( " and {0} groups were changed", groupsChanged );

                if ( errors.Any() )
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine();
                    sb.Append( "Errors: " );
                    errors.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                    string errorMessage = sb.ToString();
                    resultMessage += errorMessage;
                    throw new Exception( errorMessage );
                }

                this.Result = resultMessage;
            }
            catch ( System.Exception ex )
            {
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( ex, context2 );
                throw;
            }
        }

        /// <summary>
        /// A POCO to store the SyncId and the GroupName of a <seealso cref="GroupSync"/>
        /// </summary>
        public class GroupSyncInfo
        {
            /// <summary>
            /// The Sync Id of the <seealso cref="GroupSync"/>
            /// </summary>
            public int SyncId { get; set; }

            /// <summary>
            /// The Name of the Group which the <seealso cref="GroupSync"/> is Associated with 
            /// </summary>
            public string GroupName { get; set; }
        }
    }
}
