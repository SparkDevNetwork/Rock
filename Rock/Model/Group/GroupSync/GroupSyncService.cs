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
using Rock.Communication;
using Rock.Data;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using static Rock.Jobs.GroupSync;

namespace Rock.Model
{
    public partial class GroupSyncService
    {
        /// <summary>
        /// Sync the Group with all it's data view
        /// </summary>
        /// <param name="groupId"></param>
        public void SyncGroup( int groupId )
        {
            var groupSyncsForGroup = Queryable()
                .AsNoTracking()
                .Where( s => s.Group.Id == groupId )
                .AreNotArchived()
                .AreActive()
                .Select( x => new GroupSyncInfo { SyncId = x.Id, GroupName = x.Group.Name } )
                .ToList();

            GroupSyncService.SyncGroups( groupSyncsForGroup, 180, false, out _, out _, out _ );
        }

        /// <summary>
        /// Sync a list of <seealso cref="GroupSyncInfo"/>
        /// </summary>
        /// <param name="activeSyncList">The List of <seealso cref="GroupSyncInfo"/> to be executed</param>
        /// <param name="commandTimeout"></param>
        /// <param name="requirePasswordReset"></param>
        /// <param name="groupsChanged"></param>
        /// <param name="groupsSynced"></param>
        /// <param name="errors"></param>
        /// <param name="logMessageAction"></param>
        public static void SyncGroups( List<GroupSyncInfo> activeSyncList, int? commandTimeout, bool requirePasswordReset, out List<string> errors, out int groupsChanged, out int groupsSynced,
            Action<string> logMessageAction = null )
        {
            var groupName = string.Empty;
            var dataViewName = string.Empty;
            errors = new List<string>();
            groupsChanged = 0;
            groupsSynced = 0;
            foreach ( var syncInfo in activeSyncList )
            {
                var syncId = syncInfo.SyncId;
                var hasSyncChanged = false;
                logMessageAction?.Invoke( $"Syncing group {syncInfo.GroupName}" );

                // Use a fresh rockContext per sync so that ChangeTracker doesn't get bogged down
                using ( var rockContextReadOnly = new RockContextReadOnly() )
                {
                    // increase the timeout just in case the data view source is slow
                    rockContextReadOnly.Database.CommandTimeout = commandTimeout ?? 30;
                    rockContextReadOnly.SourceOfChange = "Group Sync";

                    // Get the Sync
                    var sync = new GroupSyncService( rockContextReadOnly )
                        .Queryable( "Group, SyncDataView" )
                        .AsNoTracking()
                        .FirstOrDefault( s => s.Id == syncId );

                    if ( sync == null || sync.SyncDataView.EntityTypeId != EntityTypeCache.Get( typeof( Person ) ).Id )
                    {
                        // invalid sync or invalid SyncDataView
                        continue;
                    }

                    dataViewName = sync.SyncDataView.Name;
                    groupName = sync.Group.Name;

                    var stopwatch = Stopwatch.StartNew();

                    // Get the person id's from the data view (source)
                    var dataViewGetQueryArgs = new DataViewGetQueryArgs
                    {
                        /*

                            11/28/2022 - CWR
                            In order to prevent potential context conflicts with allowing a new Rock context being created here,
                            this DbContext will stay set to the rockContextReadOnly that was passed in.

                         */
                        DbContext = rockContextReadOnly,
                        DatabaseTimeoutSeconds = commandTimeout
                    };

                    List<int> sourcePersonIds;

                    try
                    {
                        var dataViewQry = sync.SyncDataView.GetQuery( dataViewGetQueryArgs );
                        sourcePersonIds = dataViewQry.Select( q => q.Id ).ToList();
                    }
                    catch ( Exception ex )
                    {
                        // If any error occurred trying get the 'where expression' from the sync-data-view,
                        // just skip trying to sync that particular group's Sync Data View for now.
                        var errorMessage = $"An error occurred while trying to GroupSync group '{groupName}' and data view '{dataViewName}' so the sync was skipped. Error: {ex.Message}";
                        errors.Add( errorMessage );
                        ExceptionLogService.LogException( new Exception( errorMessage, ex ) );
                        continue;
                    }

                    stopwatch.Stop();
                    DataViewService.AddRunDataViewTransaction( sync.SyncDataView.Id, Convert.ToInt32( stopwatch.Elapsed.TotalMilliseconds ) );

                    // Get the person id's in the group (target) for the role being synced.
                    // Note: targetPersonIds must include archived group members
                    // so we don't try to delete anyone who's already archived, and
                    // it must include deceased members so we can remove them if they
                    // are no longer in the data view.
                    var existingGroupMemberPersonList = new GroupMemberService( rockContextReadOnly )
                        .Queryable( true, true ).AsNoTracking()
                        .Where( gm => gm.GroupId == sync.GroupId && gm.GroupRoleId == sync.GroupTypeRoleId )
                        .Select( gm => new
                        {
                            PersonId = gm.PersonId,
                            IsArchived = gm.IsArchived
                        } )
                        .ToList();

                    var targetPersonIdsToDelete = existingGroupMemberPersonList.Where( t => !sourcePersonIds.Contains( t.PersonId ) && t.IsArchived != true ).ToList();
                    if ( targetPersonIdsToDelete.Any() )
                    {
                        logMessageAction?.Invoke( $"Deleting {targetPersonIdsToDelete.Count()} group records in {syncInfo.GroupName} that are no longer in the sync data view" );
                    }

                    var deletedCount = 0;

                    // Delete people from the group/role that are no longer in the data view --
                    // but not the ones that are already archived.
                    foreach ( var targetPerson in targetPersonIdsToDelete )
                    {
                        deletedCount++;
                        if ( deletedCount % 100 == 0 )
                        {
                            logMessageAction?.Invoke( $"Deleted {deletedCount} of {targetPersonIdsToDelete.Count()} group member records for group {syncInfo.GroupName}" );
                        }

                        try
                        {
                            // Use a new context to limit the amount of change-tracking required
                            using ( var groupMemberContext = new RockContext() )
                            {
                                // Delete the records for that person's group and role.
                                // NOTE: just in case there are duplicate records, delete all group member records for that person and role
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
                                if ( sync.ExitSystemCommunication != null )
                                {
                                    var person = new PersonService( groupMemberContext ).Get( targetPerson.PersonId );
                                    if ( person.CanReceiveEmail( false ) )
                                    {
                                        // Send the exit email
                                        var mergeFields = new Dictionary<string, object>
                                        {
                                            { "Group", sync.Group },
                                            { "Person", person }
                                        };
                                        var emailMessage = new RockEmailMessage( sync.ExitSystemCommunication );
                                        emailMessage.AddRecipient( new RockEmailMessageRecipient( person, mergeFields ) );
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

                    // Now find all the people in the source list who are NOT already in the target list (as Unarchived)
                    var targetPersonIdsToAdd = sourcePersonIds.Where( s => !existingGroupMemberPersonList.Any( t => t.PersonId == s && t.IsArchived == false ) ).ToList();

                    // Make a list of PersonIds that have an Archived group member record
                    // if this person isn't already a member of the list as an Unarchived member, we can Restore the group member for that PersonId instead
                    var archivedTargetPersonIds = existingGroupMemberPersonList.Where( t => t.IsArchived == true ).Select( a => a.PersonId ).ToList();

                    logMessageAction?.Invoke( $"Adding {targetPersonIdsToAdd.Count()} group member records for group {syncInfo.GroupName}" );
                    var addedCount = 0;
                    var notAddedCount = 0;
                    foreach ( var personId in targetPersonIdsToAdd )
                    {
                        if ( ( addedCount + notAddedCount ) % 100 == 0 )
                        {
                            string notAddedMessage = string.Empty;
                            if ( notAddedCount > 0 )
                            {
                                notAddedMessage = $"{Environment.NewLine} There are {notAddedCount} members that could not be added due to group requirements.";
                            }

                            logMessageAction?.Invoke( $"Added {addedCount} of {targetPersonIdsToAdd.Count()} group member records for group {syncInfo.GroupName}. {notAddedMessage}" );
                        }

                        try
                        {
                            // Use a new context to limit the amount of change-tracking required
                            using ( var groupMemberContext = new RockContext() )
                            {
                                var groupMemberService = new GroupMemberService( groupMemberContext );
                                var groupService = new GroupService( groupMemberContext );

                                // If this person is currently archived...
                                if ( archivedTargetPersonIds.Contains( personId ) )
                                {
                                    // ...then we'll just restore them;
                                    GroupMember archivedGroupMember = groupService.GetArchivedGroupMember( sync.Group, personId, sync.GroupTypeRoleId );

                                    if ( archivedGroupMember == null )
                                    {
                                        // shouldn't happen, but just in case
                                        continue;
                                    }

                                    archivedGroupMember.GroupMemberStatus = GroupMemberStatus.Active;
                                    if ( archivedGroupMember.IsValidGroupMember( groupMemberContext ) )
                                    {
                                        addedCount++;
                                        groupMemberService.Restore( archivedGroupMember );
                                        groupMemberContext.SaveChanges();
                                    }
                                    else
                                    {
                                        notAddedCount++;

                                        // Validation errors will get added to the ValidationResults collection. Add those results to the log and then move on to the next person.
                                        var ex = new GroupMemberValidationException( "Archived group member: " + string.Join( ",", archivedGroupMember.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );
                                        ExceptionLogService.LogException( ex );
                                        continue;
                                    }
                                }
                                else
                                {
                                    // ...otherwise we will add a new person to the group with the role specified in the sync.
                                    var newGroupMember = new GroupMember
                                    {
                                        Id = 0,
                                        PersonId = personId,
                                        GroupId = sync.GroupId,
                                        GroupMemberStatus = GroupMemberStatus.Active,
                                        GroupRoleId = sync.GroupTypeRoleId
                                    };

                                    if ( newGroupMember.IsValidGroupMember( groupMemberContext ) )
                                    {
                                        addedCount++;
                                        groupMemberService.Add( newGroupMember );
                                        groupMemberContext.SaveChanges();
                                    }
                                    else
                                    {
                                        notAddedCount++;

                                        // Validation errors will get added to the ValidationResults collection. Add those results to the log and then move on to the next person.
                                        var ex = new GroupMemberValidationException( "New group member: " + string.Join( ",", newGroupMember.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );
                                        ExceptionLogService.LogException( ex );
                                        continue;
                                    }
                                }

                                // If the Group has a welcome email, and person has an email address, send them the welcome email and possibly create a login
                                if ( sync.WelcomeSystemCommunication != null )
                                {
                                    var person = new PersonService( groupMemberContext ).Get( personId );
                                    if ( person.CanReceiveEmail( false ) )
                                    {
                                        // If the group is configured to add a user account for anyone added to the group, and person does not yet have an
                                        // account, add one for them.
                                        var newPassword = string.Empty;
                                        var createLogin = sync.AddUserAccountsDuringSync;

                                        // Only create a login if requested, no logins exist and we have enough information to generate a user name.
                                        if ( createLogin && !person.Users.Any() && !string.IsNullOrWhiteSpace( person.NickName ) && !string.IsNullOrWhiteSpace( person.LastName ) )
                                        {
                                            newPassword = System.Web.Security.Membership.GeneratePassword( 9, 1 );
                                            var username = Rock.Security.Authentication.Database.GenerateUsername( person.NickName, person.LastName );

                                            var login = UserLoginService.Create(
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
                                        var mergeFields = new Dictionary<string, object>
                                        {
                                            { "Group", sync.Group },
                                            { "Person", person },
                                            { "NewPassword", newPassword },
                                            { "CreateLogin", createLogin }
                                        };
                                        var emailMessage = new RockEmailMessage( sync.WelcomeSystemCommunication );
                                        emailMessage.AddRecipient( new RockEmailMessageRecipient( person, mergeFields ) );
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

                // Update last refresh datetime in different context to avoid side-effects.
                using ( var rockContext = new RockContext() )
                {
                    var sync = new GroupSyncService( rockContext )
                        .Queryable()
                        .FirstOrDefault( s => s.Id == syncId );

                    sync.LastRefreshDateTime = RockDateTime.Now;

                    rockContext.SaveChanges();
                }
            }
        }
    }
}
