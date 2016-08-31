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
    public class GroupTypeCache : CachedModel<GroupType>
    {
        #region Constructors

        private GroupTypeCache( GroupType groupType )
        {
            CopyFromModel( groupType );
        }

        #endregion

        #region Properties

        private object _obj = new object();

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
                    return GroupTypeCache.Read( InheritedGroupTypeId.Value );
                }
                else
                {
                    return null;
                }
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
                else
                {
                    return null;
                }
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
        /// Gets or sets the roles.
        /// </summary>
        /// <value>
        /// The roles.
        /// </value>
        public List<GroupTypeRoleCache> Roles{ get; set; }

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

                lock( _obj )
                { 
                    if ( childGroupTypeIds == null )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            childGroupTypeIds = new GroupTypeService( rockContext )
                                .GetChildGroupTypes( this.Id )
                                .Select( g => g.Id )
                                .ToList();                        
                        }
                    }
                }

                foreach ( int id in childGroupTypeIds )
                {
                    var groupType = GroupTypeCache.Read( id );
                    if ( groupType != null )
                    {
                        childGroupTypes.Add( groupType );
                    }
                }

                return childGroupTypes;
            }
        }
        private List<int> childGroupTypeIds = null;

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
                            .GetParentGroupTypes( this.Id )
                            .Select( g => g.Id )
                            .ToList();
                    }
                }

                if ( parentGroupTypeIds != null )
                {
                    foreach ( int id in parentGroupTypeIds )
                    {
                        var groupType = GroupTypeCache.Read( id );
                        if ( groupType != null )
                        {
                            parentGroupTypes.Add( groupType );
                        }
                    }
                }

                return parentGroupTypes;
            }
        }
        private List<int> parentGroupTypeIds = null;

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
                List<DefinedValueCache> locationTypeValues = new List<DefinedValueCache>();

                if ( locationTypeValueIDs != null )
                {
                    foreach ( int id in locationTypeValueIDs.ToList() )
                    {
                        locationTypeValues.Add( DefinedValueCache.Read( id ) );
                    }

                    return locationTypeValues;
                }

                return null;
            }
        }
        private List<int> locationTypeValueIDs = null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is GroupType )
            {
                var groupType = (GroupType)model;
                this.IsSystem = groupType.IsSystem;
                this.Name = groupType.Name;
                this.Description = groupType.Description;
                this.GroupTerm = groupType.GroupTerm;
                this.GroupMemberTerm = groupType.GroupMemberTerm;
                this.DefaultGroupRoleId = groupType.DefaultGroupRoleId;
                this.AllowMultipleLocations = groupType.AllowMultipleLocations;
                this.ShowInGroupList = groupType.ShowInGroupList;
                this.ShowInNavigation = groupType.ShowInNavigation;
                this.IconCssClass = groupType.IconCssClass;
                this.TakesAttendance = groupType.TakesAttendance;
                this.AttendanceCountsAsWeekendService = groupType.AttendanceCountsAsWeekendService;
                this.SendAttendanceReminder = groupType.SendAttendanceReminder;
                this.ShowConnectionStatus = groupType.ShowConnectionStatus;
                this.AttendanceRule = groupType.AttendanceRule;
                this.GroupCapacityRule = groupType.GroupCapacityRule;
                this.AttendancePrintTo = groupType.AttendancePrintTo;
                this.Order = groupType.Order;
                this.InheritedGroupTypeId = groupType.InheritedGroupTypeId;
                this.AllowedScheduleTypes = groupType.AllowedScheduleTypes;
                this.LocationSelectionMode = groupType.LocationSelectionMode;
                this.EnableLocationSchedules = groupType.EnableLocationSchedules;
                this.GroupTypePurposeValueId = groupType.GroupTypePurposeValueId;
                this.IgnorePersonInactivated = groupType.IgnorePersonInactivated;

                this.locationTypeValueIDs = groupType.LocationTypes.Select( l => l.LocationTypeValueId ).ToList();

                this.Roles = new List<GroupTypeRoleCache>();
                groupType.Roles
                    .OrderBy( r => r.Order )
                    .ToList()
                    .ForEach( r => Roles.Add( new GroupTypeRoleCache( r ) ) );

                this.GroupScheduleExclusions = new List<DateRange>();
                groupType.GroupScheduleExclusions
                    .OrderBy( s => s.StartDate )
                    .ToList()
                    .ForEach( s => GroupScheduleExclusions.Add( new DateRange( s.StartDate, s.EndDate ) ) );
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:GroupType:{0}", id );
        }

        /// <summary>
        /// Returns GroupType object from cache.  If groupType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static GroupTypeCache Read( int id, RockContext rockContext = null )
        {
            return GetOrAddExisting( GroupTypeCache.CacheKey( id ),
                () => LoadById( id, rockContext ) );
        }

        private static GroupTypeCache LoadById( int id, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadById2( id, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadById2( id, rockContext2 );
            }
        }

        private static GroupTypeCache LoadById2( int id, RockContext rockContext )
        {
            var groupTypeService = new GroupTypeService( rockContext );
            var groupTypeModel = groupTypeService.Queryable().Include(a => a.Roles).FirstOrDefault(a => a.Id == id );
            if ( groupTypeModel != null )
            {
                return new GroupTypeCache( groupTypeModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static GroupTypeCache Read( string guid )
        {
            return Read( new Guid( guid ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static GroupTypeCache Read( Guid guid, RockContext rockContext = null )
        {
            int id = GetOrAddExisting( guid.ToString(),
                () => LoadByGuid( guid, rockContext ) );

            return Read( id, rockContext );
        }

        private static int LoadByGuid( Guid guid, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadByGuid2( guid, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadByGuid2( guid, rockContext2 );
            }
        }

        private static int LoadByGuid2( Guid guid, RockContext rockContext )
        {
            var groupTypeService = new GroupTypeService( rockContext );
            return groupTypeService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ) )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Reads the specified field type model.
        /// </summary>
        /// <param name="groupTypeModel">The field type model.</param>
        /// <returns></returns>
        public static GroupTypeCache Read( GroupType groupTypeModel )
        {
            return GetOrAddExisting( GroupTypeCache.CacheKey( groupTypeModel.Id ),
                () => LoadByModel( groupTypeModel ) );
        }

        private static GroupTypeCache LoadByModel( GroupType groupTypeModel )
        {
            if ( groupTypeModel != null )
            {
                return new GroupTypeCache( groupTypeModel );
            }
            return null;
        }

        /// <summary>
        /// Removes groupType from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            FlushCache( GroupTypeCache.CacheKey( id ) );
        }

        /// <summary>
        /// Gets the 'Family' Group Type.
        /// </summary>
        /// <returns></returns>
        public static GroupTypeCache GetFamilyGroupType()
        {
            return GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
        }

        /// <summary>
        /// Gets the 'Security Role' Group Type.
        /// </summary>
        /// <returns></returns>
        public static GroupTypeCache GetSecurityRoleGroupType()
        {
            return GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );
        }

        #endregion
    }

    /// <summary>
    /// Cached version of GroupTypeRole
    /// </summary>
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
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}