//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.Serialization;

using Rock;
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

        private GroupTypeCache()
        {
        }

        private GroupTypeCache( GroupType groupType )
        {
            CopyFromModel( groupType );
        }

        #endregion

        #region Properties

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
        /// Gets or sets the icon small file identifier.
        /// </summary>
        /// <value>
        /// The icon small file identifier.
        /// </value>
        [DataMember]
        public int? IconSmallFileId { get; set; }

        /// <summary>
        /// Gets or sets the icon large file identifier.
        /// </summary>
        /// <value>
        /// The icon large file identifier.
        /// </value>
        [DataMember]
        public int? IconLargeFileId { get; set; }

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
        /// Gets or sets the attendance rule.
        /// </summary>
        /// <value>
        /// The attendance rule.
        /// </value>
        [DataMember]
        public AttendanceRule AttendanceRule { get; set; }

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
        /// Gets or sets the location selection mode.
        /// </summary>
        /// <value>
        /// The location selection mode.
        /// </value>
        [DataMember]
        public GroupLocationPickerMode LocationSelectionMode { get; set; }

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
                if (GroupTypePurposeValueId.HasValue && GroupTypePurposeValueId.Value != 0)
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
        /// Gets the child group types.
        /// </summary>
        /// <value>
        /// The child group types.
        /// </value>
        public List<GroupTypeCache> ChildGroupTypes
        {
            get
            {
                List<GroupTypeCache> childGroupTypes = new List<GroupTypeCache>();

                if ( childGroupTypeIds != null )
                {
                    foreach ( int id in childGroupTypeIds.ToList() )
                    {
                        childGroupTypes.Add( GroupTypeCache.Read( id ) );
                    }
                }
                else
                {
                    childGroupTypeIds = new List<int>();

                    var groupTypeService = new GroupTypeService();
                    foreach ( GroupType groupType in groupTypeService.GetChildGroupTypes( this.Id ) )
                    {
                        groupType.LoadAttributes();
                        childGroupTypeIds.Add( groupType.Id );
                        childGroupTypes.Add( GroupTypeCache.Read( groupType ) );
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
                List<GroupTypeCache> parentGroupTypes = new List<GroupTypeCache>();

                if ( parentGroupTypeIds != null )
                {
                    foreach ( int id in parentGroupTypeIds.ToList() )
                    {
                        parentGroupTypes.Add( GroupTypeCache.Read( id ) );
                    }
                }
                else
                {
                    parentGroupTypeIds = new List<int>();

                    var groupTypeService = new GroupTypeService();
                    foreach ( GroupType groupType in groupTypeService.GetParentGroupTypes( this.Id ) )
                    {
                        groupType.LoadAttributes();
                        parentGroupTypeIds.Add( groupType.Id );
                        parentGroupTypes.Add( GroupTypeCache.Read( groupType ) );
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
                this.IconSmallFileId = groupType.IconSmallFileId;
                this.IconLargeFileId = groupType.IconLargeFileId;
                this.IconCssClass = groupType.IconCssClass;
                this.TakesAttendance = groupType.TakesAttendance;
                this.AttendanceRule = groupType.AttendanceRule;
                this.AttendancePrintTo = groupType.AttendancePrintTo;
                this.Order = groupType.Order;
                this.InheritedGroupTypeId = groupType.InheritedGroupTypeId;
                this.LocationSelectionMode = groupType.LocationSelectionMode;
                this.GroupTypePurposeValueId = groupType.GroupTypePurposeValueId;
                this.locationTypeValueIDs = groupType.LocationTypes.Select( l => l.LocationTypeValueId).ToList();
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
        /// <param name="id"></param>
        /// <returns></returns>
        public static GroupTypeCache Read( int id )
        {
            string cacheKey = GroupTypeCache.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            GroupTypeCache groupType = cache[cacheKey] as GroupTypeCache;

            if ( groupType != null )
            {
                return groupType;
            }
            else
            {
                var groupTypeService = new GroupTypeService();
                var groupTypeModel = groupTypeService.Get( id );
                if ( groupTypeModel != null )
                {
                    groupTypeModel.LoadAttributes();
                    groupType = new GroupTypeCache( groupTypeModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( cacheKey, groupType, cachePolicy );
                    cache.Set( groupType.Guid.ToString(), groupType.Id, cachePolicy );
                    
                    return groupType;
                }
                else
                {
                    return null;
                }
            }
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
        /// <returns></returns>
        public static GroupTypeCache Read( Guid guid )
        {
            ObjectCache cache = MemoryCache.Default;
            object cacheObj = cache[guid.ToString()];

            if ( cacheObj != null )
            {
                return Read( (int)cacheObj );
            }
            else
            {
                var groupTypeService = new GroupTypeService();
                var groupTypeModel = groupTypeService.Get( guid );
                if ( groupTypeModel != null )
                {
                    groupTypeModel.LoadAttributes();
                    var groupType = new GroupTypeCache( groupTypeModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( GroupTypeCache.CacheKey( groupType.Id ), groupType, cachePolicy );
                    cache.Set( groupType.Guid.ToString(), groupType.Id, cachePolicy );

                    return groupType;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// Reads the specified field type model.
        /// </summary>
        /// <param name="groupTypeModel">The field type model.</param>
        /// <returns></returns>
        public static GroupTypeCache Read( GroupType groupTypeModel )
        {
            string cacheKey = GroupTypeCache.CacheKey( groupTypeModel.Id );

            ObjectCache cache = MemoryCache.Default;
            GroupTypeCache groupType = cache[cacheKey] as GroupTypeCache;

            if ( groupType != null )
            {
                return groupType;
            }
            else
            {
                groupType = new GroupTypeCache( groupTypeModel );

                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, groupType, cachePolicy );
                cache.Set( groupType.Guid.ToString(), groupType.Id, cachePolicy );
                
                return groupType;
            }
        }

        /// <summary>
        /// Removes groupType from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( GroupTypeCache.CacheKey( id ) );
        }

        /// <summary>
        /// Gets the 'Family' Group Type.
        /// </summary>
        /// <returns></returns>
        public static GroupTypeCache GetFamilyGroupType()
        {
            return GroupTypeCache.Read(Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid());
        }

        /// <summary>
        /// Gets the 'Security Role' Group Type.
        /// </summary>
        /// <returns></returns>
        public static GroupTypeCache GetSecurityRoleGroupType()
        {
            return GroupTypeCache.Read(Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid());
        }

        #endregion
    }
}