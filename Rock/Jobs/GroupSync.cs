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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

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
    ///            1          1           0   change IsArchived to false
    ///
    /// NOTE: It should do this regardless of the person's IsDeceased flag.
    /// NOTE: The job can sync new people at about 45/sec or 2650/minute.
    /// </summary>
    [DisallowConcurrentExecution]
    [BooleanField( "Require Password Reset On New Logins", "Determines if new logins should be created in such a way that the individual will need to reset the password on their first login.", Key = "RequirePasswordReset" )]
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for each operation to complete. Leave blank to use the default for this job (180).", false, 3 * 60, "General", 1, "CommandTimeout" )]
    public class GroupSync : IJob
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

        /// <summary>
        /// Job that will sync groups.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            // Get the job setting(s)
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            bool requirePasswordReset = dataMap.GetBoolean( "RequirePasswordReset" );
            var commandTimeout = dataMap.GetString( "CommandTimeout" ).AsIntegerOrNull() ?? 180;

            // Counters for displaying results
            int groupsSynced = 0;
            int groupsChanged = 0;
            string groupName = string.Empty;
            string dataViewName = string.Empty;
            var errors = new List<string>();

            try
            {
                // get groups set to sync
                var activeSyncIds = new List<int>();
                using ( var rockContext = new RockContext() )
                {
                    // Get groups that are not archived and are still active.
                    activeSyncIds = new GroupSyncService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( x => !x.Group.IsArchived && x.Group.IsActive )
                        .Select( x => x.Id )
                        .ToList();
                }

                foreach ( var syncId in activeSyncIds )
                {
                    bool hasSyncChanged = false;

                    // Use a fresh rockContext per sync so that ChangeTracker doesn't get bogged down
                    using ( var rockContext = new RockContext() )
                    {
                        // increase the timeout just in case the data view source is slow
                        rockContext.Database.CommandTimeout = commandTimeout;
                        rockContext.SourceOfChange = "Group Sync";

                        // Get the Sync
                        var sync = new GroupSyncService( rockContext )
                            .Queryable().AsNoTracking()
                            .FirstOrDefault( s => s.Id == syncId );

                        // Ensure that the group's Sync Data View is a person data view
                        if ( sync != null && sync.SyncDataView.EntityTypeId == EntityTypeCache.Get( typeof( Person ) ).Id )
                        {
                            List<string> syncErrors = new List<string>();

                            dataViewName = sync.SyncDataView.Name;
                            groupName = sync.Group.Name;

                            // Get the person id's from the data view (source)
                            var personService = new PersonService( rockContext );
                            var sourcePersonIds = sync.SyncDataView.GetQuery( null, rockContext, commandTimeout, out syncErrors )
                                .Select( q => q.Id ).ToList();

                            // If any error occurred trying get the 'where expression' from the sync-data-view,
                            // just skip trying to sync that particular group's Sync Data View for now.
                            if ( syncErrors.Count > 0 )
                            {
                                errors.AddRange( syncErrors );
                                ExceptionLogService.LogException( new Exception( string.Format( "An error occurred while trying to GroupSync group '{0}' and data view '{1}' so the sync was skipped. Error: {2}", groupName, dataViewName, String.Join( ",", syncErrors ) ) ) );
                                continue;
                            }

                            // Get the person id's in the group (target) for the role being synced.
                            // Note: targetPersonIds must include archived group members
                            // so we don't try to delete anyone who's already archived, and
                            // it must include deceased members so we can remove them if they
                            // are no longer in the data view.
                            var targetPersonIds = new GroupMemberService( rockContext )
                                .Queryable( true, true ).AsNoTracking()
                                .Where( gm => gm.GroupId == sync.GroupId )
                                .Where( gm => gm.GroupRoleId == sync.GroupTypeRoleId )
                                .Select( gm => new {
                                    PersonId = gm.PersonId,
                                    IsArchived = gm.IsArchived
                                } )
                                .ToList();

                            // Delete people from the group/role that are no longer in the data view --
                            // but not the ones that are already archived.
                            foreach ( var targetPerson in targetPersonIds.Where( t => !sourcePersonIds.Contains( t.PersonId ) && t.IsArchived != true ) )
                            {
                                try
                                {
                                    // Use a new context to limit the amount of change-tracking required
                                    using ( var groupMemberContext = new RockContext() )
                                    {
                                        // Delete the records for that person's group and role
                                        var groupMemberService = new GroupMemberService( groupMemberContext );
                                        foreach ( var groupMember in groupMemberService
                                            .Queryable( true, true )
                                            .Where( m =>
                                                m.GroupId == sync.GroupId &&
                                                m.GroupRoleId == sync.GroupTypeRoleId &&
                                                m.PersonId == targetPerson.PersonId )
                                            .ToList() )
                                        {
                                            groupMemberService.Delete( groupMember );
                                        }

                                        groupMemberContext.SaveChanges();

                                        // If the Group has an exit email, and person has an email address, send them the exit email
                                        if ( sync.ExitSystemEmail != null )
                                        {
                                            var person = new PersonService( groupMemberContext ).Get( targetPerson.PersonId );
                                            if ( person.Email.IsNotNullOrWhiteSpace() )
                                            {
                                                // Send the exit email
                                                var mergeFields = new Dictionary<string, object>();
                                                mergeFields.Add( "Group", sync.Group );
                                                mergeFields.Add( "Person", person );
                                                var emailMessage = new RockEmailMessage( sync.ExitSystemEmail );
                                                emailMessage.AddRecipient( new RecipientData( person.Email, mergeFields ) );
                                                var emailErrors = new List<string>();
                                                emailMessage.Send( out emailErrors );
                                                errors.AddRange( emailErrors);
                                            }
                                        }
                                    }
                                }
                                catch ( Exception ex )
                                {
                                    ExceptionLogService.LogException( ex );
                                    continue;
                                }
                                
                                hasSyncChanged = true;
                            }

                            // Now find all the people in the source list who are NOT already in the target list
                            // or in the target list as archived (so we can restore them).
                            foreach ( var personId in sourcePersonIds.Where( s => ! targetPersonIds.Any( t => t.PersonId == s )
                            || targetPersonIds.Any( t => t.PersonId == s && t.IsArchived ) ) )
                            {
                                try
                                {
                                    // Use a new context to limit the amount of change-tracking required
                                    using ( var groupMemberContext = new RockContext() )
                                    {
                                        var groupMemberService = new GroupMemberService( groupMemberContext );

                                        // If this person is currently archived...
                                        if ( targetPersonIds.Any( t => t.PersonId == personId && t.IsArchived == true ) )
                                        {
                                            // ...then we'll just restore them;

                                            // NOTE: AddOrRestoreGroupMember will find the exact group member record and
                                            // sets their IsArchived back to false. 
                                            var newGroupMember = groupMemberService.AddOrRestoreGroupMember( sync.Group, personId, sync.GroupTypeRoleId );
                                            newGroupMember.GroupMemberStatus = GroupMemberStatus.Active;
                                            groupMemberContext.SaveChanges();
                                        }
                                        else
                                        {
                                            // ...otherwise we will add a new person to the group with the role specified in the sync.
                                            var newGroupMember = new GroupMember { Id = 0 };
                                            newGroupMember.PersonId = personId;
                                            newGroupMember.GroupId = sync.GroupId;
                                            newGroupMember.GroupMemberStatus = GroupMemberStatus.Active;
                                            newGroupMember.GroupRoleId = sync.GroupTypeRoleId;

                                            if ( newGroupMember.IsValidGroupMember( groupMemberContext ) )
                                            {
                                                groupMemberService.Add( newGroupMember );
                                                groupMemberContext.SaveChanges();
                                            }
                                            else
                                            {
                                                // Validation errors will get added to the ValidationResults collection. Add those results to the log and then move on to the next person.
                                                var ex = new GroupMemberValidationException( string.Join( ",", newGroupMember.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );
                                                ExceptionLogService.LogException( ex );
                                                continue;
                                            }
                                        }

                                        // If the Group has a welcome email, and person has an email address, send them the welcome email and possibly create a login
                                        if ( sync.WelcomeSystemEmail != null )
                                        {
                                            var person = new PersonService( groupMemberContext ).Get( personId );
                                            if ( person.Email.IsNotNullOrWhiteSpace() )
                                            {
                                                // If the group is configured to add a user account for anyone added to the group, and person does not yet have an
                                                // account, add one for them.
                                                string newPassword = string.Empty;
                                                bool createLogin = sync.AddUserAccountsDuringSync;

                                                // Only create a login if requested, no logins exist and we have enough information to generate a user name.
                                                if ( createLogin && !person.Users.Any() && !string.IsNullOrWhiteSpace( person.NickName ) && !string.IsNullOrWhiteSpace( person.LastName ) )
                                                {
                                                    newPassword = System.Web.Security.Membership.GeneratePassword( 9, 1 );
                                                    string username = Rock.Security.Authentication.Database.GenerateUsername( person.NickName, person.LastName );

                                                    UserLogin login = UserLoginService.Create(
                                                        groupMemberContext,
                                                        person,
                                                        AuthenticationServiceType.Internal,
                                                        EntityTypeCache.Get( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                                                        username,
                                                        newPassword,
                                                        true,
                                                        requirePasswordReset );
                                                }

                                                // Send the welcome email
                                                var mergeFields = new Dictionary<string, object>();
                                                mergeFields.Add( "Group", sync.Group );
                                                mergeFields.Add( "Person", person );
                                                mergeFields.Add( "NewPassword", newPassword );
                                                mergeFields.Add( "CreateLogin", createLogin );
                                                var emailMessage = new RockEmailMessage( sync.WelcomeSystemEmail );
                                                emailMessage.AddRecipient( new RecipientData( person.Email, mergeFields ) );
                                                var emailErrors = new List<string>();
                                                emailMessage.Send( out emailErrors );
                                                errors.AddRange( emailErrors );
                                            }
                                        }
                                    }
                                }
                                catch ( Exception ex )
                                {
                                    ExceptionLogService.LogException( ex );
                                    continue;
                                }

                                hasSyncChanged = true;
                            }

                            // Increment Groups Changed Counter (if people were deleted or added to the group)
                            if ( hasSyncChanged )
                            {
                                groupsChanged++;
                            }

                            // Increment the Groups Synced Counter
                            groupsSynced++;
                        }
                    }
                }

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

                context.Result = resultMessage;
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
