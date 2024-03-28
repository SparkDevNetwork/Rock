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
using System.Linq;
using System.Threading.Tasks;

using Rock.Data;
using Rock.Tasks;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class GroupMember
    {
        /// <summary>
        /// Save hook implementation for <see cref="GroupMember"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<GroupMember>
        {
            private List<HistoryItem> HistoryChanges { get; set; }

            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                var rockContext = ( RockContext ) this.RockContext;
                if ( this.State != EntityContextState.Deleted
                     && Entity.IsArchived == false
                     && Entity.GroupMemberStatus != GroupMemberStatus.Inactive )
                {
                    // Bypass Group Member requirement check when group member is unarchived; instead, we'll show "does not meet" symbol in group member list.
                    var previousIsArchived = this.State == EntityContextState.Modified && OriginalValues[nameof( GroupMember.IsArchived )].ToStringSafe().AsBoolean();
                    if ( !previousIsArchived )
                    {
                        if ( !Entity.IsValidGroupMember( rockContext ) )
                        {
                            var message = Entity.ValidationResults != null
                                ? Entity.ValidationResults.AsDelimited( "; " )
                                : string.Empty;

                            throw new GroupMemberValidationException( message );
                        }
                    }
                }

                int? oldPersonId = null;
                int? newPersonId = null;

                int? oldGroupId = null;
                int? newGroupId = null;

                switch ( State )
                {
                    case EntityContextState.Added:
                        {
                            oldPersonId = null;
                            newPersonId = Entity.PersonId;

                            oldGroupId = null;
                            newGroupId = Entity.GroupId;

                            if ( !Entity.DateTimeAdded.HasValue )
                            {
                                Entity.DateTimeAdded = RockDateTime.Now;
                            }

                            // if this is a new record, but is saved with IsActive=False, set the InactiveDateTime if it isn't set already
                            if ( Entity.GroupMemberStatus == GroupMemberStatus.Inactive )
                            {
                                Entity.InactiveDateTime = Entity.InactiveDateTime ?? RockDateTime.Now;
                            }

                            break;
                        }

                    case EntityContextState.Modified:
                        {
                            oldPersonId = OriginalValues[nameof( GroupMember.PersonId )].ToStringSafe().AsIntegerOrNull();
                            newPersonId = Entity.PersonId;

                            oldGroupId = OriginalValues[nameof( GroupMember.GroupId )].ToStringSafe().AsIntegerOrNull();
                            newGroupId = Entity.GroupId;

                            var originalStatus = OriginalValues[nameof( GroupMember.GroupMemberStatus )].ToStringSafe().ConvertToEnum<GroupMemberStatus>();

                            // IsActive was modified, set the InactiveDateTime if it changed to Inactive, or set it to NULL if it changed to Active
                            if ( originalStatus != Entity.GroupMemberStatus )
                            {
                                if ( Entity.GroupMemberStatus == GroupMemberStatus.Inactive )
                                {
                                    // if the caller didn't already set InactiveDateTime, set it to now
                                    Entity.InactiveDateTime = Entity.InactiveDateTime ?? RockDateTime.Now;
                                }
                                else
                                {
                                    Entity.InactiveDateTime = null;
                                }
                            }

                            break;
                        }

                    case EntityContextState.Deleted:
                        {
                            oldPersonId = Entity.PersonId;
                            newPersonId = null;

                            oldGroupId = Entity.GroupId;
                            newGroupId = null;

                            break;
                        }
                }

                Group group = Entity.Group;
                if ( group == null )
                {
                    group = new GroupService( rockContext ).Get( Entity.GroupId );
                }

                if ( group != null )
                {
                    this.Entity.GroupTypeId = group.GroupTypeId;
                    string oldGroupName = group.Name;
                    if ( oldGroupId.HasValue && oldGroupId.Value != group.Id )
                    {
                        var oldGroup = new GroupService( rockContext ).Get( oldGroupId.Value );
                        if ( oldGroup != null )
                        {
                            oldGroupName = oldGroup.Name;
                        }
                    }

                    HistoryChanges = new List<HistoryItem>();
                    if ( newPersonId.HasValue )
                    {
                        HistoryChanges.Add( new HistoryItem()
                        {
                            PersonId = newPersonId.Value,
                            Caption = group.Name,
                            GroupId = group.Id,
                            Group = group
                        } );
                    }

                    if ( oldPersonId.HasValue )
                    {
                        HistoryChanges.Add( new HistoryItem()
                        {
                            PersonId = oldPersonId.Value,
                            Caption = oldGroupName,
                            GroupId = oldGroupId
                        } );
                    }

                    if ( newPersonId.HasValue && newGroupId.HasValue &&
                        ( !oldPersonId.HasValue || oldPersonId.Value != newPersonId.Value || !oldGroupId.HasValue || oldGroupId.Value != newGroupId.Value ) )
                    {
                        // New Person in group
                        var historyItem = HistoryChanges.First( h => h.PersonId == newPersonId.Value && h.GroupId == newGroupId.Value );

                        historyItem.PersonHistoryChangeList.AddChange( History.HistoryVerb.AddedToGroup, History.HistoryChangeType.Record, $"'{group.Name}' Group" );
                        History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Role", ( int? ) null, Entity.GroupRole, Entity.GroupRoleId, rockContext );
                        History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Note", string.Empty, Entity.Note );
                        History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Status", null, Entity.GroupMemberStatus );
                        History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Communication Preference", null, Entity.CommunicationPreference );
                        History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Guest Count", ( int? ) null, Entity.GuestCount );

                        var addedMemberPerson = Entity.Person ?? new PersonService( rockContext ).Get( Entity.PersonId );

                        // add the Person's Name as ValueName and Caption (just in case the groupmember record is deleted later)
                        historyItem.GroupMemberHistoryChangeList.AddChange( History.HistoryVerb.AddedToGroup, History.HistoryChangeType.Record, $"{addedMemberPerson?.FullName}" ).SetCaption( $"{addedMemberPerson?.FullName}" );
                        History.EvaluateChange( historyItem.GroupMemberHistoryChangeList, $"Role", ( int? ) null, Entity.GroupRole, Entity.GroupRoleId, rockContext );
                        History.EvaluateChange( historyItem.GroupMemberHistoryChangeList, $"Note", string.Empty, Entity.Note );
                        History.EvaluateChange( historyItem.GroupMemberHistoryChangeList, $"Status", null, Entity.GroupMemberStatus );
                        History.EvaluateChange( historyItem.GroupMemberHistoryChangeList, $"Communication Preference", null, Entity.CommunicationPreference );
                        History.EvaluateChange( historyItem.GroupMemberHistoryChangeList, $"Guest Count", ( int? ) null, Entity.GuestCount );
                    }

                    if ( newPersonId.HasValue && oldPersonId.HasValue && oldPersonId.Value == newPersonId.Value &&
                         newGroupId.HasValue && oldGroupId.HasValue && oldGroupId.Value == newGroupId.Value )
                    {
                        // Updated same person in group
                        var historyItem = HistoryChanges.First( h => h.PersonId == newPersonId.Value && h.GroupId == newGroupId.Value );
                        History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Role", OriginalValues[nameof( Entity.GroupRoleId )].ToStringSafe().AsIntegerOrNull(), Entity.GroupRole, Entity.GroupRoleId, rockContext );
                        History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Note", OriginalValues[nameof( Entity.Note )].ToStringSafe(), Entity.Note );
                        History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Status", OriginalValues[nameof( Entity.GroupMemberStatus )].ToStringSafe().ConvertToEnum<GroupMemberStatus>(), Entity.GroupMemberStatus );
                        History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Communication Preference", OriginalValues[nameof( Entity.CommunicationPreference )].ToStringSafe().ConvertToEnum<CommunicationType>(), Entity.CommunicationPreference );
                        History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Guest Count", OriginalValues[nameof( Entity.GuestCount )].ToStringSafe().AsIntegerOrNull(), Entity.GuestCount );
                        History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Archived", OriginalValues[nameof( Entity.IsArchived )].ToStringSafe().AsBoolean(), Entity.IsArchived );

                        // If the groupmember was Archived, make sure it is the first GroupMember History change (since they get summarized when doing a HistoryLog and Timeline
                        bool origIsArchived = OriginalValues[nameof( GroupMember.IsArchived )].ToStringSafe().AsBoolean();

                        if ( origIsArchived != Entity.IsArchived )
                        {
                            var memberPerson = Entity.Person ?? new PersonService( rockContext ).Get( Entity.PersonId );
                            if ( Entity.IsArchived == true )
                            {
                                // GroupMember changed to Archived
                                historyItem.GroupMemberHistoryChangeList.AddChange( History.HistoryVerb.RemovedFromGroup, History.HistoryChangeType.Record, $"{memberPerson?.FullName}" ).SetCaption( $"{memberPerson?.FullName}" );
                            }
                            else
                            {
                                // GroupMember changed to Not archived
                                historyItem.GroupMemberHistoryChangeList.AddChange( History.HistoryVerb.AddedToGroup, History.HistoryChangeType.Record, $"{memberPerson?.FullName}" ).SetCaption( $"{memberPerson?.FullName}" );
                            }
                        }

                        History.EvaluateChange( historyItem.GroupMemberHistoryChangeList, $"Role", OriginalValues[nameof( GroupMember.GroupRoleId )].ToStringSafe().AsIntegerOrNull(), Entity.GroupRole, Entity.GroupRoleId, rockContext );
                        History.EvaluateChange( historyItem.GroupMemberHistoryChangeList, $"Note", OriginalValues[nameof( GroupMember.Note )].ToStringSafe(), Entity.Note );
                        History.EvaluateChange( historyItem.GroupMemberHistoryChangeList, $"Status", OriginalValues[nameof( GroupMember.GroupMemberStatus )].ToStringSafe().ConvertToEnum<GroupMemberStatus>(), Entity.GroupMemberStatus );
                        History.EvaluateChange( historyItem.GroupMemberHistoryChangeList, $"Communication Preference", OriginalValues[nameof( GroupMember.CommunicationPreference )].ToStringSafe().ConvertToEnum<CommunicationType>(), Entity.CommunicationPreference );
                        History.EvaluateChange( historyItem.GroupMemberHistoryChangeList, $"Guest Count", OriginalValues[nameof( GroupMember.GuestCount )].ToStringSafe().AsIntegerOrNull(), Entity.GuestCount );
                    }

                    if ( oldPersonId.HasValue && oldGroupId.HasValue &&
                        ( !newPersonId.HasValue || newPersonId.Value != oldPersonId.Value || !newGroupId.HasValue || newGroupId.Value != oldGroupId.Value ) )
                    {
                        // Removed a person/groupmember in group
                        var historyItem = HistoryChanges.First( h => h.PersonId == oldPersonId.Value && h.GroupId == oldGroupId.Value );

                        historyItem.PersonHistoryChangeList.AddChange( History.HistoryVerb.RemovedFromGroup, History.HistoryChangeType.Record, $"{oldGroupName} Group" );

                        var deletedMemberPerson = Entity.Person ?? new PersonService( rockContext ).Get( Entity.PersonId );

                        historyItem.GroupMemberHistoryChangeList.AddChange( History.HistoryVerb.RemovedFromGroup, History.HistoryChangeType.Record, $"{deletedMemberPerson?.FullName}" ).SetCaption( $"{deletedMemberPerson?.FullName}" );
                    }
                }

                _preSaveChangesOldGroupId = oldGroupId;

                base.PreSave();
            }

            private int? _preSaveChangesOldGroupId = null;

            /// <summary>
            /// Called after the save operation has been executed
            /// </summary>
            /// <remarks>
            /// This method is only called if <see cref="M:Rock.Data.EntitySaveHook`1.PreSave" /> returns
            /// without error.
            /// </remarks>
            protected override void PostSave()
            {
                var rockContext = ( RockContext ) this.RockContext;
                if ( HistoryChanges != null )
                {
                    foreach ( var historyItem in HistoryChanges )
                    {
                        int personId = historyItem.PersonId > 0 ? historyItem.PersonId : Entity.PersonId;

                        // if GroupId is 0, it is probably a Group that wasn't saved yet, so get the GroupId from historyItem.Group.Id instead
                        if ( historyItem.GroupId == 0 )
                        {
                            historyItem.GroupId = historyItem.Group?.Id;
                        }

                        var changes = HistoryService.GetChanges(
                            typeof( Person ),
                            Rock.SystemGuid.Category.HISTORY_PERSON_GROUP_MEMBERSHIP.AsGuid(),
                            personId,
                            historyItem.PersonHistoryChangeList,
                            historyItem.Caption,
                            typeof( Group ),
                            historyItem.GroupId,
                            Entity.ModifiedByPersonAliasId,
                            rockContext.SourceOfChange );

                        if ( changes.Any() )
                        {
                            Task.Run( () =>
                            {
                                try
                                {
                                    using ( var insertRockContext = new RockContext() )
                                    {
                                        insertRockContext.BulkInsert( changes );
                                    }
                                }
                                catch ( SystemException ex )
                                {
                                    ExceptionLogService.LogException( ex, null );
                                }
                            } );
                        }

                        var groupMemberChanges = HistoryService.GetChanges(
                            typeof( GroupMember ),
                            Rock.SystemGuid.Category.HISTORY_GROUP_CHANGES.AsGuid(),
                            Entity.Id,
                            historyItem.GroupMemberHistoryChangeList,
                            historyItem.Caption,
                            typeof( Group ),
                            historyItem.GroupId,
                            Entity.ModifiedByPersonAliasId,
                            rockContext.SourceOfChange );

                        if ( groupMemberChanges.Any() )
                        {
                            Task.Run( () =>
                            {
                                try
                                {
                                    using ( var insertRockContext = new RockContext() )
                                    {
                                        insertRockContext.BulkInsert( groupMemberChanges );
                                    }
                                }
                                catch ( SystemException ex )
                                {
                                    ExceptionLogService.LogException( ex, null );
                                }
                            } );
                        }
                    }
                }

                base.PostSave();

                // if this is a GroupMember record on a Family, ensure that AgeClassification, PrimaryFamily,
                // GivingLeadId, and GroupSalution is updated
                // NOTE: This is also done on Person.PostSaveChanges in case Birthdate changes
                var groupTypeFamilyRoleIds = GroupTypeCache.GetFamilyGroupType()?.Roles?.Select( a => a.Id ).ToList();
                if ( groupTypeFamilyRoleIds?.Any() == true )
                {
                    if ( groupTypeFamilyRoleIds.Contains( Entity.GroupRoleId ) )
                    {
                        PersonService.UpdatePersonAgeClassification( Entity.PersonId, rockContext );
                        PersonService.UpdatePrimaryFamily( Entity.PersonId, rockContext );
                        PersonService.UpdateGivingLeaderId( Entity.PersonId, rockContext );

                        GroupService.UpdateGroupSalutations( Entity.GroupId, rockContext );

                        if ( _preSaveChangesOldGroupId.HasValue && _preSaveChangesOldGroupId.Value != Entity.GroupId )
                        {
                            // if person was moved to a different family, the old family will need its GroupSalutations updated
                            GroupService.UpdateGroupSalutations( _preSaveChangesOldGroupId.Value, rockContext );
                        }
                    }
                }

                if ( State == EntityContextState.Added || State == EntityContextState.Modified )
                {
                    if ( Entity.Group != null && Entity.Person != null )
                    {
                        if ( Entity.Group?.IsSecurityRoleOrSecurityGroupType() == true )
                        {
                            /* 09/27/2021 MDP

                            If this GroupMember record results in making this Person having a higher AccountProtectionProfile level,
                            update the Person's AccountProtectionProfile.
                            Note: If this GroupMember record could result in making this Person having a *lower* AccountProtectionProfile level,
                            don't lower the AccountProtectionProfile here, because other rules have to be considered before
                            lowering the AccountProtectionProfile level. So we'll let the RockCleanup job take care of making sure the
                            AccountProtectionProfile is updated after factoring in all the rules.
                            
                             */

                            if ( Entity.Group.ElevatedSecurityLevel >= Utility.Enums.ElevatedSecurityLevel.Extreme
                                && Entity.Person.AccountProtectionProfile < Utility.Enums.AccountProtectionProfile.Extreme )
                            {
                                Entity.Person.AccountProtectionProfile = Utility.Enums.AccountProtectionProfile.Extreme;
                                rockContext.SaveChanges();
                            }
                            else if ( Entity.Group.ElevatedSecurityLevel >= Utility.Enums.ElevatedSecurityLevel.High
                                && Entity.Person.AccountProtectionProfile < Utility.Enums.AccountProtectionProfile.High )
                            {
                                Entity.Person.AccountProtectionProfile = Utility.Enums.AccountProtectionProfile.High;
                                rockContext.SaveChanges();
                            }
                        }
                    }
                }

                // process universal search indexing if required
                var groupType = GroupTypeCache.Get( this.Entity.GroupTypeId );
                if ( groupType != null && groupType.IsIndexEnabled )
                {
                    var group = this.Entity.Group;
                    if ( group == null )
                    {
                        group = new GroupService( this.RockContext ).Get( this.Entity.GroupId );
                    }
                    if ( group?.IsActive ?? false )
                    {
                        var GroupEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.GROUP );
                        var groupIndexTransaction = new IndexEntityTransaction( new EntityIndexInfo() { EntityTypeId = GroupEntityTypeId.Value, EntityId = this.Entity.GroupId } );
                        groupIndexTransaction.Enqueue();
                    }
                }

                SendUpdateGroupMemberMessage();
            }

            /// <summary>
            /// Sends the update group member message.
            /// Don't do this in pre-save as it can cause a Race Condition with the message bus and the DB save.
            /// </summary>
            private void SendUpdateGroupMemberMessage()
            {
                var updateGroupMemberMsg = new UpdateGroupMember.Message
                {
                    State = State,
                    GroupId = Entity.GroupId,
                    PersonId = Entity.PersonId,
                    GroupMemberStatus = Entity.GroupMemberStatus,
                    GroupMemberRoleId = Entity.GroupRoleId,
                    IsArchived = Entity.IsArchived
                };

                if ( Entity.Group != null )
                {
                    updateGroupMemberMsg.GroupTypeId = Entity.Group.GroupTypeId;
                }

                // If this isn't a new group member, get the previous status and role values
                if ( State == EntityContextState.Modified )
                {
                    updateGroupMemberMsg.PreviousGroupMemberStatus = OriginalValues[nameof( GroupMember.GroupMemberStatus )].ToStringSafe().ConvertToEnum<GroupMemberStatus>();
                    updateGroupMemberMsg.PreviousGroupMemberRoleId = OriginalValues[nameof( GroupMember.GroupRoleId )].ToStringSafe().AsInteger();
                    updateGroupMemberMsg.PreviousIsArchived = OriginalValues[nameof( GroupMember.IsArchived )].ToStringSafe().AsBoolean();
                }

                // If this isn't a deleted group member, get the group member guid
                if ( State != EntityContextState.Deleted )
                {
                    updateGroupMemberMsg.GroupMemberGuid = Entity.Guid;
                }

                updateGroupMemberMsg.Send();
            }
        }
    }
}
