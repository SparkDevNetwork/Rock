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
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Enums.Group;
using Rock.Lava;
using Rock.Security;
using Rock.UniversalSearch;
using Rock.Utility.Enums;
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
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.GROUP )]
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
        /// Gets or sets the <see cref="Rock.Model.Schedule"/> identifier.
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
        /// Indicates this Group is a Security Role even though it isn't a SecurityRole Group Type.
        /// Note: Don't use this alone to determine if a Group is a security role group. Use <see cref="IsSecurityRoleOrSecurityGroupType()"/> to see if a Group is for a Security Role.
        /// </summary>
        /// <value>
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Previewable]
        public bool IsSecurityRole { get; set; }

        /// <summary>
        /// Gets or sets the elevated security level. This setting is used to determine the group member's Account Protection Profile.
        /// </summary>
        /// <value>
        /// The elevated security level.
        /// </value>
        [DataMember()]
        public ElevatedSecurityLevel ElevatedSecurityLevel { get; set; }

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
        /// Gets or sets the <see cref="Rock.Model.PersonAlias">PersonAliasId</see> that archived (soft deleted) this group
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
        /// Gets or sets the <see cref="Rock.Model.PersonAlias">PersonAliasId</see> of the person to notify when a person cancels
        /// </summary>
        /// <value>
        /// The schedule cancellation person alias identifier.
        /// </value>
        [Obsolete( "Use ScheduleCoordinatorPersonAliasId instead." )]
        [RockObsolete( "1.16" )]
        [DataMember]
        [NotMapped]
        public int? ScheduleCancellationPersonAliasId
        {
            get => this.ScheduleCoordinatorPersonAliasId;
            set => this.ScheduleCoordinatorPersonAliasId = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias">PersonAliasId</see> of the person who receives
        /// notifications about changes to scheduled individuals.
        /// </summary>
        /// <value>
        /// The schedule coordinator person alias identifier.
        /// </value>
        /// <remarks>
        /// The notification types specified in this group's <see cref="GroupType.ScheduleCoordinatorNotificationTypes"/>
        /// (or overridden in this group's own <see cref="ScheduleCoordinatorNotificationTypes"/>) will determine which
        /// notifications - if any - are sent to this person.
        /// </remarks>
        [DataMember]
        public int? ScheduleCoordinatorPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the types of notifications the coordinator receives about scheduled individuals.
        /// </summary>
        /// <value>
        /// The schedule coordinator notification types.
        /// </value>
        [DataMember]
        public ScheduleCoordinatorNotificationType? ScheduleCoordinatorNotificationTypes { get; set; }

        /// <summary>
        /// Gets or sets the group administrator <see cref="Rock.Model.PersonAlias"/> identifier.
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
        [DefinedValue( SystemGuid.DefinedType.GROUPTYPE_INACTIVE_REASONS )]
        public int? InactiveReasonValueId { get; set; }

        /// <summary>
        /// Gets or sets the inactive reason note.
        /// </summary>
        /// <value>
        /// The inactive reason note.
        /// </value>
        [DataMember]
        public string InactiveReasonNote { get; set; }

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

        /// <summary>
        /// List leaders names, in order by males → females.
        /// Examples: Ted &#38; Cindy Decker -or- Ted Decker &#38; Cindy Wright.
        /// This is populated from the logic in <seealso cref="Person.GetFamilySalutation(Person, bool, bool, bool, string, string)"/>
        /// with includeChildren=false, and useFormalNames=false.
        /// </summary>
        /// <value>
        /// The group salutation.
        /// </value>
        [DataMember]
        [MaxLength( 250 )]
        public string GroupSalutation { get; set; }

        /// <summary>
        /// List all active group members, or order by leaders males → females - non leaders by age.
        /// Examples: Ted, Cindy, Noah and Alex Decker.
        /// This is populated from the logic in <seealso cref="Person.GetFamilySalutation(Person, bool, bool, bool, string, string)"/>
        /// with includeChildren=true, and useFormalNames=false.
        /// </summary>
        /// <value>
        /// The group salutation.
        /// </value>
        [DataMember]
        [MaxLength( 250 )]
        public string GroupSalutationFull { get; set; }

        /// <summary>
        /// Gets or sets the confirmation additional details.
        /// </summary>
        /// <value>
        /// The confirmation additional details.
        /// </value>
        [DataMember]
        public string ConfirmationAdditionalDetails { get; set; }

        /// <summary>
        /// Gets or sets the system communication to use for sending a reminder.
        /// </summary>
        /// <value>
        /// The reminder system communication identifier.
        /// </value>
        [DataMember]
        public int? ReminderSystemCommunicationId { get; set; }

        /// <summary>
        /// Gets or sets the number of days prior to an event date that a reminder should be sent.
        /// </summary>
        /// <value>
        /// The number of days.
        /// </value>
        [DataMember]
        public int? ReminderOffsetDays { get; set; }


        /// <summary>
        /// Gets or sets the reminder additional details.
        /// </summary>
        /// <value>
        /// The reminder additional details.
        /// </value>
        [DataMember]
        public string ReminderAdditionalDetails { get; set; }

        /// <summary>
        /// Gets or sets the schedule confirmation logic.
        /// </summary>
        /// <value>
        /// The schedule confirmation logic.
        /// </value>
        [DataMember]
        public ScheduleConfirmationLogic? ScheduleConfirmationLogic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether relationship growth is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [relationship growth enabled]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool? RelationshipGrowthEnabledOverride { get; set; }

        /// <summary>
        /// Gets or sets the relationship strength.
        /// </summary>
        /// <value>
        /// The relationship strength.
        /// </value>
        [DataMember]
        public int? RelationshipStrengthOverride { get; set; }

        /// <summary>
        /// Gets or sets the leader to leader relationship multiplier.
        /// </summary>
        /// <value>
        /// The leader to leader relationship multiplier.
        /// </value>
        [DataMember]
        [DecimalPrecision( 8, 2 )]
        public decimal? LeaderToLeaderRelationshipMultiplierOverride { get; set; }

        /// <summary>
        /// Gets or sets the leader to non leader relationship multiplier.
        /// </summary>
        /// <value>
        /// The leader to non leader relationship multiplier.
        /// </value>
        [DataMember]
        [DecimalPrecision( 8, 2 )]
        public decimal? LeaderToNonLeaderRelationshipMultiplierOverride { get; set; }

        /// <summary>
        /// Gets or sets the non leader to non leader relationship multiplier.
        /// </summary>
        /// <value>
        /// The non leader to non leader relationship multiplier.
        /// </value>
        [DataMember]
        [DecimalPrecision( 8, 2 )]
        public decimal? NonLeaderToNonLeaderRelationshipMultiplierOverride { get; set; }

        /// <summary>
        /// Gets or sets the non leader to leader relationship multiplier.
        /// </summary>
        /// <value>
        /// The non leader to leader relationship multiplier.
        /// </value>
        [DataMember]
        [DecimalPrecision( 8, 2 )]
        public decimal? NonLeaderToLeaderRelationshipMultiplierOverride { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets a value that indicates if this group is a special needs
        /// group.
        /// </para>
        /// <para>
        /// For a check-in group, this indicates that the group is intended for
        /// people with special needs. It can be used in other contexts to have
        /// different meaning for "special needs".
        /// </para>
        /// </summary>
        /// <value>
        /// A value that indicates if this gorup is a special needs group.
        /// </value>
        [DataMember]
        public bool IsSpecialNeeds { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the system communication to use for sending an RSVP reminder.
        /// </summary>
        /// <value>
        /// The RSVP reminder system communication object.
        /// </value>
        [DataMember]
        public virtual SystemCommunication RSVPReminderSystemCommunication { get; set; }

        /// <summary>
        /// Gets or sets this parent Group of this Group.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Group"/> representing the Group's parent group. If this Group does not have a parent, the value will be null.
        /// </value>
        [LavaVisible]
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
        /// Gets or sets the <see cref="Rock.Model.Schedule"/>.
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
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> that archived (soft deleted) this group
        /// </summary>
        /// <value>
        /// The archived by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias ArchivedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the group administrator <see cref="Rock.Model.PersonAlias"/>.
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
        [LavaVisible]
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
        /// Gets or sets the <see cref="Rock.Model.GroupMemberWorkflowTrigger">Group Member Workflow Triggers</see>.
        /// </summary>
        /// <value>
        /// The group member workflow triggers.
        /// </value>
        [LavaVisible]
        public virtual ICollection<GroupMemberWorkflowTrigger> GroupMemberWorkflowTriggers { get; set; } = new Collection<GroupMemberWorkflowTrigger>();

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupSync">group syncs</see>.
        /// </summary>
        /// <value>
        /// The group syncs.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupSync> GroupSyncs { get; set; } = new Collection<GroupSync>();

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EventItemOccurrenceGroupMap">linkages</see>.
        /// </summary>
        /// <value>
        /// The linkages.
        /// </value>
        [LavaVisible]
        public virtual ICollection<EventItemOccurrenceGroupMap> Linkages { get; set; } = new Collection<EventItemOccurrenceGroupMap>();

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the Group's status. DefinedType depends on this group's <see cref="Rock.Model.GroupType.GroupTypePurposeValue"/>
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object representing the Group's status.
        /// </value>
        [DataMember]
        public virtual DefinedValue StatusValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> of the person to notify when a person cancels
        /// </summary>
        /// <value>
        /// The schedule cancellation person alias.
        /// </value>
        [Obsolete( "Use ScheduleCoordinatorPersonAlias instead." )]
        [RockObsolete( "1.16" )]
        [NotMapped]
        public virtual PersonAlias ScheduleCancellationPersonAlias
        {
            get => this.ScheduleCoordinatorPersonAlias;
            set => this.ScheduleCoordinatorPersonAlias = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> of the person who receives notifications about changes
        /// to scheduled individuals.
        /// </summary>
        /// <value>
        /// The schedule coordinator person alias.
        /// </value>
        /// <remarks>
        /// The notification types specified in this group's <see cref="GroupType.ScheduleCoordinatorNotificationTypes"/>
        /// (or overridden in this group's own <see cref="ScheduleCoordinatorNotificationTypes"/>) will determine which
        /// notifications - if any - are sent to this person.
        /// </remarks>
        public virtual PersonAlias ScheduleCoordinatorPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the inactive group reason.
        /// </summary>
        /// <value>
        /// The inactive group reason.
        /// </value>
        public virtual DefinedValue InactiveReasonValue { get; set; }

        /// <summary>
        /// Provides a <see cref="Dictionary{TKey, TValue}"/> of actions that this model supports, and the description of each.
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

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> containing the Location's Name that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> containing the Location's Name that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion Public Methods
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
            this.HasOptional( p => p.ScheduleCoordinatorPersonAlias ).WithMany().HasForeignKey( p => p.ScheduleCoordinatorPersonAliasId ).WillCascadeOnDelete( false );
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
}
