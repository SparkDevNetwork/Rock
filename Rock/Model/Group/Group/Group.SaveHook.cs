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
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Group
    {
        /// <summary>
        /// Save hook implementation for <see cref="Group"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<Group>
        {
            private History.HistoryChangeList HistoryChangeList { get; set; }
            private bool _FamilyCampusIsChanged = false;

            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                HistoryChangeList = new History.HistoryChangeList();

                var rockContext = ( RockContext ) this.RockContext;
                _FamilyCampusIsChanged = false;

                switch ( State )
                {
                    case EntityContextState.Added:
                        {
                            HistoryChangeList.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Group" ).SetNewValue( Entity.Name );

                            History.EvaluateChange( HistoryChangeList, "Name", string.Empty, Entity.Name );
                            History.EvaluateChange( HistoryChangeList, "Description", string.Empty, Entity.Description );
                            History.EvaluateChange( HistoryChangeList, "Group Type", ( int? ) null, Entity.GroupType, Entity.GroupTypeId );
                            History.EvaluateChange( HistoryChangeList, "Campus", ( int? ) null, Entity.Campus, Entity.CampusId );
                            History.EvaluateChange( HistoryChangeList, "Security Role", ( bool? ) null, Entity.IsSecurityRole );
                            History.EvaluateChange( HistoryChangeList, "Active", ( bool? ) null, Entity.IsActive );
                            History.EvaluateChange( HistoryChangeList, "Allow Guests", ( bool? ) null, Entity.AllowGuests );
                            History.EvaluateChange( HistoryChangeList, "Public", ( bool? ) null, Entity.IsPublic );
                            History.EvaluateChange( HistoryChangeList, "Group Capacity", ( int? ) null, Entity.GroupCapacity );

                            // if this is a new record, but is saved with IsActive=False, set the InactiveDateTime if it isn't set already
                            if ( !Entity.IsActive )
                            {
                                Entity.InactiveDateTime = Entity.InactiveDateTime ?? RockDateTime.Now;
                            }

                            break;
                        }

                    case EntityContextState.Modified:
                        {
                            var originalIsActive = OriginalValues[nameof( Group.IsActive )].ToStringSafe().AsBoolean();
                            DateTime? originalInactiveDateTime = OriginalValues[nameof( Entity.InactiveDateTime )].ToStringSafe().AsDateTime();

                            var originalIsArchived = OriginalValues[nameof( Group.IsArchived )].ToStringSafe().AsBoolean();
                            DateTime? originalArchivedDateTime = OriginalValues[nameof( Group.ArchivedDateTime )].ToStringSafe().AsDateTime();

                            History.EvaluateChange( HistoryChangeList, "Name", OriginalValues[nameof( Group.Name )].ToStringSafe(), Entity.Name );
                            History.EvaluateChange( HistoryChangeList, "Description", OriginalValues[nameof( Group.Description )].ToStringSafe(), Entity.Description );
                            History.EvaluateChange( HistoryChangeList, "Group Type", OriginalValues[nameof( Group.GroupTypeId )].ToStringSafe().AsIntegerOrNull(), Entity.GroupType, Entity.GroupTypeId );
                            History.EvaluateChange( HistoryChangeList, "Campus", OriginalValues[nameof( Group.CampusId )].ToStringSafe().AsIntegerOrNull(), Entity.Campus, Entity.CampusId );
                            History.EvaluateChange( HistoryChangeList, "Security Role", OriginalValues[nameof( Group.IsSecurityRole )].ToStringSafe().AsBoolean(), Entity.IsSecurityRole );
                            History.EvaluateChange( HistoryChangeList, "Active", originalIsActive, Entity.IsActive );
                            History.EvaluateChange( HistoryChangeList, "Allow Guests", OriginalValues[nameof( Group.AllowGuests )].ToStringSafe().AsBooleanOrNull(), Entity.AllowGuests );
                            History.EvaluateChange( HistoryChangeList, "Public", OriginalValues[nameof( Group.IsPublic )].ToStringSafe().AsBoolean(), Entity.IsPublic );
                            History.EvaluateChange( HistoryChangeList, "Group Capacity", OriginalValues[nameof( Group.GroupCapacity )].ToStringSafe().AsIntegerOrNull(), Entity.GroupCapacity );
                            History.EvaluateChange( HistoryChangeList, "Archived", OriginalValues[nameof( Group.IsArchived )].ToStringSafe().AsBoolean(), Entity.IsArchived );

                            // IsActive was modified, set the InactiveDateTime if it changed to Inactive, or set it to NULL if it changed to Active
                            if ( originalIsActive != Entity.IsActive )
                            {
                                if ( !Entity.IsActive )
                                {
                                    // if the caller didn't already set InactiveDateTime, set it to now
                                    Entity.InactiveDateTime = Entity.InactiveDateTime ?? RockDateTime.Now;
                                }
                                else
                                {
                                    Entity.InactiveDateTime = null;
                                }

                                DateTime? newInactiveDateTime = Entity.InactiveDateTime;

                                UpdateGroupMembersActiveStatusFromGroupStatus( rockContext, originalIsActive, originalInactiveDateTime, Entity.IsActive, newInactiveDateTime );
                            }


                            // IsArchived was modified, set the ArchivedDateTime if it changed to IsArchived, or set it to NULL if IsArchived was changed to false
                            if ( originalIsArchived != Entity.IsArchived )
                            {
                                if ( Entity.IsArchived )
                                {
                                    // if the caller didn't already set ArchivedDateTime, set it to now
                                    Entity.ArchivedDateTime = Entity.ArchivedDateTime ?? RockDateTime.Now;
                                }
                                else
                                {
                                    Entity.ArchivedDateTime = null;
                                }

                                DateTime? newArchivedDateTime = Entity.ArchivedDateTime;

                                UpdateGroupMembersArchivedValueFromGroupArchivedValue( rockContext, originalIsArchived, originalArchivedDateTime, Entity.IsArchived, newArchivedDateTime );
                            }

                            // If Campus is modified for an existing Family Group, set a flag to trigger updates for calculated field Person.PrimaryCampusId.
                            var group = Entity as Group;

                            var familyGroupTypeId = GroupTypeCache.GetFamilyGroupType().Id;

                            _FamilyCampusIsChanged = ( group.GroupTypeId == familyGroupTypeId
                                                       && group.CampusId.GetValueOrDefault( 0 ) != OriginalValues[nameof( Entity.CampusId )].ToStringSafe().AsInteger() );

                            break;
                        }

                    case EntityContextState.Deleted:
                        {
                            HistoryChangeList.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, null );

                            // manually delete any grouprequirements of this group since it can't be cascade deleted
                            var groupRequirementService = new GroupRequirementService( rockContext );
                            var groupRequirements = groupRequirementService.Queryable().Where( a => a.GroupId.HasValue && a.GroupId == Entity.Id ).ToList();
                            if ( groupRequirements.Any() )
                            {
                                groupRequirementService.DeleteRange( groupRequirements );
                            }

                            // manually set any attendance search group ids to null
                            var attendanceService = new AttendanceService( rockContext );
                            var attendancesToNullSearchResultGroupId = attendanceService.Queryable()
                                .Where( a =>
                                    a.SearchResultGroupId.HasValue &&
                                    a.SearchResultGroupId.Value == Entity.Id );

                            rockContext.BulkUpdate( attendancesToNullSearchResultGroupId, a => new Attendance { SearchResultGroupId = null } );

                            // since we can't put a CascadeDelete on both Attendance.Occurrence.GroupId and Attendance.OccurrenceId, manually delete all Attendance records associated with this GroupId
                            var attendancesToDelete = attendanceService.Queryable()
                                .Where( a =>
                                    a.Occurrence.GroupId.HasValue &&
                                    a.Occurrence.GroupId.Value == Entity.Id );
                            if ( attendancesToDelete.Any() )
                            {
                                rockContext.BulkDelete( attendancesToDelete );
                            }

                            // This should eventually be accomplished with a cascade delete within the GroupMemberAssignmentConfiguration, once the migration token moves back to the develop branch.
                            var groupMemberAssignmentsToDelete = new GroupMemberAssignmentService( rockContext )
                                .Queryable()
                                .Where( a => a.GroupMember.GroupId == Entity.Id );
                            if ( groupMemberAssignmentsToDelete.Any() )
                            {
                                rockContext.BulkDelete( groupMemberAssignmentsToDelete );
                            }

                            break;
                        }
                }

                base.PreSave();
            }

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

                if ( HistoryChangeList?.Any() == true )
                {
                    HistoryService.SaveChanges( rockContext, typeof( Group ), Rock.SystemGuid.Category.HISTORY_GROUP_CHANGES.AsGuid(), Entity.Id, HistoryChangeList, Entity.Name, null, null, true, Entity.ModifiedByPersonAliasId, rockContext.SourceOfChange );
                }

                if ( _FamilyCampusIsChanged )
                {
                    PersonService.UpdatePrimaryFamilyByGroup( Entity.Id, rockContext );
                }

                base.PostSave();
            }

            /// <summary>
            /// Updates the group members active status from the group's active status.
            /// </summary>
            /// <param name="rockContext">The rock context.</param>
            /// <param name="originalIsActive">if set to <c>true</c> [old active status].</param>
            /// <param name="originalInactiveDateTime">The old inactive date time.</param>
            /// <param name="newActiveStatus">if set to <c>true</c> [new active status].</param>
            /// <param name="newInactiveDateTime">The new inactive date time.</param>
            private void UpdateGroupMembersActiveStatusFromGroupStatus( RockContext rockContext, bool originalIsActive, DateTime? originalInactiveDateTime, bool newActiveStatus, DateTime? newInactiveDateTime )
            {
                if ( originalIsActive == newActiveStatus || Entity.Id == 0 )
                {
                    // only change GroupMember status if the Group's status was changed 
                    return;
                }

                var groupMemberQuery = new GroupMemberService( rockContext ).Queryable().Where( a => a.GroupId == Entity.Id );

                if ( newActiveStatus == false )
                {
                    // group was changed to from Active to Inactive, so change all Active/Pending GroupMembers to Inactive and stamp their inactivate DateTime to be the same as the group's inactive DateTime.
                    foreach ( var groupMember in groupMemberQuery.Where( a => a.GroupMemberStatus != GroupMemberStatus.Inactive ).ToList() )
                    {
                        groupMember.GroupMemberStatus = GroupMemberStatus.Inactive;
                        groupMember.InactiveDateTime = newInactiveDateTime;
                    }
                }
                else if ( originalInactiveDateTime.HasValue )
                {
                    // group was changed to from Inactive to Active, so change all Inactive GroupMembers to Active if their InactiveDateTime is within 24 hours of the Group's InactiveDateTime
                    foreach ( var groupMember in groupMemberQuery.Where( a => a.GroupMemberStatus == GroupMemberStatus.Inactive && a.InactiveDateTime.HasValue && Math.Abs( SqlFunctions.DateDiff( "hour", a.InactiveDateTime.Value, originalInactiveDateTime.Value ).Value ) < 24 ).ToList() )
                    {
                        groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                        groupMember.InactiveDateTime = newInactiveDateTime;

                        if ( !groupMember.IsValidGroupMember( rockContext ) )
                        {
                            // Don't fail the entire operation for a single invalid member's reactivation attempt.
                            rockContext.Entry( groupMember ).State = EntityState.Detached;
                        }
                    }
                }
            }

            /// <summary>
            /// Updates the group members IsArchived value from the group's IsArchived value.
            /// </summary>
            /// <param name="rockContext">The rock context.</param>
            /// <param name="originalIsArchived">if set to <c>true</c> [original is archived].</param>
            /// <param name="originalArchivedDateTime">The original archived date time.</param>
            /// <param name="newIsArchived">if set to <c>true</c> [new is archived].</param>
            /// <param name="newArchivedDateTime">The new archived date time.</param>
            private void UpdateGroupMembersArchivedValueFromGroupArchivedValue( RockContext rockContext, bool originalIsArchived, DateTime? originalArchivedDateTime, bool newIsArchived, DateTime? newArchivedDateTime )
            {
                if ( originalIsArchived == newIsArchived || Entity.Id == 0 )
                {
                    // only change GroupMember archived value if the Group's archived value was changed 
                    return;
                }

                // IMPORTANT: When dealing with Archived Groups or GroupMembers, we always need to get
                // a query without the "filter" (AsNoFilter) that comes from the GroupConfiguration and/or
                // GroupMemberConfiguration because otherwise the query will not include archived items.
                var groupMemberQuery = new GroupMemberService( rockContext ).AsNoFilter().Where( a => a.GroupId == Entity.Id );

                if ( newIsArchived )
                {
                    // group IsArchived was changed from false to true, so change all archived GroupMember's IsArchived to true and stamp their IsArchivedDateTime to be the same as the group's IsArchivedDateTime.
                    foreach ( var groupMember in groupMemberQuery.Where( a => a.IsArchived == false ).ToList() )
                    {
                        groupMember.IsArchived = true;
                        groupMember.ArchivedDateTime = newArchivedDateTime;
                    }
                }
                else if ( originalArchivedDateTime.HasValue )
                {
                    // group IsArchived was changed from true to false, so change all archived GroupMember's IsArchived if their ArchivedDateTime is within 24 hours of the Group's ArchivedDateTime
                    foreach ( var groupMember in groupMemberQuery.Where( a => a.IsArchived == true && a.ArchivedDateTime.HasValue && Math.Abs( SqlFunctions.DateDiff( "hour", a.ArchivedDateTime.Value, originalArchivedDateTime.Value ).Value ) < 24 ).ToList() )
                    {
                        groupMember.IsArchived = false;
                        groupMember.ArchivedDateTime = newArchivedDateTime;
                    }
                }
            }
        }
    }
}
