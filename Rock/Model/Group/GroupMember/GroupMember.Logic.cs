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
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;

using Humanizer;

using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class GroupMember
    {
        #region Methods

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check the default authorization on the current type, and
        /// then the authorization on the Rock.Security.GlobalDefault entity
        /// </summary>
        public override ISecured ParentAuthority
        {
            get
            {
                if ( this.Group != null )
                {
                    return this.Group;
                }
                else
                {
                    return base.ParentAuthority;
                }
            }
        }

        /// <summary>
        /// An optional additional parent authority.  (i.e for Groups, the GroupType is main parent
        /// authority, but parent group is an additional parent authority )
        /// </summary>
        public override ISecured ParentAuthorityPre
        {
            get
            {
                if ( this.Group != null && this.Group.GroupTypeId > 0 )
                {
                    GroupTypeCache groupType = GroupTypeCache.Get( this.Group.GroupTypeId );
                    return groupType;
                }
                else
                {
                    return base.ParentAuthorityPre;
                }
            }
        }

        /// <summary>
        /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        /// <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsAuthorized( string action, Person person )
        {
            /* 2020-09-28  MDP
             GroupMember's Group record has a special MANAGE_MEMBERS action that grants access to managing members.
             This is effectively the same as EDIT. So, if checking security on EDIT, also check Group's
             MANAGE_MEMBERS (in case they have MANAGE_MEMBERS but not EDIT)

             Possible 'action' parameters on GroupMember are
             1) VIEW
             2) EDIT
             3) ADMINISTRATE

             NOTE: MANAGE_MEMBERS is NOT a possible action on GroupMember. However, this can be confusing
             because MANAGE_MEMBERS is a possible action on Group (which would grant EDIT on its group members)

             This is how this has implemented
             - If they can EDIT a Group, then can Manage Group Members (regardless if the ManageMembers settings(
             - If they can't EDIT a Group, but can Manage Members, then can EDIT (which includes Add and Delete) group members.
                - Note that this is fairly complex, see Group.IsAuthorized for how this should work

             For areas of Rock that check for the ability to EDIT (which includes Add and Delete) group members,
             this has been implemented as allowing EDIT on GroupMember, regardless of the ManageMembers setting.
               - See https://github.com/SparkDevNetwork/Rock/blob/85197802dc0fe88afa32ef548fc44fa1d4e31813/RockWeb/Blocks/Groups/GroupMemberDetail.ascx.cs#L303
                  and https://github.com/SparkDevNetwork/Rock/blob/85197802dc0fe88afa32ef548fc44fa1d4e31813/RockWeb/Blocks/Groups/GroupMemberList.ascx.cs#L213

             */

            if ( action.Equals( Rock.Security.Authorization.EDIT, StringComparison.OrdinalIgnoreCase ) )
            {
                // first, see if they auth'd using normal AUTH rules
                var isAuthorized = base.IsAuthorized( action, person );
                if ( isAuthorized )
                {
                    return isAuthorized;
                }

                // now check if they are auth'd to EDIT or MANAGE_MEMBERS on this GroupMember's Group
                var group = this.Group ?? new GroupService( new RockContext() ).Get( this.GroupId );

                if ( group != null )
                {
                    // if they have EDIT on the group, they can edit GroupMember records
                    var canEditMembers = group.IsAuthorized( Rock.Security.Authorization.EDIT, person );
                    if ( !canEditMembers )
                    {
                        // if they don't have EDIT on the group, but do have MANAGE_MEMBERS, then they can 'edit' group members
                        canEditMembers = group.IsAuthorized( Rock.Security.Authorization.MANAGE_MEMBERS, person );
                    }

                    return canEditMembers;
                }
            }

            return base.IsAuthorized( action, person );
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// NOTE: Try using IsValidGroupMember instead
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValid
        {
            get
            {
                using ( var rockContext = new RockContext() )
                {
                    return this.IsValidGroupMember( rockContext );
                }
            }
        }

        /// <summary>
        /// Calls IsValid with the specified context (to avoid deadlocks)
        /// Try to call this instead of IsValid when possible.
        /// Note that this same validation will be done by the service layer when SaveChanges() is called
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        ///   <c>true</c> if [is valid group member] [the specified rock context]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValidGroupMember( RockContext rockContext )
        {
            var result = base.IsValid;
            if ( result )
            {
                string errorMessage;

                if ( !ValidateGroupMembership( rockContext, out errorMessage ) )
                {
                    ValidationResults.Add( new ValidationResult( errorMessage ) );
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Validates the group member does not already exist based on GroupId, GroupRoleId, and PersonId.
        /// Returns false if there is a duplicate member problem.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ValidateGroupMemberDuplicateMember( RockContext rockContext, GroupTypeCache groupType, out string errorMessage )
        {
            if ( GroupService.AllowsDuplicateMembers() )
            {
                errorMessage = string.Empty;
                return true;
            }

            bool isNewOrChangedGroupMember = this.IsNewOrChangedGroupMember( rockContext );

            // if this isn't a new group member, then we can't really do anything about a duplicate record since it already existed before editing this record, so treat it as valid
            if ( !isNewOrChangedGroupMember )
            {
                errorMessage = string.Empty;
                return true;
            }

            errorMessage = string.Empty;
            var groupService = new GroupService( rockContext );

            // If the group member record is changing (new person, role, archive status, active status) check if there are any duplicates of this Person and Role for this group (besides the current record).
            var duplicateGroupMembers = new GroupMemberService( rockContext ).GetByGroupIdAndPersonId( this.GroupId, this.PersonId ).Where( a => a.GroupRoleId == this.GroupRoleId && this.Id != a.Id );
            if ( duplicateGroupMembers.Any() )
            {
                var person = this.Person ?? new PersonService( rockContext ).GetNoTracking( this.PersonId );

                var groupRole = groupType.Roles.Where( a => a.Id == this.GroupRoleId ).FirstOrDefault();
                errorMessage = $"{person} already belongs to the {groupRole.Name.ToLower()} role for this {groupType.GroupTerm.ToLower()}, and cannot be added again with the same role";

                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the group membership.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ValidateGroupMembership( RockContext rockContext, out string errorMessage )
        {
            errorMessage = string.Empty;
            var group = this.Group ?? new GroupService( rockContext ).GetNoTracking( this.GroupId );

            var groupType = GroupTypeCache.Get( group.GroupTypeId );
            if ( groupType == null )
            {
                // For some reason we could not get a GroupType
                errorMessage = $"The GroupTypeId {group.GroupTypeId} used by {this.Group.Name} does not exist.";
                return false;
            }

            var groupRole = groupType.Roles.Where( a => a.Id == this.GroupRoleId ).FirstOrDefault();
            if ( groupRole == null )
            {
                // This is the only point in the method where we need a person object loaded.
                this.Person = this.Person ?? new PersonService( rockContext ).GetNoTracking( this.PersonId );

                // For some reason we could not get a GroupTypeRole for the GroupRoleId for this GroupMember.
                errorMessage = $"The GroupRoleId {this.GroupRoleId} for the group member {this.Person.FullName} in group {group.Name} does not exist in the GroupType.Roles for GroupType {groupType.Name}.";
                return false;
            }

            // Verify duplicate role/person
            if ( !GroupService.AllowsDuplicateMembers() )
            {
                if ( !ValidateGroupMemberDuplicateMember( rockContext, groupType, out errorMessage ) )
                {
                    return false;
                }
            }

            // Verify max group role membership
            if ( groupRole.MaxCount.HasValue && this.GroupMemberStatus == GroupMemberStatus.Active )
            {
                int memberCountInRole = group.Members
                        .Where( m =>
                            m.GroupRoleId == this.GroupRoleId
                            && m.GroupMemberStatus == GroupMemberStatus.Active )
                        .Where( m => m != this )
                        .Count();

                bool roleMembershipAboveMax = false;

                // if adding new group member..
                if ( this.Id.Equals( 0 ) )
                {
                    // verify that active count has not exceeded the max
                    if ( ( memberCountInRole + 1 ) > groupRole.MaxCount )
                    {
                        roleMembershipAboveMax = true;
                    }
                }
                else
                {
                    // if existing group member changing role or status..
                    if ( this.IsStatusOrRoleModified( rockContext ) )
                    {
                        // verify that active count has not exceeded the max
                        if ( ( memberCountInRole + 1 ) > groupRole.MaxCount )
                        {
                            roleMembershipAboveMax = true;
                        }
                    }
                }

                // throw error if above max.. do not proceed
                if ( roleMembershipAboveMax )
                {
                    errorMessage = $"The number of {groupRole.Name.Pluralize().ToLower()} for this {groupType.GroupTerm.ToLower()} is above its maximum allowed limit of {groupRole.MaxCount:N0} active {groupType.GroupMemberTerm.Pluralize( groupRole.MaxCount == 1 ).ToLower()}.";
                    return false;
                }
            }

            // if the GroupMember is getting Added (or if Person or Role is different), and if this Group has requirements that must be met before the person is added, check those
            if ( this.IsNewOrChangedGroupMember( rockContext ) )
            {
                if ( group.GetGroupRequirements( rockContext ).Any( a => a.MustMeetRequirementToAddMember ) )
                {
                    var requirementStatusesRequiredForAdd = group.PersonMeetsGroupRequirements( rockContext, this.PersonId, this.GroupRoleId )
                        .Where( a => a.MeetsGroupRequirement == MeetsGroupRequirement.NotMet
                        && ( ( a.GroupRequirement.GroupRequirementType.RequirementCheckType != RequirementCheckType.Manual ) && ( a.GroupRequirement.MustMeetRequirementToAddMember == true ) ) );

                    if ( requirementStatusesRequiredForAdd.Any() )
                    {
                        // deny if any of the non-manual MustMeetRequirementToAddMember requirements are not met
                        errorMessage = "This person must meet the following requirements before they are added or made an active member in this group: "
                            + requirementStatusesRequiredForAdd
                            .Select( a => string.Format( "{0}", a.GroupRequirement.GroupRequirementType ) )
                            .ToList().AsDelimited( ", " );

                        return false;
                    }
                }
            }

            // check group capacity
            if ( groupType.GroupCapacityRule == GroupCapacityRule.Hard && group.GroupCapacity.HasValue && this.GroupMemberStatus == GroupMemberStatus.Active )
            {
                var currentActiveGroupMemberCount = group.ActiveMembers().Count( gm => gm.Id != Id );
                var existingGroupMembershipForPerson = group.Members.Where( m => m.PersonId == this.PersonId );

                // check if this would be adding an active group member (new active group member or changing existing group member status to active)
                if ( this.Id == 0 )
                {
                    if ( currentActiveGroupMemberCount + 1 > group.GroupCapacity )
                    {
                        errorMessage = "Adding this individual would put the group over capacity.";
                        return false;
                    }
                }
                else if ( existingGroupMembershipForPerson.Where( m => m.Id == this.Id && m.GroupMemberStatus != GroupMemberStatus.Active ).Any() )
                {
                    if ( currentActiveGroupMemberCount + 1 > group.GroupCapacity )
                    {
                        errorMessage = "Adding this individual would put the group over capacity.";
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether this is an existing record but the GroupMemberStatus or GroupRoleId was modified since loaded from the database
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public bool IsStatusOrRoleModified( RockContext rockContext )
        {
            if ( this.Id == 0 )
            {
                // new group member
                return false;
            }
            else
            {
                var groupMemberService = new GroupMemberService( rockContext );
                var databaseGroupMemberRecord = groupMemberService.Get( this.Id );

                if ( databaseGroupMemberRecord == null )
                {
                    return false;
                }

                // existing groupmember record, but person or role was changed
                var hasChanged = this.GroupMemberStatus != databaseGroupMemberRecord.GroupMemberStatus
                    || this.GroupRoleId != databaseGroupMemberRecord.GroupRoleId
                    || this.IsArchived != databaseGroupMemberRecord.IsArchived;

                if ( !hasChanged )
                {
                    var entry = rockContext.Entry( this );
                    if ( entry != null )
                    {
                        hasChanged = rockContext.Entry( this ).Property( nameof( this.GroupMemberStatus ) )?.IsModified == true
                            || rockContext.Entry( this ).Property( nameof( this.GroupRoleId ) )?.IsModified == true
                            || rockContext.Entry( this ).Property( nameof( this.IsArchived ) )?.IsModified == true;
                    }
                }

                return hasChanged;
            }
        }

        /// <summary>
        /// Determines whether this is a new group member (just added), if either Person or Role is different than what is stored in the database, if person is restored from archived, or their status is getting changed to Active
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public bool IsNewOrChangedGroupMember( RockContext rockContext )
        {
            if ( this.Id == 0 )
            {
                // new group member
                return true;
            }
            else
            {
                var groupMemberService = new GroupMemberService( rockContext );
                var databaseGroupMemberRecord = groupMemberService.Get( this.Id );

                // existing group member record, but person or role or archive status was changed, or active status is getting changed to true
                var hasChanged = this.PersonId != databaseGroupMemberRecord.PersonId
                    || this.GroupRoleId != databaseGroupMemberRecord.GroupRoleId
                    || ( databaseGroupMemberRecord.IsArchived && databaseGroupMemberRecord.IsArchived != this.IsArchived )
                    || ( databaseGroupMemberRecord.GroupMemberStatus != GroupMemberStatus.Active && this.GroupMemberStatus == GroupMemberStatus.Active );

                if ( !hasChanged )
                {
                    // no change detected by comparing to the database record, so check if the ChangeTracker detects that these fields were modified
                    var entry = rockContext.Entry( this );
                    if ( entry != null && entry.State != EntityState.Detached )
                    {
                        var originalStatus = ( GroupMemberStatus? ) rockContext.Entry( this ).OriginalValues[nameof( this.GroupMemberStatus )];
                        var newStatus = ( GroupMemberStatus? ) rockContext.Entry( this ).CurrentValues[nameof (this.GroupMemberStatus )];

                        hasChanged = rockContext.Entry( this ).Property( nameof( this.PersonId ) )?.IsModified == true
                        || rockContext.Entry( this ).Property( nameof( this.GroupRoleId ) )?.IsModified == true
                        || ( rockContext.Entry( this ).Property( nameof( this.IsArchived ) )?.IsModified == true && !rockContext.Entry( this ).Property( nameof( this.IsArchived ) ).ToStringSafe().AsBoolean() )
                        || ( originalStatus != GroupMemberStatus.Active && newStatus == GroupMemberStatus.Active );
                    }
                }

                return hasChanged;
            }
        }

        /// <summary>
        /// Gets the group requirements statuses.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IEnumerable<GroupRequirementStatus> GetGroupRequirementsStatuses( RockContext rockContext )
        {
            var metRequirements = this.GroupMemberRequirements.Select( a => new
            {
                GroupRequirementId = a.GroupRequirement.Id,
                MeetsGroupRequirement = a.RequirementMetDateTime.HasValue
                    ? a.RequirementWarningDateTime.HasValue ? MeetsGroupRequirement.MeetsWithWarning : MeetsGroupRequirement.Meets
                    : MeetsGroupRequirement.NotMet,
                a.RequirementWarningDateTime,
                a.LastRequirementCheckDateTime,
            } );

            // get all the group requirements that apply the group member's role
            var allGroupRequirements = this.Group.GetGroupRequirements( rockContext ).Where( a => !a.GroupRoleId.HasValue || a.GroupRoleId == this.GroupRoleId ).OrderBy( a => a.GroupRequirementType.Name ).ToList();

            // outer join on group requirements
            var result = from groupRequirement in allGroupRequirements
                         join metRequirement in metRequirements on groupRequirement.Id equals metRequirement.GroupRequirementId into j
                         from metRequirement in j.DefaultIfEmpty()
                         select new GroupRequirementStatus
                         {
                             GroupRequirement = groupRequirement,
                             MeetsGroupRequirement = metRequirement != null ? metRequirement.MeetsGroupRequirement : MeetsGroupRequirement.NotMet,
                             RequirementWarningDateTime = metRequirement != null ? metRequirement.RequirementWarningDateTime : null,
                             LastRequirementCheckDateTime = metRequirement != null ? metRequirement.LastRequirementCheckDateTime : null
                         };

            return result;
        }

        /// <summary>
        /// If Group has GroupRequirements, will calculate and update the GroupMemberRequirements for the GroupMember, then save the changes to the database
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="saveChanges">if set to <c>true</c> [save changes].</param>
        public void CalculateRequirements( RockContext rockContext, bool saveChanges = true )
        {
            // recalculate and store in the database if the groupmember isn't new or changed
            var groupMemberRequirementsService = new GroupMemberRequirementService( rockContext );
            var group = this.Group ?? new GroupService( rockContext ).Queryable( "GroupRequirements" ).FirstOrDefault( a => a.Id == this.GroupId );

            if ( !group.GetGroupRequirements( rockContext ).Any() )
            {
                // group doesn't have requirements, so clear any existing group member requirements and save if necessary.
                if ( GroupMemberRequirements.Any() )
                {
                    GroupMemberRequirements.Clear();

                    if ( saveChanges )
                    {
                        rockContext.SaveChanges();
                    }
                }

                return;
            }

            ClearInapplicableGroupRequirements( rockContext );

            var updatedRequirements = group.PersonMeetsGroupRequirements( rockContext, this.PersonId, this.GroupRoleId );

            foreach ( var updatedRequirement in updatedRequirements )
            {
                updatedRequirement.GroupRequirement.UpdateGroupMemberRequirementResult( rockContext, updatedRequirement.PersonId, this.GroupId, updatedRequirement.MeetsGroupRequirement );
            }

            if ( saveChanges )
            {
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Remoes any group requirements that are not eligible for the group member's role.  This is necessary
        /// if the group member has changed roles.
        /// </summary>
        /// <param name="rockContext">The <see cref="RockContext"/>.</param>
        private void ClearInapplicableGroupRequirements( RockContext rockContext )
        {
            var inapplicableGroupRequirements = GroupMemberRequirements
                .Where( r => r.GroupRequirement.GroupRoleId != this.GroupRoleId )
                .ToList();

            var groupMemberRequirementsService = new GroupMemberRequirementService( rockContext );
            groupMemberRequirementsService.DeleteRange( inapplicableGroupRequirements );
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool IsEqualTo( GroupMember other )
        {
            return
                this.GroupId == other.GroupId &&
                this.PersonId == other.PersonId &&
                this.GroupRoleId == other.GroupRoleId;
        }

        /// <summary>
        /// Get a list of all inherited Attributes that should be applied to this entity.
        /// </summary>
        /// <returns>A list of all inherited AttributeCache objects.</returns>
        public override List<AttributeCache> GetInheritedAttributes( Rock.Data.RockContext rockContext )
        {
            var group = this.Group;
            if ( group == null && this.GroupId > 0 )
            {
                group = new GroupService( rockContext )
                    .Queryable().AsNoTracking()
                    .FirstOrDefault( g => g.Id == this.GroupId );
            }

            if ( group != null )
            {
                var groupType = group.GroupType;
                if ( groupType == null && group.GroupTypeId > 0 )
                {
                    // Can't use GroupTypeCache here since it loads attributes and would
                    // result in a recursive stack overflow situation.
                    groupType = new GroupTypeService( rockContext )
                        .Queryable().AsNoTracking()
                        .FirstOrDefault( t => t.Id == group.GroupTypeId );
                }

                if ( groupType != null )
                {
                    return groupType.GetInheritedAttributesForQualifier( rockContext, TypeId, "GroupTypeId" );
                }
            }

            return null;
        }

        #endregion

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return null;
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            var rockContext = ( RockContext ) dbContext;

            var group = this.Group ?? new GroupService( rockContext ).GetNoTracking( this.GroupId );

            if ( group != null )
            {
                var groupType = GroupTypeCache.Get( group.GroupTypeId, rockContext );
                if ( group.IsSecurityRole || groupType?.Guid == Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() )
                {
                    RoleCache.FlushItem( group.Id );
                    Rock.Security.Authorization.Clear();
                }
            }
        }

        #endregion ICacheable
    }
}
