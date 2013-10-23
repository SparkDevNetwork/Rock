//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{

    /// <summary>
    /// Represents a type or category of <see cref="Rock.Model.Group">Groups</see> in RockChMS.  A GroupType is also used to configure how Groups that belong to a GroupType will operate
    /// and how they will interact with other components of RockChMS.
    /// </summary>
    [Table( "GroupType" )]
    [FriendlyTypeName( "Group Type" )]
    [DataContract]
    public partial class GroupType : Model<GroupType>, IOrdered
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupType"/> class.
        /// </summary>
        public GroupType()
        {
            ShowInGroupList = true;
            ShowInNavigation = true;
        }

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this GroupType is part of the RockChMS core system/framework.  This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this GroupType is part of the RockChMS core system/framework.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Name of the GroupType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the GroupType.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description of the GroupType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the GroupType.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the term that a <see cref="Rock.Model.Group"/> belonging to this <see cref="Rock.Model.GroupType"/> is called.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the term that a <see cref="Rock.Model.Group"/> belonging to this <see cref="Rock.Model.GroupType"/> is called.
        /// </value>
        /// <remarks>
        /// Examples of GroupTerms include: group, community, class, family, etc.
        /// </remarks>
        [MaxLength( 100 )]
        [DataMember]
        public string GroupTerm { get; set; }

        /// <summary>
        /// Gets or sets the term that a <see cref="Rock.Model.GroupMember"/> of a <see cref="Rock.Model.Group"/> that belongs to this GroupType is called.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the term that a <see cref="Rock.Model.GroupMember"/> of a <see cref="Rock.Model.Group"/> belonging to this 
        /// GroupType is called.
        /// </value>
        /// <example>
        /// Examples of GroupMemberTerms include: member, attendee, team member, student, etc.
        /// </example>
        [MaxLength( 100 )]
        [DataMember]
        public string GroupMemberTerm { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.GroupTypeRole"/> that a <see cref="Rock.Model.GroupMember"/> of a <see cref="Rock.Model.Group"/> belonging to this GroupType is given by default.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupTypeRole"/> that a <see cref="Rock.Model.GroupMember"/> of a <see cref="Rock.Model.Group"/> belonging to this GroupType is given by default.
        /// </value>
        [DataMember]
        public int? DefaultGroupRoleId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if <see cref="Rock.Model.Group">Groups</see> of this type are allowed to have multiple locations.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if a <see cref="Rock.Model.Group"/> of this GroupType are allowed to have multiple locations; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowMultipleLocations { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if a <see cref="Rock.Model.Group"/> of this GroupType will be shown in the group list.
        /// </summary>
        /// <value>
        ///   A <see cref="System.Boolean"/> value that is <c>true</c> if a <see cref="Rock.Model.Group"/> of this GroupType will be shown in the GroupList; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowInGroupList { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this GroupType and its <see cref="Rock.Model.Group">Groups</see> are shown in Navigation.
        /// If false, this GroupType will be hidden navigation controls, such as TreeViews and Menus
        /// </summary>
        /// <remarks>
        ///  Navigation controls include objects lie menus and treeview controls.
        /// </remarks>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this GroupType and Groups should be displayed in Navigation controls.
        /// </value>
        [DataMember]
        public bool ShowInNavigation { get; set; }

        /// <summary>
        /// Gets the Id of the <see cref="Rock.Model.BinaryFile"/> image that is used for the GroupType's small icon.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the image that is used for the GroupType's small icon.
        /// </value>
        [DataMember]
        public int? IconSmallFileId { get; set; }

        /// <summary>
        /// Gets or sets Id of the <see cref="Rock.Model.BinaryFile"/> image that is used for the GroupType's large icon.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the image that is used for the GroupType's large icon.
        /// </value>
        [DataMember]
        public int? IconLargeFileId { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class name for a font vector based icon.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the CSS class name of a font based icon.
        /// </value>
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if a <see cref="Rock.Model.Group" /> of this GroupType supports taking attendance.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean" /> representing if a <see cref="Rock.Model.Group" /> of this GroupType supports taking attendance.
        /// </value>
        [DataMember]
        public bool TakesAttendance { get; set; }


        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.AttendanceRule"/> that indicates how attendance is managed a <see cref="Rock.Model.Group"/> of this GroupType
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.AttendanceRule"/> that indicates how attendance is manged for a <see cref="Rock.Model.Group"/> of this GroupType.
        /// </value>
        /// <example>
        /// The available options are:
        /// AttendanceRule.None -> A <see cref="Rock.Model.Person"/> does not have to previously belong to the <see cref="Rock.Model.Group"/> that they are checking into, and they will not be automatically added.
        /// AttendanceRule.AddOnCheckin -> If a <see cref="Rock.Model.Person"/> does not belong to the <see cref="Rock.Model.Group"/> that they are checking into, they will be automatically added with the default
        /// <see cref="Rock.Model.GroupTypeRole"/> upon check in.
        /// </example>
        [DataMember]
        public AttendanceRule AttendanceRule { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PrintTo"/> indicating the type of  location of where attendee labels for <see cref="Rock.Model.Group">Groups</see> of this GroupType should print.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.PrintTo"/> enum value indicating how and where attendee labels for <see cref="Rock.Model.Group">Groups</see> of this GroupType should print.
        /// </value>
        /// <remarks>
        /// The available options include:
        /// PrintTo.Default -> print to the default printer.
        /// PrintTo.Kiosk -> print to the printer associated with the kiosk.
        /// PrintTo.Location -> print to the location
        /// </remarks>
        [DataMember]
        public PrintTo AttendancePrintTo { get; set; }

        /// <summary>
        /// Gets or sets the order for this GroupType. This is used for display and priority purposes, the lower the number the higher the priority, or the higher the GroupType is displayed. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the display/priority order for this GroupType.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the Id of the GroupType to inherit settings and properties from. This is essentially copying the values, but they can be overridden.
        /// </summary>
        /// <value>A <see cref="System.Int32"/> representing the Id of a GroupType to inherit properties and values from.</value>
        [DataMember]
        public int? InheritedGroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the type of <see cref="Rock.Model.Location">Locations</see> that can be selected for <see cref="Rock.Model.Group">Groups</see> of this type and 
        /// this property is also used to configure the Location Picker.
        /// </summary>
        /// <value>
        /// The <see cref="LocationPickerMode"/> that represents the type of <see cref="Rock.Model.Location">Locations</see> that can be selected for <see cref="Rock.Model.Group">Groups</see>
        /// of this GroupType. This property is also used to configure the Location Picker.
        /// </value>
        /// <remarks>
        /// Available options include:
        ///     LocationPickerMode.Any -> Use any Location Picker mode.
        ///     LocationPickerMode.Address -> Selection by address (i.e. 7007 W Happy Valley Rd Peoria, AZ 85383)
        ///     LocationPickerMode.Point -> A geographic point (i.e. 38.229336, -85.542045)
        ///     LocationPickerMode.Polygon -> A geographic polygon.
        ///     LocationPickerMode.PointofInterest -> A named location.
        /// </remarks>
        [DataMember]
        public LocationPickerMode LocationSelectionMode { get; set; }

        /// <summary>
        /// Gets or sets Id of the <see cref="Rock.Model.DefinedValue"/> that represents the purpose of the GroupType.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.DefinedValue"/> that represents the purpose of the GroupType.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.GROUPTYPE_PURPOSE )]
        public int? GroupTypePurposeValueId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets a collection of the <see cref="Rock.Model.Group">Groups</see> that belong to this GroupType.
        /// </summary>
        /// <value>
        /// A collection containing a collection of the <see cref="Rock.Model.Group">Groups</see> that belong to this GroupType.
        /// </value>
        [DataMember]
        public virtual ICollection<Group> Groups
        {
            get { return _groups ?? ( _groups = new Collection<Group>() ); }
            set { _groups = value; }
        }
        private ICollection<Group> _groups;

        /// <summary>
        /// Gets or sets the collection of GroupTypes that inherit from this GroupType.
        /// </summary>
        /// <value>
        /// A collection of the GroupTypes that inherit from this groupType.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupType> ChildGroupTypes
        {
            get { return _childGroupTypes ?? ( _childGroupTypes = new Collection<GroupType>() ); }
            set { _childGroupTypes = value; }
        }
        private ICollection<GroupType> _childGroupTypes;

        /// <summary>
        /// Gets or sets a collection containing the GroupTypes that this GroupType inherits from.
        /// </summary>
        /// <value>
        /// A collection containing the GroupTypes that this GroupType inherits from.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupType> ParentGroupTypes
        {
            get { return _parentGroupTypes ?? ( _parentGroupTypes = new Collection<GroupType>() ); }
            set { _parentGroupTypes = value; }
        }
        private ICollection<GroupType> _parentGroupTypes;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.GroupRole">GroupRoles</see> that this GroupType utilizes.
        /// </summary>
        /// <value>
        /// A collection containing the <see cref="Rock.Model.GroupRole">GroupRoles that are associated with this GroupType.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupTypeRole> Roles
        {
            get { return _roles ?? ( _roles = new Collection<GroupTypeRole>() ); }
            set { _roles = value; }
        }
        private ICollection<GroupTypeRole> _roles;

        /// <summary>
        /// Gets or sets a collection of the <see cref="Rock.Model.GroupTypeLocationType">GroupTypeLocationTypes</see> that are associated with this GroupType.
        /// </summary>
        /// <value>
        /// A collection of the <see cref="Rock.Model.GroupTypeLocationType">GroupTypeLocationTypes</see> that are associated with this GroupType.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupTypeLocationType> LocationTypes
        {
            get { return _locationTypes ?? ( _locationTypes = new Collection<GroupTypeLocationType>() ); }
            set { _locationTypes = value; }
        }
        private ICollection<GroupTypeLocationType> _locationTypes;


        /// <summary>
        /// Gets or sets the default <see cref="Rock.Model.GroupTypeRole"/> for <see cref="Rock.Model.GroupMember">GroupMembers</see> who belong to a 
        /// <see cref="Rock.Model.Group"/> of this GroupType.
        /// </summary>
        /// <value>
        /// The default <see cref="Rock.Model.GroupTypeRole"/> for <see cref="Rock.Model.GroupMember">GroupMembers</see> who belong to a <see cref="Rock.Model.Group"/>
        /// of this GroupType.
        /// </value>
        public virtual GroupTypeRole DefaultGroupRole { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BinaryFile"/> that is used as the small icon representing this GroupType. This is only used when the GroupType uses
        /// an image/file based icon.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.BinaryFile"/> representing the image that is used as the small icon for this GroupType. This value will be null if no icon is provided or
        /// if a font based icon is used.
        /// 
        /// </value>
        [DataMember]
        public virtual BinaryFile IconSmallFile { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BinaryFile"/> that is used as the large icon representing this GroupType. This is only used when a GroupType uses
        /// an image/file based icon.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.BinaryFile"/> representing the image that is used as the large icon for this GroupType. This value will be null if no icon is provided or
        /// if a font based icon is used.
        /// </value>
        [DataMember]
        public virtual BinaryFile IconLargeFile { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> that represents the purpose of the GroupType.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> that represents the the purpose of the GroupType.
        /// </value>
        [DataMember]
        public virtual DefinedValue GroupTypePurposeValue { get; set; }
        
        /// <summary>
        /// Gets a count of <see cref="Rock.Model.Group">Groups</see> that belong to this GroupType.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the number of <see cref="Rock.Model.Group">Groups</see> that belong to this GroupType.
        /// </value>
        public virtual int GroupCount
        {
            get
            {
                return GroupQuery.Count();
            }
        }

        /// <summary>
        /// Gets a queryable collection of <see cref="Rock.Model.Group">Groups</see> that belong to this GroupType.
        /// </summary>
        /// <value>
        /// A queryable collection of <see cref="Rock.Model.Group">Groups</see> that belong to this GroupType.
        /// </value>
        public virtual IQueryable<Group> GroupQuery
        {
            get
            {
                var groupService = new GroupService();
                var qry = groupService.Queryable().Where( a => a.GroupTypeId.Equals( this.Id ) );
                return qry;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupType"/> that this GroupType is inheriting settings and properties from. 
        /// This is similar to a parent or a template GroupType.
        /// </summary>
        /// <value>The <see cref="Rock.Model.GroupType"/> that this GroupType is inheriting settings and properties from.</value>
        public virtual GroupType InheritedGroupType { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Name of the GroupType that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the name of the GroupType that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Group Type Configuration class.
    /// </summary>
    public partial class GroupTypeConfiguration : EntityTypeConfiguration<GroupType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupTypeConfiguration"/> class.
        /// </summary>
        public GroupTypeConfiguration()
        {
            this.HasMany( p => p.ChildGroupTypes ).WithMany( c => c.ParentGroupTypes ).Map( m => { m.MapLeftKey( "GroupTypeId" ); m.MapRightKey( "ChildGroupTypeId" ); m.ToTable( "GroupTypeAssociation" ); } );
            this.HasOptional( p => p.DefaultGroupRole ).WithMany().HasForeignKey( p => p.DefaultGroupRoleId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.IconSmallFile ).WithMany().HasForeignKey( p => p.IconSmallFileId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.IconLargeFile ).WithMany().HasForeignKey( p => p.IconLargeFileId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.InheritedGroupType ).WithMany().HasForeignKey( p => p.InheritedGroupTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Represents and indicates the  attendance rule to use when a <see cref="Rock.Model.Person"/> checks in to a <see cref="Rock.Model.Group"/> of this <see cref="Rock.Model.GroupType"/>
    /// </summary>
    public enum AttendanceRule
    {
        /// <summary>
        /// None, person does not need to belong to the group, and they will not automatically 
        /// be added to the group
        /// </summary>
        None = 0,

        /// <summary>
        /// Person will be added to the group whenever they check-in
        /// </summary>
        AddOnCheckIn = 1,

        /// <summary>
        /// User must already belong to the group before they will be allowed to check-in
        /// </summary>
        AlreadyBelongs = 2
    }

    /// <summary>
    /// Represents the type of <see cref="Rock.Model.Location">Locations</see> that should be allowed to be selected using the location picker.
    /// TODO: Move this enum to the LocationPicker class when created
    /// </summary>
    public enum LocationPickerMode
    {
        /// <summary>
        /// Any location type.
        /// </summary>
        Any = 0,

        /// <summary>
        /// An Address
        /// </summary>
        Address = 1,

        /// <summary>
        /// A Geographic point (Latitude/Longitude)
        /// </summary>
        Point = 2,

        /// <summary>
        /// A Geographic Polygon
        /// </summary>
        Polygon = 3,

        /// <summary>
        /// A Named location (Building, Room)
        /// </summary>
        PointOfInterest = 4
    }

    #endregion

}
