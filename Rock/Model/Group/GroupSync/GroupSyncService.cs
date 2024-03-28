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
using System.Diagnostics;
using System.Linq;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class GroupSyncService
    {
        /// <summary>
        /// Syncs the specified Group with all its data views.
        /// </summary>
        /// <param name="groupId">The ID of the group to sync.</param>
        /// <returns>An object representing the outcome of the group sync attempt.</returns>
        [RockInternal( "1.16.0" )]
        public GroupSyncResult SyncGroup( int groupId )
        {
            var groupSyncsForGroup = Queryable()
                .AsNoTracking()
                .Where( s => s.Group.Id == groupId )
                .AreNotArchived()
                .AreActive()
                .Select( x => new GroupSyncInfo { SyncId = x.Id, GroupName = x.Group.Name } )
                .ToList();

            return SyncGroups( groupSyncsForGroup, 180, false );
        }

        /// <summary>
        /// Syncs the groups specified within the provided <see cref="GroupSyncInfo"/>s.
        /// </summary>
        /// <param name="activeSyncList">The list of group syncs to synchronize.</param>
        /// <param name="commandTimeout">The database timeout in seconds.</param>
        /// <param name="requirePasswordReset">Whether to require a password reset for new logins.</param>
        /// <param name="updateStatusAction">The method to invoke to update the caller of sync status/progress.</param>
        /// <returns>An object representing the outcome of the group sync attempt.</returns>
        [RockInternal( "1.16.0" )]
        public static GroupSyncResult SyncGroups( List<GroupSyncInfo> activeSyncList, int? commandTimeout, bool requirePasswordReset, Action<string> updateStatusAction = null )
        {
            var result = new GroupSyncResult();

            int groupId = default;
            var groupName = string.Empty;
            var dataViewName = string.Empty;

            foreach ( var syncInfo in activeSyncList )
            {
                var syncId = syncInfo.SyncId;
                var hasSyncChanged = false;
                updateStatusAction?.Invoke( $"Syncing group '{syncInfo.GroupName}'" );

                // Use a fresh rockContext per sync so that ChangeTracker doesn't get bogged down
                using ( var rockContext = new RockContext() )
                using ( var rockContextReadOnly = new RockContextReadOnly() )
                {
                    // Always use the non-readonly context to get the sync object graph, so we know whether the
                    // actual sync process should use the readonly context or not, based on the latest settings.
                    var sync = new GroupSyncService( rockContext )
                        .Queryable()
                        .Include( s => s.Group )
                        .Include( s => s.SyncDataView )
                        .AsNoTracking()
                        .FirstOrDefault( s => s.Id == syncId );

                    if ( sync == null || sync.SyncDataView.EntityTypeId != EntityTypeCache.Get( typeof( Person ) ).Id )
                    {
                        // invalid sync or invalid SyncDataView
                        continue;
                    }

                    var syncContext = sync.SyncDataView.DisableUseOfReadOnlyContext
                        ? rockContext
                        : rockContextReadOnly;

                    // increase the timeout just in case the data view source is slow
                    syncContext.Database.CommandTimeout = commandTimeout ?? 30;
                    syncContext.SourceOfChange = "Group Sync";

                    dataViewName = sync.SyncDataView.Name;
                    groupName = sync.Group.Name;
                    groupId = sync.Group.Id;

                    var stopwatch = Stopwatch.StartNew();

                    // Get the person id's from the data view (source)
                    var dataViewGetQueryArgs = new DataViewGetQueryArgs
                    {
                        /*

                            11/28/2022 - CWR
                            In order to prevent potential context conflicts with allowing a new Rock context being created here,
                            this DbContext will stay set to the rockContextReadOnly that was passed in.

                         */
                        DbContext = syncContext,
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
                        result.WarningExceptions.Add( new Exception( $"An error occurred while trying to GroupSync group '{groupName}' and data view '{dataViewName}' so the sync was skipped.", ex ) );
                        continue;
                    }

                    stopwatch.Stop();
                    DataViewService.AddRunDataViewTransaction( sync.SyncDataView.Id, Convert.ToInt32( stopwatch.Elapsed.TotalMilliseconds ) );

                    // Get the person id's in the group (target) for the role being synced.
                    // Note: targetPersonIds must include archived group members
                    // so we don't try to delete anyone who's already archived, and
                    // it must include deceased members so we can remove them if they
                    // are no longer in the data view.
                    var existingGroupMemberPersonList = new GroupMemberService( syncContext )
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
                        updateStatusAction?.Invoke( $"Deleting or archiving {targetPersonIdsToDelete.Count()} group member records in '{groupName}' that are no longer in the sync data view." );
                    }

                    // Delete people from the group/role that are no longer in the data view --
                    // but not the ones that are already archived.
                    foreach ( var targetPerson in targetPersonIdsToDelete )
                    {
                        if ( result.DeletedMemberCount % 100 == 0 )
                        {
                            updateStatusAction?.Invoke( $"Deleted or archived {result.DeletedMemberCount} of {targetPersonIdsToDelete.Count()} group member records for group '{groupName}'." );
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

                                result.DeletedMemberCount++;
                                hasSyncChanged = true;

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

                                        if ( emailErrors?.Any() == true )
                                        {
                                            emailErrors.ForEach( e => result.WarningMessages.Add( $"Unable to send exit email to '{groupName}' group member with person ID {targetPerson.PersonId}. Error: {e}" ) );
                                        }
                                    }
                                }
                            }
                        }
                        catch ( Exception ex )
                        {
                            result.WarningExceptions.Add( new Exception( $"Unable to delete person with ID {targetPerson.PersonId} from group '{groupName}'.", ex ) );
                            continue;
                        }
                    }

                    // Now find all the people in the source list who are NOT already in the target list (as Unarchived)
                    var targetPersonIdsToAdd = sourcePersonIds.Where( s => !existingGroupMemberPersonList.Any( t => t.PersonId == s && t.IsArchived == false ) ).ToList();

                    // Make a list of PersonIds that have an Archived group member record
                    // if this person isn't already a member of the list as an Unarchived member, we can Restore the group member for that PersonId instead
                    var archivedTargetPersonIds = existingGroupMemberPersonList.Where( t => t.IsArchived == true ).Select( a => a.PersonId ).ToList();

                    updateStatusAction?.Invoke( $"Adding {targetPersonIdsToAdd.Count()} group member records to group '{groupName}'." );

                    foreach ( var personId in targetPersonIdsToAdd )
                    {
                        if ( ( result.AddedMemberCount + result.NotAddedMemberCount ) % 100 == 0 )
                        {
                            string notAddedMessage = string.Empty;
                            if ( result.NotAddedMemberCount > 0 )
                            {
                                notAddedMessage = $"{Environment.NewLine}There are {result.NotAddedMemberCount} members that could not be added.";
                            }

                            updateStatusAction?.Invoke( $"Added {result.AddedMemberCount} of {targetPersonIdsToAdd.Count()} group member records to group '{groupName}'. {notAddedMessage}" );
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

                                    // GroupMember.PreSave() will NOT call GroupMember.IsValidGroupMember() in this scenario - since we're restoring
                                    // a previously-archived member - so we must manually validate here before attempting to save.
                                    if ( archivedGroupMember.IsValidGroupMember( groupMemberContext ) )
                                    {
                                        result.AddedMemberCount++;
                                        groupMemberService.Restore( archivedGroupMember );
                                        groupMemberContext.SaveChanges();
                                    }
                                    else
                                    {
                                        // Validation errors will have been added to the ValidationResults collection. Add these errors to the results and then move on to the next person.
                                        result.WarningMessages.Add( $"Unable to restore archived group member with person ID {personId} (group Member ID {archivedGroupMember.Id}) to group '{groupName}'. {archivedGroupMember.ValidationResults.AsDelimited( "; " )}" );
                                        result.NotAddedMemberCount++;
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

                                    groupMemberService.Add( newGroupMember );

                                    try
                                    {
                                        // GroupMember.PreSave() WILL call GroupMember.IsValidGroupMember() in this scenario, so there is no need
                                        // to manually validate here before attempting to save. In fact, doing so would be redundant and result in
                                        // duplicate db queries. If this group member fails the validation check, we'll catch the exception below
                                        // and add the validation results to the overall results object.
                                        groupMemberContext.SaveChanges();
                                        result.AddedMemberCount++;
                                    }
                                    catch ( GroupMemberValidationException )
                                    {
                                        // Validation errors will have been added to the ValidationResults collection.
                                        // Add those results to the overall results object and then move on to the next person.
                                        result.WarningMessages.Add( $"Unable to add group member with person ID {personId} to group '{groupName}'. {newGroupMember.ValidationResults.AsDelimited( "; " )}" );
                                        result.NotAddedMemberCount++;
                                        continue;
                                    }
                                }

                                hasSyncChanged = true;

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

                                        if ( emailErrors?.Any() == true )
                                        {
                                            emailErrors.ForEach( e => result.WarningMessages.Add( $"Unable to send welcome email to '{groupName}' group member with person ID {personId}. Error: {e}" ) );
                                            emailErrors.ForEach( e => result.WarningExceptions.Add( new Exception( $"Unable to send welcome email to '{groupName}' group member with person ID {personId}.", new Exception( e ) ) ) );
                                        }
                                    }
                                }
                            }
                        }
                        catch ( Exception ex )
                        {
                            result.WarningExceptions.Add( new Exception( $"Unable to add person with ID {personId} to group '{groupName}'.", ex ) );
                            continue;
                        }

                        hasSyncChanged = true;
                    }

                    if ( hasSyncChanged && !result.GroupIdsChanged.Contains( groupId ) )
                    {
                        result.GroupIdsChanged.Add( groupId );
                    }

                    if ( !result.GroupIdsSynced.Contains( groupId ) )
                    {
                        result.GroupIdsSynced.Add( groupId );
                    }
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

            return result;
        }
    }
}
