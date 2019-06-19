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

using Rock.Data;
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

        #region Properties

        private readonly object _obj = new object();

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
        /// Gets the group type purpose value.
        /// </summary>
        /// <value>
        /// The group type purpose value.
        /// </value>
        public DefinedValueCache GroupTypePurposeValue => GroupTypePurposeValueId.HasValue ? DefinedValueCache.Get( GroupTypePurposeValueId.Value ) : null;

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
        public int? ScheduleConfirmationSystemEmailId { get; private set; }

        /// <summary>
        /// Gets or sets the communication template to use when sending a schedule reminder
        /// </summary>
        /// <value>
        /// The schedule reminder communication template identifier.
        /// </value>
        [DataMember]
        public int? ScheduleReminderSystemEmailId { get; private set; }

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
                    lock ( _obj )
                    {
                        if ( _roles == null )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                Roles = new List<GroupTypeRoleCache>();
                                new GroupTypeRoleService( rockContext )
                                    .Queryable().AsNoTracking()
                                    .Where( r => r.GroupTypeId == Id )
                                    .OrderBy( r => r.Order )
                                    .ToList()
                                    .ForEach( r => Roles.Add( new GroupTypeRoleCache( r ) ) );
                            }
                        }
                    }
                }
                return _roles;
            }
            private set
            {
                _roles = value;
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
                    lock ( _obj )
                    {
                        if ( _groupScheduleExclusions == null )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                GroupScheduleExclusions = new List<DateRange>();
                                new GroupScheduleExclusionService( rockContext )
                                    .Queryable().AsNoTracking()
                                    .Where( s => s.GroupTypeId == Id )
                                    .OrderBy( s => s.StartDate )
                                    .ToList()
                                    .ForEach( s => GroupScheduleExclusions.Add( new DateRange( s.StartDate, s.EndDate ) ) );

                            }
                        }
                    }
                }
                return _groupScheduleExclusions;
            }
            private set
            {
                _groupScheduleExclusions = value;
            }
        }
        private List<DateRange> _groupScheduleExclusions;

        /// <summary>
        /// Gets the child group types.
        /// </summary>
        /// <value>
        /// The child group types.
        /// </value>
        public List<GroupTypeCache> ChildGroupTypes
        {
            get
            {
                var childGroupTypes = new List<GroupTypeCache>();

                if ( ChildGroupTypeIds == null )
                {
                    lock ( _obj )
                    {
                        if ( ChildGroupTypeIds == null )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                ChildGroupTypeIds = new GroupTypeService( rockContext )
                                    .GetChildGroupTypes( Id )
                                    .Select( g => g.Id )
                                    .ToList();
                            }
                        }
                    }
                }


                if ( ChildGroupTypeIds == null )
                    return childGroupTypes;

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

        [DataMember]
        private List<int> ChildGroupTypeIds { get; set; }

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

                if ( _parentGroupTypeIds == null )
                {
                    lock ( _obj )
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
                    }
                }

                if ( _parentGroupTypeIds == null )
                    return parentGroupTypes;

                foreach ( var id in _parentGroupTypeIds )
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

        #endregion

        #region Public Methods

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
            IsSchedulingEnabled = groupType.IsSchedulingEnabled;
            ScheduleConfirmationSystemEmailId = groupType.ScheduleConfirmationSystemEmailId;
            ScheduleReminderSystemEmailId = groupType.ScheduleReminderSystemEmailId;
            ScheduleCancellationWorkflowTypeId = groupType.ScheduleCancellationWorkflowTypeId;
            ScheduleConfirmationEmailOffsetDays = groupType.ScheduleConfirmationEmailOffsetDays;
            ScheduleReminderEmailOffsetDays = groupType.ScheduleReminderEmailOffsetDays;
            RequiresReasonIfDeclineSchedule = groupType.RequiresReasonIfDeclineSchedule;
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

        /// <summary>
        /// Reads the specified unique identifier.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Get Instead" )]
        public static GroupTypeCache Read( string guid )
        {
            return Get( new Guid( guid ) );
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

        #endregion
    }

    /// <summary>
    /// Cached version of GroupTypeRole
    /// </summary>
    [Serializable]
    [DataContract]
    public class GroupTypeRoleCache
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [DataMember]
        public int Id { get; private set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        [DataMember]
        public Guid Guid { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets or sets the maximum count.
        /// </summary>
        /// <value>
        /// The maximum count.
        /// </value>
        [DataMember]
        public int? MaxCount { get; private set; }

        /// <summary>
        /// Gets or sets the minimum count.
        /// </summary>
        /// <value>
        /// The minimum count.
        /// </value>
        [DataMember]
        public int? MinCount { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is leader.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is leader; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsLeader { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can view.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can view; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CanView { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can edit.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can edit; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CanEdit { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can manage members.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can manage members; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CanManageMembers { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupTypeRoleCache"/> class.
        /// </summary>
        /// <param name="role">The role.</param>
        public GroupTypeRoleCache( GroupTypeRole role )
        {
            if ( role == null )
            {
                return;
            }

            Id = role.Id;
            Guid = role.Guid;
            Name = role.Name;
            Order = role.Order;
            MaxCount = role.MaxCount;
            MinCount = role.MinCount;
            IsLeader = role.IsLeader;
            CanView = role.CanView;
            CanEdit = role.CanEdit;
            CanManageMembers = role.CanManageMembers;
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
    }
}