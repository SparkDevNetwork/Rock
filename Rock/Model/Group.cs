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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.SqlServer;
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

        /// <summary>
        /// Gets or sets the inactive reason value identifier.
        /// </summary>
        /// <value>
        /// The inactive reason value identifier.
        /// </value>
        [DataMember]
        [DefinedValue]
        public int? InactiveReasonValueId { get; set; }

        /// <summary>
        /// Gets or sets the inactive reason note.
        /// </summary>
        /// <value>
        /// The inactive reason note.
        /// </value>
        [DataMember]
        public string InactiveReasonNote  { get; set; }

        /// <summary>
        /// Gets or sets the system communication to use for sending an RSVP reminder.
        /// </summary>
        /// <value>
        /// The RSVP reminder system communication object.
        /// </value>
        [DataMember]
        public virtual SystemCommunication RSVPReminderSystemCommunication { get; set; }

        /// <summary>
        /// Gets or sets the system communication to use for sending an RSVP reminder.
        /// </summary>
        /// <value>
        /// The RSVP reminder system communication identifier.
        /// </value>
        [DataMember]
        public int? RSVPReminderSystemCommunicationId { get; set; }

        /// <summary>
        /// Gets or sets the number of days prior to the RSVP date that a reminder should be sent.
        /// </summary>
        /// <value>
        /// The number of days.
        /// </value>
        [DataMember]
        public int? RSVPReminderOffsetDays { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the schedule toolbox access is disabled.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the schedule toolbox access is disabled; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool DisableScheduleToolboxAccess { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if scheduling is disabled.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if scheduling is disabled; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool DisableScheduling { get; set; }

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
        [Obsolete( "Use HistoryChangeList instead", true )]
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

        /// <summary>
        /// Gets or sets the inactive group reason.
        /// </summary>
        /// <value>
        /// The inactive group reason.
        /// </value>
        public virtual DefinedValue InactiveReasonValue { get; set; }

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

            if ( authorized || person == null )
            {
                return authorized;
            }

            var groupType = GroupTypeCache.Get( this.GroupTypeId );

            if ( groupType == null )
            {
                return authorized;
            }

            // if the person isn't authorized through normal security roles, check if the person has a group role that authorizes them
            // First, check if there are any roles that could authorized them. If not, we can avoid a database lookup.
            List<int> checkMemberRoleIds = new List<int>();
            if ( action == Authorization.VIEW )
            {
                checkMemberRoleIds.AddRange( groupType.Roles.Where( a => a.CanView ).Select( a => a.Id ) );
            }
            else if ( action == Authorization.MANAGE_MEMBERS )
            {
                checkMemberRoleIds.AddRange( groupType.Roles.Where( a => a.CanEdit || a.CanManageMembers ).Select( a => a.Id ) );
            }
            else if ( action == Authorization.EDIT )
            {
                checkMemberRoleIds.AddRange( groupType.Roles.Where( a => a.CanEdit ).Select( a => a.Id ) );
            }

            if ( !checkMemberRoleIds.Any() )
            {
                return authorized;
            }

            // For each occurrence of this person in this group for the roles that might grant them auth,
            // check to see if their role is valid for the group type and if the role grants them authorization
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

        private bool _FamilyCampusIsChanged = false;

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="entry"></param>
        public override void PreSaveChanges( Data.DbContext dbContext, DbEntityEntry entry )
        {
            var rockContext = ( RockContext ) dbContext;

            HistoryChangeList = new History.HistoryChangeList();

            _FamilyCampusIsChanged = false;

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
                        DateTime? originalInactiveDateTime = entry.OriginalValues["InactiveDateTime"].ToStringSafe().AsDateTime();

                        var originalIsArchived = entry.OriginalValues["IsArchived"].ToStringSafe().AsBoolean();
                        DateTime? originalArchivedDateTime = entry.OriginalValues["ArchivedDateTime"].ToStringSafe().AsDateTime();

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

                            DateTime? newInactiveDateTime = this.InactiveDateTime;

                            UpdateGroupMembersActiveStatusFromGroupStatus( rockContext, originalIsActive, originalInactiveDateTime, this.IsActive, newInactiveDateTime );
                        }


                        // IsArchived was modified, set the ArchivedDateTime if it changed to IsArchived, or set it to NULL if IsArchived was changed to false
                        if ( originalIsArchived != this.IsArchived )
                        {
                            if ( this.IsArchived )
                            {
                                // if the caller didn't already set ArchivedDateTime, set it to now
                                this.ArchivedDateTime = this.ArchivedDateTime ?? RockDateTime.Now;
                            }
                            else
                            {
                                this.ArchivedDateTime = null;
                            }

                            DateTime? newArchivedDateTime = this.ArchivedDateTime;

                            UpdateGroupMembersArchivedValueFromGroupArchivedValue( rockContext, originalIsArchived, originalArchivedDateTime, this.IsArchived, newArchivedDateTime );
                        }

                        // If Campus is modified for an existing Family Group, set a flag to trigger updates for calculated field Person.PrimaryCampusId.
                        var group = entry.Entity as Group;

                        var familyGroupTypeId = GroupTypeCache.GetFamilyGroupType().Id;

                        _FamilyCampusIsChanged = ( group.GroupTypeId == familyGroupTypeId
                                                   && group.CampusId.GetValueOrDefault( 0 ) != entry.OriginalValues["CampusId"].ToStringSafe().AsInteger() );

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
        /// Updates the group members active status from the group's active status.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="originalIsActive">if set to <c>true</c> [old active status].</param>
        /// <param name="originalInactiveDateTime">The old inactive date time.</param>
        /// <param name="newActiveStatus">if set to <c>true</c> [new active status].</param>
        /// <param name="newInactiveDateTime">The new inactive date time.</param>
        private void UpdateGroupMembersActiveStatusFromGroupStatus( RockContext rockContext, bool originalIsActive, DateTime? originalInactiveDateTime, bool newActiveStatus, DateTime? newInactiveDateTime )
        {
            if ( originalIsActive == newActiveStatus || this.Id == 0 )
            {
                // only change GroupMember status if the Group's status was changed 
                return;
            }

            var groupMemberQuery = new GroupMemberService( rockContext ).Queryable().Where( a => a.GroupId == this.Id );

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
            if ( originalIsArchived == newIsArchived || this.Id == 0 )
            {
                // only change GroupMember archived value if the Group's archived value was changed 
                return;
            }

            // IMPORTANT: When dealing with Archived Groups or GroupMembers, we always need to get
            // a query without the "filter" (AsNoFilter) that comes from the GroupConfiguration and/or
            // GroupMemberConfiguration because otherwise the query will not include archived items.
            var groupMemberQuery = new GroupMemberService( rockContext ).AsNoFilter().Where( a => a.GroupId == this.Id );

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

        /// <summary>
        /// Posts the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( Data.DbContext dbContext )
        {
            var dataContext = ( RockContext ) dbContext;

            if ( HistoryChangeList?.Any() == true )
            {
                HistoryService.SaveChanges( dataContext, typeof( Group ), Rock.SystemGuid.Category.HISTORY_GROUP_CHANGES.AsGuid(), this.Id, HistoryChangeList, this.Name, null, null, true, this.ModifiedByPersonAliasId, dbContext.SourceOfChange );
            }

            if ( _FamilyCampusIsChanged )
            {
                PersonService.UpdatePrimaryFamilyByGroup( this.Id, dataContext );
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
                        var groupType = this.GroupType ?? new GroupTypeService( rockContext ).Queryable().Where( gt => gt.Id == this.GroupTypeId ).FirstOrDefault();

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
            this.HasOptional( p => p.InactiveReasonValue ).WithMany().HasForeignKey( p => p.InactiveReasonValueId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.RSVPReminderSystemCommunication ).WithMany().HasForeignKey( p => p.RSVPReminderSystemCommunicationId ).WillCascadeOnDelete( false );

            // Tell EF that we never want archived groups. 
            // This will prevent archived members from being included in any Group queries.
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
    /// Check-in Requirements for Group Scheduling
    /// </summary>
    public enum AttendanceRecordRequiredForCheckIn
    {
        /// <summary>
        /// Person doesn't need to be scheduled
        /// </summary>
        ScheduleNotRequired = 0,

        /// <summary>
        /// Person doesn't need to be scheduled, but pre-select group if they are scheduled.
        /// </summary>
        [Description( "Pre-select Group if Scheduled" )]
        PreSelect = 1,

        /// <summary>
        /// Person cannot check into group unless they have been scheduled 
        /// </summary>
        ScheduleRequired = 2,
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
