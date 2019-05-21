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
using System.Text;

using Rock.Data;
using Rock.Security;
using Rock.UniversalSearch;
using Rock.UniversalSearch.IndexModels;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents A collection of <see cref="Rock.Model.Person"/> entities. This can be a family, small group, Bible study, security group,  etc. Groups can be hierarchical.
    /// </summary>
    /// <remarks>
    /// In Rock any collection or defined subset of people are considered a group.
    /// </remarks>
    [RockDomain( "Group" )]
    [Table( "Group" )]
    [DataContract]

    // Support Analytics Tables, but only for GroupType Family
    [Analytics( "GroupTypeId", "10", true, true )]
    public partial class Group : Model<Group>, IOrdered, IHasActiveFlag, IRockIndexable, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this Group is a part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this Group is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Group's Parent Group.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the Group's Parent Group.
        /// </value>
        [DataMember]
        public int? ParentGroupId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.GroupType"/> that this Group is a member belongs to. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupType"/> that this group is a member of.
        /// </value>
        [Required]
        [HideFromReporting]
        [DataMember( IsRequired = true )]
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Campus"/> that this Group is associated with.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Campus"/> that the Group is associated with. If the group is not 
        /// associated with a campus, this value is null.
        /// </value>
        [HideFromReporting]
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.CAMPUS )]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        [HideFromReporting]
        [DataMember]
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Group. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the Group. 
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        [Previewable]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the optional description of the group.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the group.
        /// </value>
        [DataMember]
        [Previewable]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this Group is a Security Role. This value is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this Group is a security role, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Previewable]
        public bool IsSecurityRole { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this is an active group. This value is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this group is active, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Previewable]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the display order of the group in the group list and group hierarchy. The lower the number the higher the 
        /// display priority this group has. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the display order of the group.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets whether group allows members to specify additional "guests" that will be part of the group (i.e. attend event)
        /// </summary>
        /// <value>
        /// The allow guests flag
        /// </value>
        [DataMember]
        public bool? AllowGuests { get; set; }

        /// <summary>
        /// Gets or sets whether a group member can only be added if all the GroupRequirements have been met
        /// </summary>
        /// <value>
        /// The must meet requirements to add member.
        /// </value>
        [RockObsolete( "1.7" )]
        [Obsolete( "This no longer is functional. Please use GroupRequirement.MustMeetRequirementToAddMember instead.", true )]
        [NotMapped]
        public bool? MustMeetRequirementsToAddMember { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group should be shown in group finders
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is public; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsPublic { get; set; } = true;

        /// <summary>
        /// Gets or sets the group capacity.
        /// </summary>
        /// <value>
        /// The group capacity.
        /// </value>
        [DataMember]
        public int? GroupCapacity { get; set; }

        /// <summary>
        /// Gets or sets the required signature document type identifier.
        /// </summary>
        /// <value>
        /// The required signature document type identifier.
        /// </value>
        [DataMember]
        public int? RequiredSignatureDocumentTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the date that this group became inactive
        /// </summary>
        /// <value>
        /// The in active date time.
        /// </value>
        [DataMember]
        public DateTime? InactiveDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this group is archived (soft deleted)
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is archived; otherwise, <c>false</c>.
        /// </value>
        [HideFromReporting]
        [DataMember]
        public bool IsArchived { get; set; } = false;

        /// <summary>
        /// Gets or sets the date time that this group was archived (soft deleted)
        /// </summary>
        /// <value>
        /// The archived date time.
        /// </value>
        [HideFromReporting]
        [DataMember]
        public DateTime? ArchivedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the PersonAliasId that archived (soft deleted) this group
        /// </summary>
        /// <value>
        /// The archived by person alias identifier.
        /// </value>
        [HideFromReporting]
        [DataMember]
        public int? ArchivedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the Group Status Id.  DefinedType depends on this group's <see cref="Rock.Model.GroupType.GroupStatusDefinedType"/>
        /// </summary>
        /// <value>
        /// The status value identifier.
        /// </value>
        [DataMember]
        [DefinedValue]
        public int? StatusValueId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether GroupMembers must meet GroupMemberRequirements before they can be scheduled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [scheduling must meet requirements]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool SchedulingMustMeetRequirements { get; set; }

        /// <summary>
        /// Gets or sets the attendance record required for check in.
        /// </summary>
        /// <value>
        /// The attendance record required for check in.
        /// </value>
        [DataMember]
        public AttendanceRecordRequiredForCheckIn AttendanceRecordRequiredForCheckIn { get; set; }

        /// <summary>
        /// Gets or sets the PersonAliasId of the person to notify when a person cancels
        /// </summary>
        /// <value>
        /// The schedule cancellation person alias identifier.
        /// </value>
        [DataMember]
        public int? ScheduleCancellationPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the group administrator person alias identifier.
        /// </summary>
        /// <value>
        /// The group administrator person alias identifier.
        /// </value>
        [DataMember]
        public virtual int? GroupAdministratorPersonAliasId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets this parent Group of this Group.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Group"/> representing the Group's parent group. If this Group does not have a parent, the value will be null.
        /// </value>
        [LavaInclude]
        public virtual Group ParentGroup { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupType"/> that this Group is a member of.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.GroupType"/> that this Group is a member of.
        /// </value>
        [DataMember]
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> that this Group is associated with.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Campus"/> that this Group is associated with.
        /// </value>
        [DataMember]
        public virtual Rock.Model.Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [DataMember]
        public virtual Rock.Model.Schedule Schedule { get; set; }

        /// <summary>
        /// Gets or sets the type of the required signature document.
        /// </summary>
        /// <value>
        /// The type of the required signature document.
        /// </value>
        [DataMember]
        public virtual SignatureDocumentTemplate RequiredSignatureDocumentTemplate { get; set; }

        /// <summary>
        /// Gets or sets the PersonAlias that archived (soft deleted) this group
        /// </summary>
        /// <value>
        /// The archived by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias ArchivedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the group administrator person alias.
        /// </summary>
        /// <value>
        /// The group administrator person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias GroupAdministratorPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets a collection the Groups that are children of this group.
        /// </summary>
        /// <value>
        /// A collection of Groups that are children of this group.
        /// </value>
        [LavaInclude]
        public virtual ICollection<Group> Groups { get; set; } = new Collection<Group>();

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.GroupMember">GroupMembers</see> who are associated with the Group.
        /// Note that this does not include Archived GroupMembers
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.GroupMember">GroupMembers</see> who are associated with the Group.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupMember> Members { get; set; } = new Collection<GroupMember>();

        /// <summary>
        /// Gets or Sets the <see cref="Rock.Model.GroupLocation">GroupLocations</see> that are associated with the Group.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.GroupLocation">GroupLocations</see> that are associated with the Group.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupLocation> GroupLocations { get; set; } = new Collection<GroupLocation>();

        /// <summary>
        /// Gets or sets the group requirements (not including GroupRequirements from the GroupType)
        /// </summary>
        /// <value>
        /// The group requirements.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupRequirement> GroupRequirements { get; set; } = new Collection<GroupRequirement>();

        /// <summary>
        /// Gets or sets the group member workflow triggers.
        /// </summary>
        /// <value>
        /// The group member workflow triggers.
        /// </value>
        [LavaInclude]
        public virtual ICollection<GroupMemberWorkflowTrigger> GroupMemberWorkflowTriggers { get; set; } = new Collection<GroupMemberWorkflowTrigger>();

        /// <summary>
        /// Gets or sets the group syncs.
        /// </summary>
        /// <value>
        /// The group syncs.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupSync> GroupSyncs { get; set; } = new Collection<GroupSync>();

        /// <summary>
        /// Gets or sets the linkages.
        /// </summary>
        /// <value>
        /// The linkages.
        /// </value>
        [LavaInclude]
        public virtual ICollection<EventItemOccurrenceGroupMap> Linkages { get; set; } = new Collection<EventItemOccurrenceGroupMap>();

        /// <summary>
        /// Gets the securable object that security permissions should be inherited from.  If block is located on a page
        /// security will be inherited from the page, otherwise it will be inherited from the site.
        /// </summary>
        /// <value>
        /// The parent authority. If the block is located on the page, security will be
        /// inherited from the page, otherwise it will be inherited from the site.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.ParentGroup != null ? this.ParentGroup : base.ParentAuthority;
            }
        }

        /// <summary>
        /// An optional additional parent authority.  (i.e for Groups, the GroupType is main parent
        /// authority, but parent group is an additional parent authority )
        /// </summary>
        public override Security.ISecured ParentAuthorityPre
        {
            get
            {
                if ( this.GroupTypeId > 0 )
                {
                    GroupTypeCache groupType = GroupTypeCache.Get( this.GroupTypeId );
                    return groupType;
                }
                else
                {
                    return base.ParentAuthorityPre;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether [allows interactive bulk indexing].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allows interactive bulk indexing]; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        [NotMapped]
        public bool AllowsInteractiveBulkIndexing => true;

        /// <summary>
        /// A dictionary of actions that this class supports and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                if ( _supportedActions == null )
                {
                    _supportedActions = new Dictionary<string, string>();
                    _supportedActions.Add( Authorization.VIEW, "The roles and/or users that have access to view." );
                    _supportedActions.Add( Authorization.MANAGE_MEMBERS, "The roles and/or users that have access to manage the group members." );
                    _supportedActions.Add( Authorization.EDIT, "The roles and/or users that have access to edit." );
                    _supportedActions.Add( Authorization.ADMINISTRATE, "The roles and/or users that have access to administrate." );
                    _supportedActions.Add( Authorization.SCHEDULE, "The roles and/or users that may perform scheduling." );
                }

                return _supportedActions;
            }
        }

        private Dictionary<string, string> _supportedActions;

        /// <summary>
        /// Gets or sets the history changes.
        /// </summary>
        /// <value>
        /// The history changes.
        /// </value>
        [NotMapped]
        [RockObsolete( "1.8" )]
        [Obsolete( "Use HistoryChangeList instead" )]
        public virtual List<string> HistoryChanges { get; set; }

        /// <summary>
        /// Gets or sets the history change list.
        /// </summary>
        /// <value>
        /// The history change list.
        /// </value>
        [NotMapped]
        public virtual History.HistoryChangeList HistoryChangeList { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the Group's status. DefinedType depends on this group's <see cref="Rock.Model.GroupType.GroupTypePurposeValue"/>
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object representing the Group's status.
        /// </value>
        [DataMember]
        public virtual DefinedValue StatusValue { get; set; }

        /// <summary>
        /// Gets or sets the PersonAlias of the person to notify when a person cancels
        /// </summary>
        /// <value>
        /// The schedule cancellation person alias.
        /// </value>
        public virtual PersonAlias ScheduleCancellationPersonAlias { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets any group type role limit warnings based on GroupTypeRole restrictions
        /// </summary>
        /// <param name="warningMessageHtml">The warning message HTML.</param>
        /// <returns></returns>
        public bool GetGroupTypeRoleLimitWarnings( out string warningMessageHtml )
        {
            var roleLimitWarnings = new StringBuilder();
            var group = this;
            var groupType = GroupTypeCache.Get( group.GroupTypeId );
            if ( groupType?.Roles != null && groupType.Roles.Any() )
            {
                var groupMemberService = new GroupMemberService( new RockContext() );
                foreach ( var role in groupType.Roles.Where( a => a.MinCount.HasValue || a.MaxCount.HasValue ) )
                {
                    int curCount = groupMemberService.Queryable().Where( m => m.GroupId == group.Id && m.GroupRoleId == role.Id && m.GroupMemberStatus == GroupMemberStatus.Active ).Count();

                    if ( role.MinCount.HasValue && role.MinCount.Value > curCount )
                    {
                        string format = "The <strong>{1}</strong> role is currently below its minimum requirement of {2:N0} active {3}.<br/>";
                        roleLimitWarnings.AppendFormat( format, role.Name.Pluralize().ToLower(), role.Name.ToLower(), role.MinCount, role.MinCount == 1 ? groupType.GroupMemberTerm.ToLower() : groupType.GroupMemberTerm.Pluralize().ToLower() );
                    }

                    if ( role.MaxCount.HasValue && role.MaxCount.Value < curCount )
                    {
                        string format = "The <strong>{1}</strong> role is currently above its maximum limit of {2:N0} active {3}.<br/>";
                        roleLimitWarnings.AppendFormat( format, role.Name.Pluralize().ToLower(), role.Name.ToLower(), role.MaxCount, role.MaxCount == 1 ? groupType.GroupMemberTerm.ToLower() : groupType.GroupMemberTerm.Pluralize().ToLower() );
                    }
                }
            }

            warningMessageHtml = roleLimitWarnings.ToString();
            return !string.IsNullOrEmpty( warningMessageHtml );
        }

        /// <summary>
        /// Gets all the group member workflow triggers from the group and the group type sorted by order
        /// </summary>
        /// <param name="includeGroupTypeTriggers">if set to <c>true</c> [include group type triggers].</param>
        /// <returns></returns>
        public IOrderedEnumerable<GroupMemberWorkflowTrigger> GetGroupMemberWorkflowTriggers( bool includeGroupTypeTriggers = true )
        {
            return this.GroupMemberWorkflowTriggers.Union( this.GroupType.GroupMemberWorkflowTriggers ).OrderBy( a => a.Order ).ThenBy( a => a.Name );
        }

        /// <summary>
        /// Determines whether the specified action is authorized.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsAuthorized( string action, Person person )
        {
            // Check to see if user is authorized using normal authorization rules
            bool authorized = base.IsAuthorized( action, person );

            // If the user is not authorized for group through normal security roles, and this is a logged
            // in user trying to view or edit, check to see if they should be allowed based on their role
            // in the group.
            if ( !authorized && person != null && ( action == Authorization.VIEW || action == Authorization.MANAGE_MEMBERS || action == Authorization.EDIT ) )
            {
                // Get the cached group type
                var groupType = GroupTypeCache.Get( this.GroupTypeId );
                if ( groupType != null )
                {
                    // For each occurrence of this person in this group, check to see if their role is valid
                    // for the group type and if the role grants them authorization
                    using ( var rockContext = new RockContext() )
                    {
                        foreach ( int roleId in new GroupMemberService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( m =>
                                m.PersonId == person.Id &&
                                m.GroupId == this.Id &&
                                m.GroupMemberStatus == GroupMemberStatus.Active )
                            .Select( m => m.GroupRoleId ) )
                        {
                            var role = groupType.Roles.FirstOrDefault( r => r.Id == roleId );
                            if ( role != null )
                            {
                                if ( action == Authorization.VIEW && role.CanView )
                                {
                                    return true;
                                }

                                if ( action == Authorization.MANAGE_MEMBERS && ( role.CanEdit || role.CanManageMembers ) )
                                {
                                    return true;
                                }

                                if ( action == Authorization.EDIT && role.CanEdit )
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return authorized;
        }

        /// <summary>
        /// Gets the group requirements.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IQueryable<GroupRequirement> GetGroupRequirements( RockContext rockContext )
        {
            return new GroupRequirementService( rockContext ).Queryable().Include( a => a.GroupRequirementType ).Where( a => ( a.GroupId.HasValue && a.GroupId == this.Id ) || ( a.GroupTypeId.HasValue && a.GroupTypeId == this.GroupTypeId ) );
        }

        /// <summary>
        /// Persons the meets group requirements.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <returns></returns>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use PersonMeetsGroupRequirements(rockContext, personId, groupRoleId) instead", true )]
        public IEnumerable<PersonGroupRequirementStatus> PersonMeetsGroupRequirements( int personId, int? groupRoleId )
        {
            using ( var rockContext = new RockContext() )
            {
                return this.PersonMeetsGroupRequirements( rockContext, personId, groupRoleId );
            }
        }

        /// <summary>
        /// Persons the meets group requirements.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <returns></returns>
        public IEnumerable<PersonGroupRequirementStatus> PersonMeetsGroupRequirements( RockContext rockContext, int personId, int? groupRoleId )
        {
            var result = new List<PersonGroupRequirementStatus>();
            foreach ( var groupRequirement in this.GetGroupRequirements( rockContext ).OrderBy( a => a.GroupRequirementType.Name ) )
            {
                var requirementStatus = groupRequirement.PersonMeetsGroupRequirement( personId, this.Id, groupRoleId );
                result.Add( requirementStatus );
            }

            return result;
        }

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="entry"></param>
        public override void PreSaveChanges( Data.DbContext dbContext, DbEntityEntry entry )
        {
            var rockContext = ( RockContext ) dbContext;

            HistoryChangeList = new History.HistoryChangeList();

            switch ( entry.State )
            {
                case EntityState.Added:
                    {
                        HistoryChangeList.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Group" ).SetNewValue( Name );

                        History.EvaluateChange( HistoryChangeList, "Name", string.Empty, Name );
                        History.EvaluateChange( HistoryChangeList, "Description", string.Empty, Description );
                        History.EvaluateChange( HistoryChangeList, "Group Type", ( int? ) null, GroupType, GroupTypeId );
                        History.EvaluateChange( HistoryChangeList, "Campus", ( int? ) null, Campus, CampusId );
                        History.EvaluateChange( HistoryChangeList, "Security Role", ( bool? ) null, IsSecurityRole );
                        History.EvaluateChange( HistoryChangeList, "Active", ( bool? ) null, IsActive );
                        History.EvaluateChange( HistoryChangeList, "Allow Guests", ( bool? ) null, AllowGuests );
                        History.EvaluateChange( HistoryChangeList, "Public", ( bool? ) null, IsPublic );
                        History.EvaluateChange( HistoryChangeList, "Group Capacity", ( int? ) null, GroupCapacity );

                        // if this is a new record, but is saved with IsActive=False, set the InactiveDateTime if it isn't set already
                        if ( !this.IsActive )
                        {
                            this.InactiveDateTime = this.InactiveDateTime ?? RockDateTime.Now;
                        }

                        break;
                    }

                case EntityState.Modified:
                    {
                        var originalIsActive = entry.OriginalValues["IsActive"].ToStringSafe().AsBoolean();
                        History.EvaluateChange( HistoryChangeList, "Name", entry.OriginalValues["Name"].ToStringSafe(), Name );
                        History.EvaluateChange( HistoryChangeList, "Description", entry.OriginalValues["Description"].ToStringSafe(), Description );
                        History.EvaluateChange( HistoryChangeList, "Group Type", entry.OriginalValues["GroupTypeId"].ToStringSafe().AsIntegerOrNull(), GroupType, GroupTypeId );
                        History.EvaluateChange( HistoryChangeList, "Campus", entry.OriginalValues["CampusId"].ToStringSafe().AsIntegerOrNull(), Campus, CampusId );
                        History.EvaluateChange( HistoryChangeList, "Security Role", entry.OriginalValues["IsSecurityRole"].ToStringSafe().AsBoolean(), IsSecurityRole );
                        History.EvaluateChange( HistoryChangeList, "Active", originalIsActive, IsActive );
                        History.EvaluateChange( HistoryChangeList, "Allow Guests", entry.OriginalValues["AllowGuests"].ToStringSafe().AsBooleanOrNull(), AllowGuests );
                        History.EvaluateChange( HistoryChangeList, "Public", entry.OriginalValues["IsPublic"].ToStringSafe().AsBoolean(), IsPublic );
                        History.EvaluateChange( HistoryChangeList, "Group Capacity", entry.OriginalValues["GroupCapacity"].ToStringSafe().AsIntegerOrNull(), GroupCapacity );
                        History.EvaluateChange( HistoryChangeList, "Archived", entry.OriginalValues["IsArchived"].ToStringSafe().AsBoolean(), this.IsArchived );

                        // IsActive was modified, set the InactiveDateTime if it changed to Inactive, or set it to NULL if it changed to Active
                        if ( originalIsActive != this.IsActive )
                        {
                            if ( !this.IsActive )
                            {
                                // if the caller didn't already set InactiveDateTime, set it to now
                                this.InactiveDateTime = this.InactiveDateTime ?? RockDateTime.Now;
                            }
                            else
                            {
                                this.InactiveDateTime = null;
                            }
                        }

                        break;
                    }

                case EntityState.Deleted:
                    {
                        HistoryChangeList.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, null );

                        // manually delete any grouprequirements of this group since it can't be cascade deleted
                        var groupRequirementService = new GroupRequirementService( rockContext );
                        var groupRequirements = groupRequirementService.Queryable().Where( a => a.GroupId.HasValue && a.GroupId == this.Id ).ToList();
                        if ( groupRequirements.Any() )
                        {
                            groupRequirementService.DeleteRange( groupRequirements );
                        }

                        // manually set any attendance search group ids to null
                        var attendanceService = new AttendanceService( rockContext );
                        var attendancesToNullSearchResultGroupId = attendanceService.Queryable()
                            .Where( a =>
                                a.SearchResultGroupId.HasValue &&
                                a.SearchResultGroupId.Value == this.Id );

                        dbContext.BulkUpdate( attendancesToNullSearchResultGroupId, a => new Attendance { SearchResultGroupId = null } );

                        // since we can't put a CascadeDelete on both Attendance.Occurrence.GroupId and Attendance.OccurrenceId, manually delete all Attendance records associated with this GroupId
                        var attendancesToDelete = attendanceService.Queryable()
                            .Where( a =>
                                a.Occurrence.GroupId.HasValue &&
                                a.Occurrence.GroupId.Value == this.Id );
                        if ( attendancesToDelete.Any() )
                        {
                            dbContext.BulkDelete( attendancesToDelete );
                        }

                        break;
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
            if ( HistoryChangeList != null && HistoryChangeList.Any() )
            {
                HistoryService.SaveChanges( ( RockContext ) dbContext, typeof( Group ), Rock.SystemGuid.Category.HISTORY_GROUP_CHANGES.AsGuid(), this.Id, HistoryChangeList, this.Name, null, null, true, this.ModifiedByPersonAliasId, dbContext.SourceOfChange );
            }

            base.PostSaveChanges( dbContext );
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValid
        {
            get
            {
                var result = base.IsValid;
                if ( result )
                {
                    string errorMessage;
                    using ( var rockContext = new RockContext() )
                    {
                        // validate that a campus is not required
                        var groupType = this.GroupType ?? new GroupTypeService( rockContext ).Queryable().Where( g => g.Id == this.GroupTypeId ).FirstOrDefault();

                        if ( groupType != null )
                        {
                            if ( groupType.GroupsRequireCampus && this.CampusId == null )
                            {
                                errorMessage = string.Format( "{0} require a campus.", groupType.Name.Pluralize() );
                                ValidationResults.Add( new ValidationResult( errorMessage ) );
                                result = false;
                            }
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Get a list of all inherited Attributes that should be applied to this entity.
        /// </summary>
        /// <returns>A list of all inherited AttributeCache objects.</returns>
        public override List<AttributeCache> GetInheritedAttributes( Rock.Data.RockContext rockContext )
        {
            var groupType = this.GroupType;
            if ( groupType == null && this.GroupTypeId > 0 )
            {
                groupType = new GroupTypeService( rockContext )
                    .Queryable().AsNoTracking()
                    .FirstOrDefault( t => t.Id == this.GroupTypeId );
            }

            if ( groupType != null )
            {
                return groupType.GetInheritedAttributesForQualifier( rockContext, TypeId, "GroupTypeId" );
            }

            return null;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Name of the Group that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the Name of the Group that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region Indexing Methods

        /// <summary>
        /// Bulks the index documents.
        /// </summary>
        public void BulkIndexDocuments()
        {
            List<IndexModelBase> indexableItems = new List<IndexModelBase>();

            RockContext rockContext = new RockContext();

            // return people
            var groups = new GroupService( rockContext ).Queryable().AsNoTracking()
                                .Where( g =>
                                     g.IsActive == true
                                     && g.GroupType.IsIndexEnabled == true );

            int recordCounter = 0;

            foreach ( var group in groups )
            {
                var indexableGroup = GroupIndex.LoadByModel( group );
                indexableItems.Add( indexableGroup );

                recordCounter++;

                if ( recordCounter > 100 )
                {
                    IndexContainer.IndexDocuments( indexableItems );
                    indexableItems = new List<IndexModelBase>();
                    recordCounter = 0;
                }
            }

            IndexContainer.IndexDocuments( indexableItems );
        }

        /// <summary>
        /// Indexes the document.
        /// </summary>
        /// <param name="id"></param>
        public void IndexDocument( int id )
        {
            var groupEntity = new GroupService( new RockContext() ).Get( id );

            // check that this group type is set to be indexed.
            if ( groupEntity.GroupType.IsIndexEnabled && groupEntity.IsActive )
            {
                var indexItem = GroupIndex.LoadByModel( groupEntity );
                IndexContainer.IndexDocument( indexItem );
            }
        }

        /// <summary>
        /// Deletes the indexed document.
        /// </summary>
        /// <param name="id"></param>
        public void DeleteIndexedDocument( int id )
        {
            Type indexType = Type.GetType( "Rock.UniversalSearch.IndexModels.GroupIndex" );
            IndexContainer.DeleteDocumentById( indexType, id );
        }

        /// <summary>
        /// Deletes the indexed documents.
        /// </summary>
        public void DeleteIndexedDocuments()
        {
            IndexContainer.DeleteDocumentsByType<GroupIndex>();
        }

        /// <summary>
        /// Indexes the name of the model.
        /// </summary>
        /// <returns></returns>
        public Type IndexModelType()
        {
            return typeof( GroupIndex );
        }

        /// <summary>
        /// Gets the index filter values.
        /// </summary>
        /// <returns></returns>
        public ModelFieldFilterConfig GetIndexFilterConfig()
        {
            ModelFieldFilterConfig filterConfig = new ModelFieldFilterConfig();
            filterConfig.FilterValues = new GroupTypeService( new RockContext() ).Queryable().AsNoTracking().Where( t => t.IsIndexEnabled ).Select( t => t.Name ).ToList();
            filterConfig.FilterLabel = "Group Types";
            filterConfig.FilterField = "groupTypeName";

            return filterConfig;
        }

        /// <summary>
        /// Gets the index filter field.
        /// </summary>
        /// <returns></returns>
        public bool SupportsIndexFieldFiltering()
        {
            return true;
        }
        #endregion

        #region ICacheable

        private int? _originalGroupTypeId;
        private bool? _originalIsSecurityRole;

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="state">The state.</param>
        public override void PreSaveChanges( Data.DbContext dbContext, DbEntityEntry entry, EntityState state )
        {
            if ( state == EntityState.Modified || state == EntityState.Deleted )
            {
                _originalGroupTypeId = entry.OriginalValues["GroupTypeId"]?.ToString().AsIntegerOrNull();
                _originalIsSecurityRole = entry.OriginalValues["IsSecurityRole"]?.ToString().AsBooleanOrNull();
            }

            base.PreSaveChanges( dbContext, entry, state );
        }

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            // doesn't apply
            return null;
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            // If the group changed, and it was a security group, flush the security for the group
            Guid? originalGroupTypeGuid = null;
            Guid groupTypeScheduleRole = Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid();
            if ( _originalGroupTypeId.HasValue && _originalGroupTypeId != this.GroupTypeId )
            {
                originalGroupTypeGuid = GroupTypeCache.Get( _originalGroupTypeId.Value, ( RockContext ) dbContext )?.Guid;
            }

            var groupTypeGuid = GroupTypeCache.Get( this.GroupTypeId, ( RockContext ) dbContext )?.Guid;
            if ( this.IsSecurityRole || ( _originalIsSecurityRole == true ) || ( groupTypeGuid == groupTypeScheduleRole ) || ( originalGroupTypeGuid == groupTypeScheduleRole ) )
            {
                RoleCache.FlushItem( this.Id );
            }
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Group Configuration class.
    /// </summary>
    public partial class GroupConfiguration : EntityTypeConfiguration<Group>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupConfiguration"/> class.
        /// </summary>
        public GroupConfiguration()
        {
            this.HasOptional( c => c.GroupAdministratorPersonAlias ).WithMany().HasForeignKey( c => c.GroupAdministratorPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ParentGroup ).WithMany( p => p.Groups ).HasForeignKey( p => p.ParentGroupId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.GroupType ).WithMany( p => p.Groups ).HasForeignKey( p => p.GroupTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Campus ).WithMany().HasForeignKey( p => p.CampusId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Schedule ).WithMany().HasForeignKey( p => p.ScheduleId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.RequiredSignatureDocumentTemplate ).WithMany().HasForeignKey( p => p.RequiredSignatureDocumentTemplateId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ArchivedByPersonAlias ).WithMany().HasForeignKey( p => p.ArchivedByPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.StatusValue ).WithMany().HasForeignKey( p => p.StatusValueId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ScheduleCancellationPersonAlias ).WithMany().HasForeignKey( p => p.ScheduleCancellationPersonAliasId ).WillCascadeOnDelete( false );

            // Tell EF that we never want archived groups. 
            // This will prevent archived members from being included in any Groupqueries.
            // It will also prevent navigation properties of Group from including archived groups.
            Z.EntityFramework.Plus.QueryFilterManager.Filter<Group>( x => x.Where( m => m.IsArchived == false ) );

            // In the case of Group as a property (not a collection), we DO want to fetch the group record even if it is archived, so ensure that AllowPropertyFilter = false;
            // NOTE: This is not specific to Group, it is for any Filtered Model (currently just Group and GroupMember)
            Z.EntityFramework.Plus.QueryFilterManager.AllowPropertyFilter = false;
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Controls what selections the person is shown when checking in
    /// </summary>
    public enum AttendanceRecordRequiredForCheckIn
    {
        /// <summary>
        /// All groups are shown
        /// </summary>
        AllShow,

        /// <summary>
        /// Person cannot check into group unless they have been scheduled
        /// </summary>
        RequireAttendanceRecord,

        /// <summary>
        /// The group is preselected if the person is scheduled for this team
        /// </summary>
        UseAttendanceRecordAsPreference
    }

    #endregion

    #region Custom Exceptions

    /// <summary>
    /// Represents a circular reference exception. This occurs when a group is set as a parent of a group that is higher in the group hierarchy. 
    /// </summary>
    /// <remarks>
    ///  An example of this is when a child group is set as the parent of its parent group.
    /// </remarks>
    public class GroupParentCircularReferenceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupParentCircularReferenceException" /> class.
        /// </summary>
        public GroupParentCircularReferenceException()
            : base( "Circular Reference in Group Parents" )
        {
        }
    }

    #endregion
}
