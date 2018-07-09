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
using System.Runtime.Serialization;

using Rock.Cache;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a groupType. This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    [Obsolete( "Use Rock.Cache.CacheGroupType instead" )]
    public class GroupTypeCache : CachedModel<GroupType>
    {
        #region Constructors

        private GroupTypeCache( CacheGroupType cacheGroupType )
        {
            CopyFromNewCache( cacheGroupType );
        }

        #endregion

        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the group term.
        /// </summary>
        /// <value>
        /// The group term.
        /// </value>
        [DataMember]
        public string GroupTerm { get; set; }

        /// <summary>
        /// Gets or sets the group member term.
        /// </summary>
        /// <value>
        /// The group member term.
        /// </value>
        [DataMember]
        public string GroupMemberTerm { get; set; }

        /// <summary>
        /// Gets or sets the default group role identifier.
        /// </summary>
        /// <value>
        /// The default group role identifier.
        /// </value>
        [DataMember]
        public int? DefaultGroupRoleId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow multiple locations].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allow multiple locations]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowMultipleLocations { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show in group list].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in group list]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowInGroupList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show in navigation].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in navigation]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowInNavigation { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [takes attendance].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [takes attendance]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool TakesAttendance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [attendance counts as weekend service].
        /// </summary>
        /// <value>
        /// <c>true</c> if [attendance counts as weekend service]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AttendanceCountsAsWeekendService { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [send attendance reminder].
        /// </summary>
        /// <value>
        /// <c>true</c> if [send attendance reminder]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool SendAttendanceReminder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show connection status].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show connection status]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the attendance rule.
        /// </summary>
        /// <value>
        /// The attendance rule.
        /// </value>
        [DataMember]
        public AttendanceRule AttendanceRule { get; set; }

        /// <summary>
        /// Gets or sets the group capacity rule.
        /// </summary>
        /// <value>
        /// The group capacity rule.
        /// </value>
        [DataMember]
        public GroupCapacityRule GroupCapacityRule { get; set; }

        /// <summary>
        /// Gets or sets the attendance print to.
        /// </summary>
        /// <value>
        /// The attendance print to.
        /// </value>
        [DataMember]
        public PrintTo AttendancePrintTo { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the inherited group type identifier.
        /// </summary>
        /// <value>
        /// The inherited group type identifier.
        /// </value>
        [DataMember]
        public int? InheritedGroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is indexable.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is indexable; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsIndexEnabled { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if specific groups are allowed to have their own member attributes.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this specific group are allowed to have their own member attributes, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowSpecificGroupMemberAttributes { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if group requirements section is enabled for group of this type.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if group requirements section is enabled for group of this type, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableSpecificGroupRequirements { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if groups of this type are allowed to be sync'ed.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if groups of this type are allowed to be sync'ed, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowGroupSync { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if groups of this type should be allowed to have Group Member Workflows.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if groups of this type should be allowed to have group member workflows, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowSpecificGroupMemberWorkflows { get; set; }

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
                    return Read( InheritedGroupTypeId.Value );
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
        public ScheduleType AllowedScheduleTypes { get; set; }


        /// <summary>
        /// Gets or sets the location selection mode.
        /// </summary>
        /// <value>
        /// The location selection mode.
        /// </value>
        [DataMember]
        public GroupLocationPickerMode LocationSelectionMode { get; set; }

        /// <summary>
        /// Gets or sets the enable location schedules.
        /// </summary>
        /// <value>
        /// The enable location schedules.
        /// </value>
        [DataMember]
        public bool? EnableLocationSchedules { get; set; }

        /// <summary>
        /// Gets or sets the group type purpose value identifier.
        /// </summary>
        /// <value>
        /// The group type purpose value identifier.
        /// </value>
        [DataMember]
        public int? GroupTypePurposeValueId { get; set; }

        /// <summary>
        /// Gets the group type purpose value.
        /// </summary>
        /// <value>
        /// The group type purpose value.
        /// </value>
        public DefinedValueCache GroupTypePurposeValue
        {
            get
            {
                if ( GroupTypePurposeValueId.HasValue && GroupTypePurposeValueId.Value != 0 )
                {
                    return DefinedValueCache.Read( GroupTypePurposeValueId.Value );
                }

                return null;
            }
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
        public bool IgnorePersonInactivated { get; set; }

        /// <summary>
        /// Gets or sets a lava template that can be used for generating  view details for Group.
        /// </summary>
        /// <value>
        /// The Group View Lava Template.
        /// </value>
        [DataMember]
        public string GroupViewLavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        /// <value>
        /// The roles.
        /// </value>
        public List<GroupTypeRoleCache> Roles { get; set; }

        /// <summary>
        /// Gets or sets the group schedule exclusions.
        /// </summary>
        /// <value>
        /// The group schedule exclusions.
        /// </value>
        public List<DateRange> GroupScheduleExclusions { get; set; }

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

                lock ( _obj )
                {
                    if ( childGroupTypeIds == null )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            childGroupTypeIds = new GroupTypeService( rockContext )
                                .GetChildGroupTypes( Id )
                                .Select( g => g.Id )
                                .ToList();
                        }
                    }
                }

                if ( childGroupTypeIds == null ) return childGroupTypes;

                foreach ( var id in childGroupTypeIds )
                {
                    var groupType = Read( id );
                    if ( groupType != null )
                    {
                        childGroupTypes.Add( groupType );
                    }
                }

                return childGroupTypes;
            }
        }
        private List<int> childGroupTypeIds;

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

                lock ( _obj )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        parentGroupTypeIds = new GroupTypeService( rockContext )
                            .GetParentGroupTypes( Id )
                            .Select( g => g.Id )
                            .ToList();
                    }
                }

                if ( parentGroupTypeIds == null ) return parentGroupTypes;
                foreach ( var id in parentGroupTypeIds )
                {
                    var groupType = Read( id );
                    if ( groupType != null )
                    {
                        parentGroupTypes.Add( groupType );
                    }
                }

                return parentGroupTypes;
            }
        }
        private List<int> parentGroupTypeIds;

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
                if ( locationTypeValueIDs == null ) return null;

                foreach ( var id in locationTypeValueIDs.ToList() )
                {
                    locationTypeValues.Add( DefinedValueCache.Read( id ) );
                }

                return locationTypeValues;

            }
        }
        private List<int> locationTypeValueIDs;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is GroupType ) ) return;

            var groupType = (GroupType)model;
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
            locationTypeValueIDs = groupType.LocationTypes.Select( l => l.LocationTypeValueId ).ToList();
            AllowSpecificGroupMemberAttributes = groupType.AllowSpecificGroupMemberAttributes;
            EnableSpecificGroupRequirements = groupType.EnableSpecificGroupRequirements;
            AllowGroupSync = groupType.AllowGroupSync;
            AllowSpecificGroupMemberWorkflows = groupType.AllowSpecificGroupMemberWorkflows;
            Roles = new List<GroupTypeRoleCache>();

            groupType.Roles
                .OrderBy( r => r.Order )
                .ToList()
                .ForEach( r => Roles.Add( new GroupTypeRoleCache( r ) ) );

            GroupScheduleExclusions = new List<DateRange>();
            groupType.GroupScheduleExclusions
                .OrderBy( s => s.StartDate )
                .ToList()
                .ForEach( s => GroupScheduleExclusions.Add( new DateRange( s.StartDate, s.EndDate ) ) );
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheGroupType ) ) return;

            var groupType = (CacheGroupType)cacheEntity;
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
            locationTypeValueIDs = groupType.LocationTypeValueIDs;
            AllowSpecificGroupMemberAttributes = groupType.AllowSpecificGroupMemberAttributes;
            EnableSpecificGroupRequirements = groupType.EnableSpecificGroupRequirements;
            AllowGroupSync = groupType.AllowGroupSync;
            AllowSpecificGroupMemberWorkflows = groupType.AllowSpecificGroupMemberWorkflows;
            Roles = new List<GroupTypeRoleCache>();

            groupType.Roles
                .OrderBy( r => r.Order )
                .ToList()
                .ForEach( r => Roles.Add( new GroupTypeRoleCache( r ) ) );

            GroupScheduleExclusions = new List<DateRange>( groupType.GroupScheduleExclusions );
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
        /// Returns GroupType object from cache.  If groupType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static GroupTypeCache Read( int id, RockContext rockContext = null )
        {
            return new GroupTypeCache( CacheGroupType.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static GroupTypeCache Read( string guid )
        {
            return new GroupTypeCache( CacheGroupType.Get( guid ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static GroupTypeCache Read( Guid guid, RockContext rockContext = null )
        {
            return new GroupTypeCache( CacheGroupType.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Reads the specified field type model.
        /// </summary>
        /// <param name="groupTypeModel">The field type model.</param>
        /// <returns></returns>
        public static GroupTypeCache Read( GroupType groupTypeModel )
        {
            return new GroupTypeCache( CacheGroupType.Get( groupTypeModel ) );
        }

        /// <summary>
        /// Alls this instance.
        /// </summary>
        /// <returns></returns>
        public static List<GroupTypeCache> All()
        {
            var groupTypes = new List<GroupTypeCache>();

            var cacheGroupTypes = CacheGroupType.All();
            if ( cacheGroupTypes == null ) return groupTypes;

            foreach ( var cacheGroupType in cacheGroupTypes )
            {
                groupTypes.Add( new GroupTypeCache( cacheGroupType ) );
            }

            return groupTypes;
        }

        /// <summary>
        /// Removes groupType from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CacheGroupType.Remove( id );
        }

        /// <summary>
        /// Gets the 'Family' Group Type.
        /// </summary>
        /// <returns></returns>
        public static GroupTypeCache GetFamilyGroupType()
        {
            return Read( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
        }

        /// <summary>
        /// Gets the 'Security Role' Group Type.
        /// </summary>
        /// <returns></returns>
        public static GroupTypeCache GetSecurityRoleGroupType()
        {
            return Read( SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );
        }

        #endregion
    }

    /// <summary>
    /// Cached version of GroupTypeRole
    /// </summary>
    [Obsolete( "Use Rock.Cache.GroupTypeRoleCache instead" )]
    public class GroupTypeRoleCache
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the maximum count.
        /// </summary>
        /// <value>
        /// The maximum count.
        /// </value>
        public int? MaxCount { get; set; }

        /// <summary>
        /// Gets or sets the minimum count.
        /// </summary>
        /// <value>
        /// The minimum count.
        /// </value>
        public int? MinCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is leader.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is leader; otherwise, <c>false</c>.
        /// </value>
        public bool IsLeader { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can view.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can view; otherwise, <c>false</c>.
        /// </value>
        public bool CanView { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can edit.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can edit; otherwise, <c>false</c>.
        /// </value>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can manage members.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can manage members; otherwise, <c>false</c>.
        /// </value>
        public bool CanManageMembers { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupTypeRoleCache"/> class.
        /// </summary>
        /// <param name="role">The role.</param>
        public GroupTypeRoleCache( GroupTypeRole role )
        {
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
        /// Initializes a new instance of the <see cref="GroupTypeRoleCache"/> class.
        /// </summary>
        /// <param name="role">The role.</param>
        public GroupTypeRoleCache( Rock.Cache.GroupTypeRoleCache role )
        {
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