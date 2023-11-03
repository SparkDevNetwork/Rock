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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;

using Humanizer;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

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
            // Get the job setting(s).
            bool requirePasswordReset = GetAttributeValue( "RequirePasswordReset" ).AsBoolean();
            var commandTimeout = GetAttributeValue( "CommandTimeout" ).AsIntegerOrNull() ?? 180;

            // Get groups to sync.
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

            var result = GroupSyncService.SyncGroups( activeSyncList, commandTimeout, requirePasswordReset, UpdateLastStatusMessage );

            // Format the result message.
            var groupsSyncedCount = result.GroupIdsSynced.Count();
            var groupsChangedCount = result.GroupIdsChanged.Count();

            var resultSb = new StringBuilder();
            var circleSuccess = "<i class='fa fa-circle text-success'></i>";
            resultSb.AppendLine( $"{circleSuccess} {groupsSyncedCount} {"group".PluralizeIf( groupsSyncedCount != 1 ).Titleize()} Synced{( groupsSyncedCount > 0 ? $" ({result.GroupIdsSynced.AsDelimited( ", " )})" : string.Empty )}" );
            resultSb.AppendLine( $"{circleSuccess} {groupsChangedCount} {"group".PluralizeIf( groupsChangedCount != 1 ).Titleize()} Changed{( groupsChangedCount > 0 ? $" ({result.GroupIdsChanged.AsDelimited( ", " )})" : string.Empty )}" );

            if ( groupsChangedCount > 0 )
            {
                resultSb.AppendLine( $"{circleSuccess} {result.DeletedMemberCount} {"person".PluralizeIf( result.DeletedMemberCount != 1 ).Titleize()} Removed" );
                resultSb.AppendLine( $"{circleSuccess} {result.AddedMemberCount} {"person".PluralizeIf( result.AddedMemberCount != 1 ).Titleize()} Added" );
            }

            var circleWarning = "<i class='fa fa-circle text-warning'></i>";
            if ( result.NotAddedMemberCount > 0 )
            {
                resultSb.AppendLine( $"{circleWarning} {result.NotAddedMemberCount} {"person".PluralizeIf( result.NotAddedMemberCount != 1 ).Titleize()} Could Not be Added" );
            }

            this.Result = resultSb.ToString();

            if ( result.WarningMessages.Any() || result.WarningExceptions.Any() )
            {
                resultSb.AppendLine();

                var warningExceptionMessage = "GroupSync completed with warnings.";
                AggregateException exceptionList = null;

                if ( result.WarningMessages.Any() )
                {
                    var enableLoggingMessage = "Enable 'Warning' logging level for 'Jobs' domain in Rock Logs and re-run this job to get a full list of issues.";
                    warningExceptionMessage = $"{warningExceptionMessage} {enableLoggingMessage}";

                    Log( Logging.RockLogLevel.Warning, $"{result.WarningMessages.Count} {"warning".PluralizeIf( result.WarningMessages.Count > 1 ).Titleize()}: {result.WarningMessages.AsDelimited( " | " )}" );

                    resultSb.AppendLine( $"{circleWarning} {enableLoggingMessage}" );
                }

                if ( result.WarningExceptions.Any() )
                {
                    exceptionList = new AggregateException( "One or more exceptions occurred in GroupSync.", result.WarningExceptions );

                    resultSb.AppendLine( $"{circleWarning} One or more exceptions occurred. See exception log for details." );
                }

                this.Result = resultSb.ToString();

                throw new RockJobWarningException( warningExceptionMessage, exceptionList );
            }
        }
    }
}
