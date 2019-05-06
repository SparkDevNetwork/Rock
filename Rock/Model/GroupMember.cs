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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Humanizer;

using Rock.Data;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a member of a group in Rock. A group member is a <see cref="Rock.Model.Person"/> who has a relationship with a <see cref="Rock.Model.Group"/>.
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupMember" )]
    [DataContract]
    public partial class GroupMember : Model<GroupMember>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this GroupMember is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this GroupMember is a part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Group"/> that this GroupMember is associated with. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Group"/> that the GroupMember is associated with.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Person"/> that is represented by the GroupMember. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> who is reprensented by the GroupMember.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the GroupMember's <see cref="Rock.Model.GroupMember.GroupRole"/> in the <see cref="Rock.Model.Group"/>. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupTypeRole"/> that the Group Member is in.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int GroupRoleId { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the note.
        /// </value>
        [DataMember]
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the GroupMember's status (<see cref="Rock.Model.GroupMemberStatus"/>) in the Group. This value is required.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.GroupMemberStatus"/> enum value that represents the GroupMember's status in the group.  A <c>GroupMemberStatus.Active</c> indicates that the GroupMember is active,
        /// A <c>GroupMemberStatus.Inactive</c> value indicates that the GroupMember is not active, otherwise their GroupMemberStatus will be <c>GroupMemberStatus.Pending</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public GroupMemberStatus GroupMemberStatus
        {
            get { return _groupMemberStatus; }
            set { _groupMemberStatus = value; }
        }

        private GroupMemberStatus _groupMemberStatus = GroupMemberStatus.Active;

        /// <summary>
        /// Gets or sets the number of additional guests that member will be bring to group.  Only applies when group has the 'AllowGuests' flag set to true.
        /// </summary>
        /// <value>
        /// The guest count.
        /// </value>
        [DataMember]
        public int? GuestCount { get; set; }

        /// <summary>
        /// Gets or sets the date/time that the person was added to the group.
        /// Rock will automatically set this value when a group member is added if it isn't set manually
        /// </summary>
        /// <value>
        /// The date added.
        /// </value>
        [DataMember]
        public DateTime? DateTimeAdded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is notified.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is notified; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsNotified { get; set; }

        /// <summary>
        /// Gets or sets the order of Groups of the Group's GroupType for the Person.
        /// For example, if this is a FamilyGroupType, GroupOrder can be used to specify which family should be 
        /// listed as 1st (primary), 2nd, 3rd, etc for the Person.
        /// If GroupOrder is null, the group will be listed in no particular order after the ones that do have a GroupOrder.
        /// NOTE: Use int.MaxValue in OrderBy statements for null GroupOrder values
        /// </summary>
        /// <value>
        /// The group order.
        /// </value>
        [DataMember]
        public int? GroupOrder { get; set; }

        /// <summary>
        /// Gets or sets the date that this group member became inactive
        /// </summary>
        /// <value>
        /// The in active date time.
        /// </value>
        [DataMember]
        public DateTime? InactiveDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this group member is archived (soft deleted)
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is archived; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsArchived { get; set; } = false;

        /// <summary>
        /// Gets or sets the date time that this group member was archived (soft deleted)
        /// </summary>
        /// <value>
        /// The archived date time.
        /// </value>
        [DataMember]
        public DateTime? ArchivedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the PersonAliasId that archived (soft deleted) this group member
        /// </summary>
        /// <value>
        /// The archived by person alias identifier.
        /// </value>
        [DataMember]
        public int? ArchivedByPersonAliasId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> representing the GroupMember.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Person"/> representing the person who is the GroupMember.
        /// </value>
        [DataMember]
        public virtual Model.Person Person { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/> that the GroupMember belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Group"/> representing the Group that the GroupMember is a part of.
        /// </value>
        [LavaInclude]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the the GroupMember's role (<see cref="Rock.Model.GroupTypeRole"/>) in the <see cref="Rock.Model.Group"/>.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.GroupTypeRole"/> representing the GroupMember's <see cref="Rock.Model.GroupTypeRole"/> in the <see cref="Rock.Model.Group"/>.
        /// </value>
        [DataMember]
        public virtual GroupTypeRole GroupRole { get; set; }

        /// <summary>
        /// Gets or sets the PersonAlias that archived (soft deleted) this group member
        /// </summary>
        /// <value>
        /// The archived by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias ArchivedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the group member requirements.
        /// </summary>
        /// <value>
        /// The group member requirements.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupMemberRequirement> GroupMemberRequirements
        {
            get { return _groupMemberRequirements ?? ( _groupMemberRequirements = new Collection<GroupMemberRequirement>() ); }
            set { _groupMemberRequirements = value; }
        }

        private ICollection<GroupMemberRequirement> _groupMemberRequirements;

        /// <summary>
        /// Gets or sets the person history changes.
        /// </summary>
        /// <value>
        /// The person history changes.
        /// </value>
        [NotMapped]
        private List<HistoryItem> HistoryChanges { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Person?.ToString();
        }

        /// <summary>
        /// Actions to perform prior to saving the changes to the GroupMember object in the context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        public override void PreSaveChanges( Rock.Data.DbContext dbContext, DbEntityEntry entry )
        {
            var changeTransaction = new Rock.Transactions.GroupMemberChangeTransaction( entry );
            Rock.Transactions.RockQueue.TransactionQueue.Enqueue( changeTransaction );

            int? oldPersonId = null;
            int? newPersonId = null;

            int? oldGroupId = null;
            int? newGroupId = null;

            switch ( entry.State )
            {
                case EntityState.Added:
                    {
                        oldPersonId = null;
                        newPersonId = PersonId;

                        oldGroupId = null;
                        newGroupId = GroupId;

                        if ( !this.DateTimeAdded.HasValue )
                        {
                            this.DateTimeAdded = RockDateTime.Now;
                        }

                        break;
                    }

                case EntityState.Modified:
                    {
                        oldPersonId = entry.OriginalValues["PersonId"].ToStringSafe().AsIntegerOrNull();
                        newPersonId = PersonId;

                        oldGroupId = entry.OriginalValues["GroupId"].ToStringSafe().AsIntegerOrNull();
                        newGroupId = GroupId;

                        break;
                    }

                case EntityState.Deleted:
                    {
                        oldPersonId = entry.OriginalValues["PersonId"].ToStringSafe().AsIntegerOrNull();
                        newPersonId = null;

                        oldGroupId = entry.OriginalValues["GroupId"].ToStringSafe().AsIntegerOrNull();
                        newGroupId = null;

                        break;
                    }

            }

            var rockContext = (RockContext)dbContext;

            Group group = this.Group;
            if ( group == null )
            {
                group = new GroupService( rockContext ).Get( this.GroupId );
            }

            if ( group != null )
            {
                string oldGroupName = group.Name;
                if ( oldGroupId.HasValue && oldGroupId.Value != ( group.Id ) )
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
                    History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Role", (int?)null, GroupRole, GroupRoleId, rockContext );
                    History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Note", string.Empty, Note );
                    History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Status", null, GroupMemberStatus );
                    History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Guest Count", (int?)null, GuestCount );

                    var addedMemberPerson = this.Person ?? new PersonService( rockContext ).Get( this.PersonId );

                    // add the Person's Name as ValueName and Caption (just in case the groupmember record is deleted later)
                    historyItem.GroupMemberHistoryChangeList.AddChange( History.HistoryVerb.AddedToGroup, History.HistoryChangeType.Record, $"{addedMemberPerson?.FullName}" ).SetCaption( $"{addedMemberPerson?.FullName}" );
                    History.EvaluateChange( historyItem.GroupMemberHistoryChangeList, $"Role", ( int? ) null, GroupRole, GroupRoleId, rockContext );
                    History.EvaluateChange( historyItem.GroupMemberHistoryChangeList, $"Note", string.Empty, Note );
                    History.EvaluateChange( historyItem.GroupMemberHistoryChangeList, $"Status", null, GroupMemberStatus );
                    History.EvaluateChange( historyItem.GroupMemberHistoryChangeList, $"Guest Count", ( int? ) null, GuestCount );
                }

                if ( newPersonId.HasValue && oldPersonId.HasValue && oldPersonId.Value == newPersonId.Value &&
                     newGroupId.HasValue && oldGroupId.HasValue && oldGroupId.Value == newGroupId.Value )
                {
                    // Updated same person in group
                    var historyItem = HistoryChanges.First( h => h.PersonId == newPersonId.Value && h.GroupId == newGroupId.Value );
                    History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Role", entry.OriginalValues["GroupRoleId"].ToStringSafe().AsIntegerOrNull(), GroupRole, GroupRoleId, rockContext );
                    History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Note", entry.OriginalValues["Note"].ToStringSafe(), Note );
                    History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Status", entry.OriginalValues["GroupMemberStatus"].ToStringSafe().ConvertToEnum<GroupMemberStatus>(), GroupMemberStatus );
                    History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Guest Count", entry.OriginalValues["GuestCount"].ToStringSafe().AsIntegerOrNull(), GuestCount );
                    History.EvaluateChange( historyItem.PersonHistoryChangeList, $"{historyItem.Caption} Archived", entry.OriginalValues["IsArchived"].ToStringSafe().AsBoolean(), IsArchived );

                    // If the groupmember was Archived, make sure it is the first GroupMember History change (since they get summarized when doing a HistoryLog and Timeline
                    bool origIsArchived = entry.OriginalValues["IsArchived"].ToStringSafe().AsBoolean();

                    if ( origIsArchived != IsArchived )
                    {
                        var memberPerson = this.Person ?? new PersonService( rockContext ).Get( this.PersonId );
                        if ( IsArchived == true )
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

                    History.EvaluateChange( historyItem.GroupMemberHistoryChangeList, $"Role", entry.OriginalValues["GroupRoleId"].ToStringSafe().AsIntegerOrNull(), GroupRole, GroupRoleId, rockContext );
                    History.EvaluateChange( historyItem.GroupMemberHistoryChangeList, $"Note", entry.OriginalValues["Note"].ToStringSafe(), Note );
                    History.EvaluateChange( historyItem.GroupMemberHistoryChangeList, $"Status", entry.OriginalValues["GroupMemberStatus"].ToStringSafe().ConvertToEnum<GroupMemberStatus>(), GroupMemberStatus );
                    History.EvaluateChange( historyItem.GroupMemberHistoryChangeList, $"Guest Count", entry.OriginalValues["GuestCount"].ToStringSafe().AsIntegerOrNull(), GuestCount );
                }

                if ( oldPersonId.HasValue && oldGroupId.HasValue && 
                    ( !newPersonId.HasValue || newPersonId.Value != oldPersonId.Value || !newGroupId.HasValue || newGroupId.Value != oldGroupId.Value ) )
                {
                    // Removed a person/groupmember in group
                    var historyItem = HistoryChanges.First( h => h.PersonId == oldPersonId.Value && h.GroupId == oldGroupId.Value );

                    historyItem.PersonHistoryChangeList.AddChange( History.HistoryVerb.RemovedFromGroup, History.HistoryChangeType.Record, $"{oldGroupName} Group");

                    var deletedMemberPerson = this.Person ?? new PersonService( rockContext ).Get( this.PersonId );

                    historyItem.GroupMemberHistoryChangeList.AddChange( History.HistoryVerb.RemovedFromGroup, History.HistoryChangeType.Record, $"{deletedMemberPerson?.FullName}" ).SetCaption( $"{deletedMemberPerson?.FullName}" );
                }

                // process universal search indexing if required
                var groupType = GroupTypeCache.Get( group.GroupTypeId );
                if ( groupType != null && groupType.IsIndexEnabled )
                {
                    IndexEntityTransaction transaction = new IndexEntityTransaction();
                    transaction.EntityTypeId = groupType.Id;
                    transaction.EntityId = group.Id;

                    RockQueue.TransactionQueue.Enqueue( transaction );
                }

            }

            base.PreSaveChanges( dbContext, entry );
        }


        /// <summary>
        /// Posts the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( Data.DbContext dbContext )
        {
            if ( HistoryChanges != null )
            {
                foreach ( var historyItem in HistoryChanges )
                {
                    int personId = historyItem.PersonId > 0 ? historyItem.PersonId : PersonId;

                    // if GroupId is 0, it is probably a Group that wasn't saved yet, so get the GroupId from historyItem.Group.Id instead
                    if ( historyItem.GroupId == 0 )
                    {
                        historyItem.GroupId = historyItem.Group?.Id;
                    }

                    HistoryService.SaveChanges( ( RockContext ) dbContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_GROUP_MEMBERSHIP.AsGuid(),
                        personId, historyItem.PersonHistoryChangeList, historyItem.Caption, typeof( Group ), historyItem.GroupId, true, this.ModifiedByPersonAliasId, dbContext.SourceOfChange );

                    HistoryService.SaveChanges( ( RockContext ) dbContext, typeof( GroupMember ), Rock.SystemGuid.Category.HISTORY_GROUP_CHANGES.AsGuid(),
                        this.Id, historyItem.GroupMemberHistoryChangeList, historyItem.Caption, typeof( Group ), historyItem.GroupId, true, this.ModifiedByPersonAliasId, dbContext.SourceOfChange );
                }
            }

            base.PostSaveChanges( dbContext );

            // if this is a GroupMember record on a Family, ensure that AgeClassification, PrimaryFamily is updated
            // NOTE: This is also done on Person.PostSaveChanges in case Birthdate changes
            var groupTypeFamilyRoleIds = GroupTypeCache.GetFamilyGroupType()?.Roles?.Select( a => a.Id ).ToList();
            if ( groupTypeFamilyRoleIds?.Any() == true )
            {
                if ( groupTypeFamilyRoleIds.Contains( this.GroupRoleId ) )
                {
                    PersonService.UpdatePersonAgeClassification( this.PersonId, dbContext as RockContext );
                    PersonService.UpdatePrimaryFamily( this.PersonId, dbContext as RockContext );
                    PersonService.UpdateGivingLeaderId( this.PersonId, dbContext as RockContext );
                }
            }
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
        /// Try to call this instead of IsValid when possible
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
        /// Validates the group member does not already exist based on GroupId, GroupRoleId, and PersonId
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ValidateGroupMemberDuplicateMember( RockContext rockContext, GroupTypeCache groupType, out string errorMessage )
        {
            errorMessage = string.Empty;
            var groupService = new GroupService( rockContext );

            if ( !GroupService.AllowsDuplicateMembers() )
            {
                var groupMember = new GroupMemberService( rockContext ).GetByGroupIdAndPersonIdAndGroupRoleId( this.GroupId, this.PersonId, this.GroupRoleId );
                if ( groupMember != null && groupMember.Id != this.Id )
                {
                    var person = this.Person ?? new PersonService( rockContext ).Get( this.PersonId );

                    var groupRole = groupType.Roles.Where( a => a.Id == this.GroupRoleId ).FirstOrDefault();
                    errorMessage = string.Format(
                        "{0} already belongs to the {1} role for this {2}, and cannot be added again with the same role",
                        person,
                        groupRole.Name.ToLower(),
                        groupType.GroupTerm.ToLower() );

                    return false;
                }
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
            var group = this.Group ?? new GroupService( rockContext ).Queryable().AsNoTracking().Where( g => g.Id == this.GroupId ).FirstOrDefault();

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
                // For some reason we could not get a GroupTypeRole for the GroupRoleId for this GroupMember.
                errorMessage = $"The GroupRoleId {this.GroupRoleId} for the group member {this.Person.FullName} in group {this.Group.Name} does not exist in the GroupType.Roles for GroupType {groupType.Name}.";
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
                        if ( groupRole.MaxCount != null && ( memberCountInRole + 1 ) > groupRole.MaxCount )
                        {
                            roleMembershipAboveMax = true;
                        }
                    }
                }

                // throw error if above max.. do not proceed
                if ( roleMembershipAboveMax )
                {
                    errorMessage = string.Format(
                        "The number of {0} for this {1} is above its maximum allowed limit of {2:N0} active {3}.",
                        groupRole.Name.Pluralize().ToLower(),
                        groupType.GroupTerm.ToLower(),
                        groupRole.MaxCount,
                        groupType.GroupMemberTerm.Pluralize( groupRole.MaxCount == 1 ) ).ToLower();
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
                        errorMessage = "This person must meet the following requirements before they are added to this group: "
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
                var currentActiveGroupMemberCount = group.ActiveMembers().Count();
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
                var hasChanged = this.GroupMemberStatus != databaseGroupMemberRecord.GroupMemberStatus || this.GroupRoleId != databaseGroupMemberRecord.GroupRoleId;

                if ( !hasChanged )
                {
                    var entry = rockContext.Entry( this );
                    if ( entry != null )
                    {
                        hasChanged = rockContext.Entry( this ).Property( "GroupMemberStatus" )?.IsModified == true || rockContext.Entry( this ).Property( "GroupRoleId" )?.IsModified == true;
                    }
                }

                return hasChanged;
            }
        }

        /// <summary>
        /// Determines whether this is a new group member (just added) or if either Person or Role is different than what is stored in the database
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

                // existing groupmember record, but person or role was changed
                var hasChanged = this.PersonId != databaseGroupMemberRecord.PersonId || this.GroupRoleId != databaseGroupMemberRecord.GroupRoleId;

                if ( !hasChanged )
                {
                    var entry = rockContext.Entry( this );
                    if ( entry != null )
                    {
                        hasChanged = rockContext.Entry( this ).Property( "PersonId" )?.IsModified == true || rockContext.Entry( this ).Property( "GroupRoleId" )?.IsModified == true;
                    }
                }

                return hasChanged;
            }
        }

        /// <summary>
        /// Returns the current values of the group requirements statuses for this GroupMember from the last time they were calculated ordered by GroupRequirementType.Name
        /// </summary>
        /// <returns></returns>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use GetGroupRequirementsStatuses(rockContext) instead", true )]
        public IEnumerable<GroupRequirementStatus> GetGroupRequirementsStatuses()
        {
            using ( var rockContext = new RockContext() )
            {
                return GetGroupRequirementsStatuses( rockContext );
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
                // group doesn't have requirements so no need to calculate
                return;
            }

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
            var group = this.Group ?? new GroupService( new RockContext() ).GetNoTracking( this.GroupId );
            if ( group != null )
            {

                var groupType = GroupTypeCache.Get( group.GroupTypeId, (RockContext)dbContext );
                if ( group.IsSecurityRole || groupType?.Guid == Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() )
                {
                    RoleCache.FlushItem( group.Id );
                    Rock.Security.Authorization.Clear();
                }
            }
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Member Configuration class.
    /// </summary>
    public partial class GroupMemberConfiguration : EntityTypeConfiguration<GroupMember>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberConfiguration"/> class.
        /// </summary>
        public GroupMemberConfiguration()
        {
            this.HasRequired( p => p.Person ).WithMany( p => p.Members ).HasForeignKey( p => p.PersonId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.Group ).WithMany( p => p.Members ).HasForeignKey( p => p.GroupId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.GroupRole ).WithMany().HasForeignKey( p => p.GroupRoleId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ArchivedByPersonAlias ).WithMany().HasForeignKey( p => p.ArchivedByPersonAliasId ).WillCascadeOnDelete( false );

            // Tell EF that we never want archived group members. 
            // This will prevent archived members from being included in any GroupMember queries.
            // It will also prevent navigation properties of GroupMember from including archived group members.
            Z.EntityFramework.Plus.QueryFilterManager.Filter<GroupMember>( x => x.Where( m => m.IsArchived == false ) );

            // In the case of GroupMember as a property (not a collection), we DO want to fetch the groupMember record even if it is archived, so ensure that AllowPropertyFilter = false;
            // NOTE: This is not specific to GroupMember, it is for any Filtered Model (currently just Group and GroupMember)
            Z.EntityFramework.Plus.QueryFilterManager.AllowPropertyFilter = false;
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Represents the status of a <see cref="Rock.Model.GroupMember"/> in a <see cref="Rock.Model.Group"/>.
    /// </summary>
    public enum GroupMemberStatus
    {
        /// <summary>
        /// The <see cref="Rock.Model.GroupMember"/> is not an active member of the <see cref="Rock.Model.Group"/>.
        /// </summary>
        Inactive = 0,

        /// <summary>
        /// The <see cref="Rock.Model.GroupMember"/> is an active member of the <see cref="Rock.Model.Group"/>.
        /// </summary>
        Active = 1,

        /// <summary>
        /// The <see cref="Rock.Model.GroupMember">GroupMember's</see> membership in the <see cref="Rock.Model.Group"/> is pending.
        /// </summary>
        Pending = 2
    }

    #endregion

    #region Exceptions

    /// <summary>
    /// Exception to throw if GroupMember validation rules are invalid (and can't be checked using .IsValid)
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class GroupMemberValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberValidationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public GroupMemberValidationException( string message ) : base( message )
        {
        }
    }

    #endregion

    /// <summary>
    /// Helper class for tracking changes
    /// </summary>
    public class HistoryItem 
    {
        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the changes.
        /// </summary>
        /// <value>
        /// The changes.
        /// </value>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use PersonHistoryChangeList or GroupMemberHistoryChangeList instead, depending on what you are doing. " )]
        public List<string> Changes { get; set; }

        /// <summary>
        /// Gets or sets the changes to be written as Person History
        /// </summary>
        /// <value>
        /// The changes.
        /// </value>
        public History.HistoryChangeList PersonHistoryChangeList { get; set; } = new History.HistoryChangeList();

        /// <summary>
        /// Gets or sets the changes to be written as GroupMember History
        /// </summary>
        /// <value>
        /// The group member history change list.
        /// </value>
        public History.HistoryChangeList GroupMemberHistoryChangeList { get; set; } = new History.HistoryChangeList();

        /// <summary>
        /// Gets or sets the caption.
        /// </summary>
        /// <value>
        /// The caption.
        /// </value>
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the group. Use this to get the GroupId on PostSaveChanges if GroupId is 0 (new Group)
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public Group Group { get; set; }
    }
}
