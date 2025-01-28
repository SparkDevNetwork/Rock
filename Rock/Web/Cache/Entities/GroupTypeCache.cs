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
using System.Runtime.Serialization;

using Rock.CheckIn.v2;
using Rock.Data;
using Rock.Enums.CheckIn;
using Rock.Enums.Group;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a groupType. This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class GroupTypeCache : ModelCache<GroupTypeCache, GroupType>
    {
        private TemplateConfigurationData _checkInConfiguration;

        private AreaConfigurationData _checkInAreaData;

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the group term.
        /// </summary>
        /// <value>
        /// The group term.
        /// </value>
        [DataMember]
        public string GroupTerm { get; private set; }

        /// <summary>
        /// Gets or sets the group member term.
        /// </summary>
        /// <value>
        /// The group member term.
        /// </value>
        [DataMember]
        public string GroupMemberTerm { get; private set; }

        /// <summary>
        /// Gets or sets the default group role identifier.
        /// </summary>
        /// <value>
        /// The default group role identifier.
        /// </value>
        [DataMember]
        public int? DefaultGroupRoleId { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow multiple locations].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allow multiple locations]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowMultipleLocations { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show in group list].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in group list]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowInGroupList { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show in navigation].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in navigation]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowInNavigation { get; private set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        public string IconCssClass { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [takes attendance].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [takes attendance]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool TakesAttendance { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [attendance counts as weekend service].
        /// </summary>
        /// <value>
        /// <c>true</c> if [attendance counts as weekend service]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AttendanceCountsAsWeekendService { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [send attendance reminder].
        /// </summary>
        /// <value>
        /// <c>true</c> if [send attendance reminder]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool SendAttendanceReminder { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show connection status].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show connection status]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowConnectionStatus { get; private set; }

        /// <summary>
        /// Gets or sets the attendance rule.
        /// </summary>
        /// <value>
        /// The attendance rule.
        /// </value>
        [DataMember]
        public AttendanceRule AttendanceRule { get; private set; }

        /// <inheritdoc cref="GroupType.AlreadyEnrolledMatchingLogic"/>
        [DataMember]
        public AlreadyEnrolledMatchingLogic AlreadyEnrolledMatchingLogic { get; private set; }

        /// <summary>
        /// Gets or sets the group capacity rule.
        /// </summary>
        /// <value>
        /// The group capacity rule.
        /// </value>
        [DataMember]
        public GroupCapacityRule GroupCapacityRule { get; private set; }

        /// <summary>
        /// Gets or sets the attendance print to.
        /// </summary>
        /// <value>
        /// The attendance print to.
        /// </value>
        [DataMember]
        public PrintTo AttendancePrintTo { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets or sets the inherited group type identifier.
        /// </summary>
        /// <value>
        /// The inherited group type identifier.
        /// </value>
        [DataMember]
        public int? InheritedGroupTypeId { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is indexable.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is indexable; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsIndexEnabled { get; private set; }

        /// <summary>
        /// Gets or sets a flag indicating if specific groups are allowed to have their own member attributes.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this specific group are allowed to have their own member attributes, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowSpecificGroupMemberAttributes { get; private set; }

        /// <summary>
        /// Gets or sets a flag indicating if group requirements section is enabled for group of this type.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if group requirements section is enabled for group of this type, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableSpecificGroupRequirements { get; private set; }

        /// <summary>
        /// Gets or sets a flag indicating if groups of this type are allowed to be sync'ed.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if groups of this type are allowed to be sync'ed, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowGroupSync { get; private set; }

        /// <summary>
        /// Gets or sets a flag indicating if groups of this type should be allowed to have Group Member Workflows.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if groups of this type should be allowed to have group member workflows, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowSpecificGroupMemberWorkflows { get; private set; }

        /// <summary>
        /// Gets the type of the inherited group.
        /// </summary>
        /// <value>
        /// The type of the inherited group.
        /// </value>
        public GroupTypeCache InheritedGroupType
        {
            get
            {
                if ( InheritedGroupTypeId.HasValue && InheritedGroupTypeId.Value != 0 )
                {
                    return Get( InheritedGroupTypeId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the allowed schedule types.
        /// </summary>
        /// <value>
        /// The allowed schedule types.
        /// </value>
        [DataMember]
        public ScheduleType AllowedScheduleTypes { get; private set; }


        /// <summary>
        /// Gets or sets the location selection mode.
        /// </summary>
        /// <value>
        /// The location selection mode.
        /// </value>
        [DataMember]
        public GroupLocationPickerMode LocationSelectionMode { get; private set; }

        /// <summary>
        /// Gets or sets the enable location schedules.
        /// </summary>
        /// <value>
        /// The enable location schedules.
        /// </value>
        [DataMember]
        public bool? EnableLocationSchedules { get; private set; }

        /// <summary>
        /// Gets or sets the group type purpose value identifier.
        /// </summary>
        /// <value>
        /// The group type purpose value identifier.
        /// </value>
        [DataMember]
        public int? GroupTypePurposeValueId { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is capacity required.
        /// </summary>
        /// <value><c>true</c> if this instance is capacity required; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsCapacityRequired { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether [groups require campus].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [groups require campus]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool GroupsRequireCampus { get; private set; }

        /// <summary>
        /// Gets the group type purpose value.
        /// </summary>
        /// <value>
        /// The group type purpose value.
        /// </value>
        public DefinedValueCache GroupTypePurposeValue => GroupTypePurposeValueId.HasValue ? DefinedValueCache.Get( GroupTypePurposeValueId.Value ) : null;

        /// <summary>
        /// If this GroupType is a Checkin Area, returns the Check-in Configuration (Weekly Service Check-in, Volunteer Check-in, etc) associated with this GroupType
        /// </summary>
        /// <returns></returns>
        public GroupTypeCache GetCheckInConfigurationType()
        {
            return GetParentWithGroupTypePurpose( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() );
        }

        /// <summary>
        /// If this GroupType is a Checkin Area, returns the specified attribute value for the Check-in Configuration (Weekly Service Check-in, Volunteer Check-in, etc) associated with this GroupType
        /// </summary>
        /// <returns></returns>
        public string GetCheckInConfigurationAttributeValue( string attributeKey )
        {
            return GetCheckInConfigurationType()?.GetAttributeValue( attributeKey );
        }

        /// <summary>
        /// Gets the parent with group type purpose.
        /// </summary>
        /// <param name="purposeGuid">The purpose unique identifier.</param>
        /// <returns></returns>
        private GroupTypeCache GetParentWithGroupTypePurpose( Guid purposeGuid )
        {
            return GetParentPurposeGroupType( this, purposeGuid );
        }

        /// <summary>
        /// Gets the type of the parent purpose group.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="purposeGuid">The purpose unique identifier.</param>
        /// <returns></returns>
        private GroupTypeCache GetParentPurposeGroupType( GroupTypeCache groupType, Guid purposeGuid )
        {
            return GetParentPurposeGroupType( groupType, purposeGuid, groupType );
        }

        /// <summary>
        /// Gets the type of the parent purpose group.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="purposeGuid">The purpose unique identifier.</param>
        /// <param name="startingGroup">Starting group is used to avoid circular references.</param>
        /// <param name="processedGroupTypeIds">A collection of unique identifiers representing specific group types already processed by the method. This parameter filters the operation to include only the specified group types.</param>
        /// <returns></returns>
        private GroupTypeCache GetParentPurposeGroupType( GroupTypeCache groupType, Guid purposeGuid, GroupTypeCache startingGroup, List<int> processedGroupTypeIds = null )
        {
            processedGroupTypeIds = processedGroupTypeIds ?? new List<int>();

            if ( groupType != null &&
                groupType.GroupTypePurposeValue != null &&
                groupType.GroupTypePurposeValue.Guid.Equals( purposeGuid ) )
            {
                return groupType;
            }

            foreach ( var parentGroupType in groupType.ParentGroupTypes )
            {
                // skip if parent group type and current group type are the same (a situation that should not be possible) to prevent stack overflow
                if ( groupType.Id == parentGroupType.Id ||
                     // also skip if the parent group type and starting group type are the same as this is a circular reference and can cause a stack overflow
                     startingGroup.Id == parentGroupType.Id ||
                     processedGroupTypeIds.Contains( parentGroupType.Id ) )
                {
                    continue;
                }

                processedGroupTypeIds.Add( parentGroupType.Id );

                var testGroupType = GetParentPurposeGroupType( parentGroupType, purposeGuid, startingGroup, processedGroupTypeIds );
                if ( testGroupType != null )
                {
                    return testGroupType;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore person inactivated.
        /// By default group members are inactivated in their group whenever the person
        /// is inactivated. If this value is set to true, members in groups of this type
        /// will not be marked inactive when the person is inactivated
        /// </summary>
        /// <value>
        /// <c>true</c> if [ignore person inactivated]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IgnorePersonInactivated { get; private set; }

        /// <summary>
        /// Gets or sets a lava template that can be used for generating  view details for Group.
        /// </summary>
        /// <value>
        /// The Group View Lava Template.
        /// </value>
        [DataMember]
        public string GroupViewLavaTemplate { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether group history should be enabled for groups of this type
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable group history]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableGroupHistory { get; private set; }

        /// <summary>
        /// Gets or sets the DefinedType that Groups of this type will use for the Group.StatusValue
        /// </summary>
        /// <value>
        /// The group status defined type identifier.
        /// </value>
        [DataMember]
        public int? GroupStatusDefinedTypeId { get; private set; }

        /// <summary>
        /// The color used to visually distinguish groups on lists.
        /// </summary>
        /// <value>
        /// The group type color.
        /// </value>
        [DataMember]
        public string GroupTypeColor { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show marital status].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show marital status]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowMaritalStatus { get; private set; }


        /// <summary>
        /// Indicates whether RSVP functionality should be enabled for this group.
        /// </summary>
        /// <value>
        /// A boolean value.
        /// </value>
        [DataMember]
        public bool EnableRSVP { get; private set; }

        /// <summary>
        /// Gets or sets the system communication to use for sending an RSVP reminder.
        /// </summary>
        /// <value>
        /// The RSVP reminder system communication identifier.
        /// </value>
        [DataMember]
        public int? RSVPReminderSystemCommunicationId { get; private set; }

        /// <summary>
        /// Gets or sets the number of days prior to the RSVP date that a reminder should be sent.
        /// </summary>
        /// <value>
        /// The number of days.
        /// </value>
        [DataMember]
        public int? RSVPReminderOffsetDays { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether scheduling is enabled for groups of this type
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is scheduling enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSchedulingEnabled { get; private set; }

        /// <summary>
        /// Gets or sets the communication template to use when a person is scheduled or when the schedule has been updated
        /// </summary>
        /// <value>
        /// The scheduled communication template identifier.
        /// </value>
        [DataMember]
        [Obsolete( "Use ScheduleConfirmationSystemCommunicationId instead.", true )]
        [RockObsolete( "1.10" )]
        public int? ScheduleConfirmationSystemEmailId { get; private set; }

        /// <summary>
        /// Gets or sets the communication template to use when a person is scheduled or when the schedule has been updated
        /// </summary>
        /// <value>
        /// The scheduled communication template identifier.
        /// </value>
        [DataMember]
        public int? ScheduleConfirmationSystemCommunicationId { get; private set; }

        /// <summary>
        /// Gets or sets the communication template to use when sending a schedule reminder
        /// </summary>
        /// <value>
        /// The schedule reminder communication template identifier.
        /// </value>
        [DataMember]
        [Obsolete( "Use ScheduleReminderSystemCommunicationId instead.", true )]
        [RockObsolete( "1.10" )]
        public int? ScheduleReminderSystemEmailId { get; private set; }

        /// <summary>
        /// Gets or sets the communication template to use when sending a schedule reminder
        /// </summary>
        /// <value>
        /// The schedule reminder communication template identifier.
        /// </value>
        [DataMember]
        public int? ScheduleReminderSystemCommunicationId { get; private set; }

        /// <summary>
        /// Gets or sets the WorkflowType to execute when a person indicates they won't be able to attend at their scheduled time
        /// </summary>
        /// <value>
        /// The schedule cancellation workflow type identifier.
        /// </value>
        [DataMember]
        public int? ScheduleCancellationWorkflowTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the number of days prior to the schedule to send a confirmation email.
        /// </summary>
        /// <value>
        /// The schedule confirmation email offset days.
        /// </value>
        [DataMember]
        public int? ScheduleConfirmationEmailOffsetDays { get; private set; }

        /// <summary>
        /// Gets or sets the number of days prior to the schedule to send a reminder email. See also <seealso cref="GroupMember.ScheduleReminderEmailOffsetDays"/>.
        /// </summary>
        /// <value>
        /// The schedule reminder email offset days.
        /// </value>
        [DataMember]
        public int? ScheduleReminderEmailOffsetDays { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether a person must specify a reason when declining/cancelling.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [requires reason if decline schedule]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool RequiresReasonIfDeclineSchedule { get; private set; }

        /// <summary>
        /// Gets or sets the administrator term for the group of this GroupType.
        /// </summary>
        /// <value>
        /// The administrator term for the group of this GroupType.
        /// </value>
        [DataMember]
        public string AdministratorTerm { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether administrator for the group of this GroupType will be shown.
        /// </summary>
        /// <value>
        ///   A <see cref="System.Boolean"/> value that is <c>true</c> if administrator for the group of this GroupType will be shown; otherwise <c>false</c>.
        /// </value>
        [DataMember( IsRequired = true )]
        public bool ShowAdministrator { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether group tag should be enabled for groups of this type
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable group tag]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableGroupTag { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [allow any child group type].
        /// Use this along with <seealso cref="ChildGroupTypes"/> to determine if a child group can have a parent group of this group type
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow any child group type]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowAnyChildGroupType { get; private set; }

        /// <summary>
        /// Gets or sets the schedule confirmation logic.
        /// </summary>
        /// <value>
        /// The schedule confirmation logic.
        /// </value>
        [DataMember]
        public ScheduleConfirmationLogic ScheduleConfirmationLogic { get; set; }

        /// <summary>
        /// Gets a value that groups in this area should not be available
        /// when a person already has a check-in for the same schedule.
        /// </summary>
        [DataMember]
        public bool IsConcurrentCheckInPrevented { get; private set; }

        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        /// <value>
        /// The roles.
        /// </value>
        [DataMember]
        public List<GroupTypeRoleCache> Roles
        {
            get
            {
                if ( _roles == null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var roleIds = new GroupTypeRoleService( rockContext )
                            .Queryable()
                            .Where( r => r.GroupTypeId == Id )
                            .Select( r => r.Id )
                            .ToList();

                        // GroupTypeRole invalidates the GroupTypeCache object
                        // when it is changed, so it is safe to cache the full
                        // GroupTypeRoleCache objects instead of just the Ids.
                        _roles = GroupTypeRoleCache.GetMany( roleIds, rockContext )
                            .OrderBy( r => r.Order )
                            .ToList();
                    }
                }

                return _roles;
            }
        }

        private List<GroupTypeRoleCache> _roles = null;

        /// <summary>
        /// Gets or sets the group schedule exclusions.
        /// </summary>
        /// <value>
        /// The group schedule exclusions.
        /// </value>
        [DataMember]
        public List<DateRange> GroupScheduleExclusions
        {
            get
            {
                if ( _groupScheduleExclusions == null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        _groupScheduleExclusions = new GroupScheduleExclusionService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( s => s.GroupTypeId == Id )
                            .OrderBy( s => s.StartDate )
                            .ToList()
                            .Select( s => new DateRange( s.StartDate, s.EndDate ) )
                            .ToList();
                    }
                }

                return _groupScheduleExclusions;
            }
        }

        private List<DateRange> _groupScheduleExclusions;

        /// <summary>
        /// Gets the group types that are allowed for child groups.
        /// Use this along with <seealso cref="AllowAnyChildGroupType"/> to
        /// determine if a child group can have a parent group of this group type
        /// </summary>
        /// <value>
        /// The child group types.
        /// </value>
        public List<GroupTypeCache> ChildGroupTypes
        {
            /* 2020-09-02 MDP
             * ChildGroupTypes are used in two different ways, see engineering notes for ChildGroupTypes in Rock.Model.GroupType 
             */

            get
            {
                var childGroupTypes = new List<GroupTypeCache>();

                foreach ( var id in ChildGroupTypeIds )
                {
                    var groupType = Get( id );
                    if ( groupType != null )
                    {
                        childGroupTypes.Add( groupType );
                    }
                }

                return childGroupTypes;
            }
        }

        /// <summary>
        /// Gets the group type identifiers that are allowed for child groups.
        /// Use this along with <seealso cref="AllowAnyChildGroupType"/> to
        /// determine if a child group can have a parent group of this group type
        /// </summary>
        /// <value>
        /// The child group type identifiers.
        /// </value>
        [DataMember]
        private List<int> ChildGroupTypeIds
        {
            get
            {
                if ( _childGroupTypeIds == null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        _childGroupTypeIds = new GroupTypeService( rockContext )
                            .GetChildGroupTypes( Id )
                            .Select( g => g.Id )
                            .ToList();
                    }
                }

                return _childGroupTypeIds;
            }
        }
        private List<int> _childGroupTypeIds;

        /// <summary>
        /// Gets the parent group types.
        /// </summary>
        /// <value>
        /// The parent group types.
        /// </value>
        public List<GroupTypeCache> ParentGroupTypes
        {
            get
            {
                var parentGroupTypes = new List<GroupTypeCache>();

                foreach ( var id in ParentGroupTypeIds )
                {
                    var groupType = Get( id );
                    if ( groupType != null )
                    {
                        parentGroupTypes.Add( groupType );
                    }
                }

                return parentGroupTypes;
            }
        }

        /// <summary>
        /// Gets the parent group type identifiers.
        /// </summary>
        /// <value>
        /// The parent group type identifiers.
        /// </value>
        private List<int> ParentGroupTypeIds
        {
            get
            {
                if ( _parentGroupTypeIds == null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        _parentGroupTypeIds = new GroupTypeService( rockContext )
                            .GetParentGroupTypes( Id )
                            .Select( g => g.Id )
                            .ToList();
                    }
                }

                return _parentGroupTypeIds;
            }
        }
        private List<int> _parentGroupTypeIds;

        /// <summary>
        /// Gets or sets the location type value i ds.
        /// </summary>
        /// <value>
        /// The location type value i ds.
        /// </value>
        [DataMember]
        public List<int> LocationTypeValueIDs { get; private set; }

        /// <summary>
        /// Gets the location type values.
        /// </summary>
        /// <value>
        /// The location type values.
        /// </value>
        public List<DefinedValueCache> LocationTypeValues
        {
            get
            {
                var locationTypeValues = new List<DefinedValueCache>();
                if ( LocationTypeValueIDs == null )
                    return null;

                foreach ( var id in LocationTypeValueIDs.ToList() )
                {
                    locationTypeValues.Add( DefinedValueCache.Get( id ) );
                }

                return locationTypeValues;

            }
        }

        /// <summary>
        /// Gets or sets the DefinedType that Groups of this type will use for the Group.StatusValue
        /// </summary>
        /// <value>
        /// The type of the group status defined.
        /// </value>
        public DefinedTypeCache GroupStatusDefinedType => GroupStatusDefinedTypeId.HasValue ? DefinedTypeCache.Get( this.GroupStatusDefinedTypeId.Value ) : null;

        /// <inheritdoc cref="Rock.Model.Group.ScheduleCoordinatorNotificationTypes" />
        [DataMember]
        public ScheduleCoordinatorNotificationType? ScheduleCoordinatorNotificationTypes { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets a list of all attributes defined for the GroupTypes specified that
        /// match the entityTypeQualifierColumn and the GroupType Ids.
        /// </summary>
        /// <param name="entityTypeId">The Entity Type Id for which Attributes to load.</param>
        /// <param name="entityTypeQualifierColumn">The EntityTypeQualifierColumn value to match against.</param>
        /// <returns>A list of attributes defined in the inheritance tree.</returns>
        internal List<AttributeCache> GetInheritedAttributesForQualifier( int entityTypeId, string entityTypeQualifierColumn )
        {
            var groupTypeIds = GetInheritedGroupTypeIds();

            var inheritedAttributes = new Dictionary<int, List<AttributeCache>>();
            groupTypeIds.ForEach( g => inheritedAttributes.Add( g, new List<AttributeCache>() ) );

            //
            // Walk each group type and generate a list of matching attributes.
            //
            foreach ( var entityTypeAttribute in AttributeCache.GetByEntityType( entityTypeId ) )
            {
                // group type ids exist and qualifier is for a group type id
                if ( string.Compare( entityTypeAttribute.EntityTypeQualifierColumn, entityTypeQualifierColumn, true ) == 0 )
                {
                    int groupTypeIdValue = int.MinValue;
                    if ( int.TryParse( entityTypeAttribute.EntityTypeQualifierValue, out groupTypeIdValue ) && groupTypeIds.Contains( groupTypeIdValue ) )
                    {
                        inheritedAttributes[groupTypeIdValue].Add( entityTypeAttribute );
                    }
                }
            }

            //
            // Walk the generated list of attribute groups and put them, ordered, into a list
            // of inherited attributes.
            //
            var attributes = new List<AttributeCache>();
            foreach ( var attributeGroup in inheritedAttributes )
            {
                foreach ( var attribute in attributeGroup.Value.OrderBy( a => a.Order ) )
                {
                    attributes.Add( attribute );
                }
            }

            return attributes;
        }

        /// <summary>
        /// Gets a list of GroupType Ids, including our own Id, that identifies the
        /// inheritance tree.
        /// </summary>
        /// <returns>A list of GroupType Ids, including our own Id, that identifies the inheritance tree.</returns>
        internal List<int> GetInheritedGroupTypeIds()
        {
            //
            // Can't use GroupTypeCache here since it loads attributes and could
            // result in a recursive stack overflow situation when we are called
            // from a GetInheritedAttributes() method.
            //
            var groupTypeIds = new List<int>();
            var groupType = this;

            //
            // Loop until we find a recursive loop or run out of parent group types.
            //
            while ( groupType != null && !groupTypeIds.Contains( groupType.Id ) )
            {
                groupTypeIds.Insert( 0, groupType.Id );

                groupType = groupType.InheritedGroupType;
            }

            return groupTypeIds;
        }

        /// <summary>
        /// Gets the check-in configuration that represents all the attribute
        /// values of this group type. If this group type is not a check-in
        /// configuration group type then <c>null</c> will be returned.
        /// </summary>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <returns>An instance of <see cref="TemplateConfigurationData"/> or <c>null</c>.</returns>
        internal TemplateConfigurationData GetCheckInConfiguration( RockContext rockContext )
        {
            if ( rockContext == null )
            {
                throw new ArgumentNullException( nameof( rockContext ) );
            }

            if ( _checkInConfiguration == null )
            {
                var checkinTemplateTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid(), rockContext )?.Id;

                if ( GroupTypePurposeValueId != checkinTemplateTypeId )
                {
                    return null;
                }

                _checkInConfiguration = new TemplateConfigurationData( this, rockContext );
            }

            return _checkInConfiguration;
        }

        /// <summary>
        /// Gets the check-in data that represents all the attribute
        /// values of this area group type.
        /// </summary>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <returns>An instance of <see cref="AreaConfigurationData"/>.</returns>
        internal AreaConfigurationData GetCheckInAreaData( RockContext rockContext )
        {
            if ( rockContext == null )
            {
                throw new ArgumentNullException( nameof( rockContext ) );
            }

            if ( _checkInAreaData == null )
            {
                _checkInAreaData = new AreaConfigurationData( this, rockContext );
            }

            return _checkInAreaData;
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var groupType = entity as GroupType;
            if ( groupType == null )
                return;

            IsSystem = groupType.IsSystem;
            Name = groupType.Name;
            Description = groupType.Description;
            GroupTerm = groupType.GroupTerm;
            GroupMemberTerm = groupType.GroupMemberTerm;
            DefaultGroupRoleId = groupType.DefaultGroupRoleId;
            AllowMultipleLocations = groupType.AllowMultipleLocations;
            ShowInGroupList = groupType.ShowInGroupList;
            ShowInNavigation = groupType.ShowInNavigation;
            IconCssClass = groupType.IconCssClass;
            TakesAttendance = groupType.TakesAttendance;
            AttendanceCountsAsWeekendService = groupType.AttendanceCountsAsWeekendService;
            SendAttendanceReminder = groupType.SendAttendanceReminder;
            ShowConnectionStatus = groupType.ShowConnectionStatus;
            AttendanceRule = groupType.AttendanceRule;
            AlreadyEnrolledMatchingLogic = groupType.AlreadyEnrolledMatchingLogic;
            GroupCapacityRule = groupType.GroupCapacityRule;
            AttendancePrintTo = groupType.AttendancePrintTo;
            Order = groupType.Order;
            InheritedGroupTypeId = groupType.InheritedGroupTypeId;
            AllowedScheduleTypes = groupType.AllowedScheduleTypes;
            LocationSelectionMode = groupType.LocationSelectionMode;
            EnableLocationSchedules = groupType.EnableLocationSchedules;
            GroupTypePurposeValueId = groupType.GroupTypePurposeValueId;
            IgnorePersonInactivated = groupType.IgnorePersonInactivated;
            IsIndexEnabled = groupType.IsIndexEnabled;
            GroupViewLavaTemplate = groupType.GroupViewLavaTemplate;
            LocationTypeValueIDs = groupType.LocationTypes.Select( l => l.LocationTypeValueId ).ToList();
            AllowSpecificGroupMemberAttributes = groupType.AllowSpecificGroupMemberAttributes;
            EnableSpecificGroupRequirements = groupType.EnableSpecificGroupRequirements;
            AllowGroupSync = groupType.AllowGroupSync;
            AllowSpecificGroupMemberWorkflows = groupType.AllowSpecificGroupMemberWorkflows;
            EnableGroupHistory = groupType.EnableGroupHistory;
            GroupTypeColor = groupType.GroupTypeColor;
            ShowMaritalStatus = groupType.ShowMaritalStatus;
            AdministratorTerm = groupType.AdministratorTerm;
            ShowAdministrator = groupType.ShowAdministrator;
            EnableGroupTag = groupType.EnableGroupTag;
            GroupStatusDefinedTypeId = groupType.GroupStatusDefinedTypeId;
            EnableRSVP = groupType.EnableRSVP;
            RSVPReminderSystemCommunicationId = groupType.RSVPReminderSystemCommunicationId;
            RSVPReminderOffsetDays = groupType.RSVPReminderOffsetDays;
            IsSchedulingEnabled = groupType.IsSchedulingEnabled;
            ScheduleConfirmationSystemCommunicationId = groupType.ScheduleConfirmationSystemCommunicationId;
            ScheduleReminderSystemCommunicationId = groupType.ScheduleReminderSystemCommunicationId;
            ScheduleCancellationWorkflowTypeId = groupType.ScheduleCancellationWorkflowTypeId;
            ScheduleConfirmationEmailOffsetDays = groupType.ScheduleConfirmationEmailOffsetDays;
            ScheduleReminderEmailOffsetDays = groupType.ScheduleReminderEmailOffsetDays;
            RequiresReasonIfDeclineSchedule = groupType.RequiresReasonIfDeclineSchedule;
            AllowAnyChildGroupType = groupType.AllowAnyChildGroupType;
            ScheduleConfirmationLogic = groupType.ScheduleConfirmationLogic;
            IsCapacityRequired = groupType.IsCapacityRequired;
            GroupsRequireCampus = groupType.GroupsRequireCampus;
            ScheduleCoordinatorNotificationTypes = groupType.ScheduleCoordinatorNotificationTypes;
            IsConcurrentCheckInPrevented = groupType.IsConcurrentCheckInPrevented;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the 'Family' Group Type.
        /// </summary>
        /// <returns></returns>
        public static GroupTypeCache GetFamilyGroupType() => Get( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );

        /// <summary>
        /// Gets the 'Security Role' Group Type.
        /// </summary>
        /// <returns></returns>
        public static GroupTypeCache GetSecurityRoleGroupType() => Get( SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );

        /// <summary>
        /// Gets the descendent group types (all children recursively)
        /// </summary>
        /// <returns></returns>
        public List<GroupTypeCache> GetDescendentGroupTypes()
        {
            List<int> recursionControl = new List<int>();
            return GetDescendentGroupTypes( this, recursionControl );
        }

        /// <summary>
        /// Gets the descendent group types.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="recursionControl">A list of GroupTypeIds that have already been added, this helps prevent infinite recursion.</param>
        /// <returns></returns>
        private static List<GroupTypeCache> GetDescendentGroupTypes( GroupTypeCache groupType, List<int> recursionControl = null )
        {
            var results = new List<GroupTypeCache>();

            if ( groupType != null )
            {
                recursionControl = recursionControl ?? new List<int>();
                if ( !recursionControl.Contains( groupType.Id ) )
                {
                    recursionControl.Add( groupType.Id );
                    results.Add( groupType );

                    foreach ( var childGroupType in groupType.ChildGroupTypes )
                    {
                        var childResults = GetDescendentGroupTypes( childGroupType, recursionControl );
                        childResults.ForEach( c => results.Add( c ) );
                    }
                }
            }

            return results;
        }

        #endregion
    }
}
