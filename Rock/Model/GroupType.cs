// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

namespace Rock.Model
{

    /// <summary>
    /// Represents a type or category of <see cref="Rock.Model.Group">Groups</see> in Rock.  A GroupType is also used to configure how Groups that belong to a GroupType will operate
    /// and how they will interact with other components of Rock.
    /// </summary>
    [Table( "GroupType" )]
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
            GroupTerm = "Group";
            GroupMemberTerm = "Member";
        }

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this GroupType is part of the Rock core system/framework.  This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this GroupType is part of the Rock core system/framework.
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
        [Required]
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
        [Required]
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
        /// Gets or sets the icon CSS class name for a font vector based icon.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the CSS class name of a font based icon.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
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
        /// Gets or sets a value indicating if an attendance reminder should be sent to group leaders.
        /// </summary>
        /// <value>
        /// <c>true</c> if [send attendance reminder]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool SendAttendanceReminder { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.AttendanceRule"/> that indicates how attendance is managed a <see cref="Rock.Model.Group"/> of this GroupType
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.AttendanceRule"/> that indicates how attendance is managed for a <see cref="Rock.Model.Group"/> of this GroupType.
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
        /// Gets or sets the allowed schedule types.
        /// </summary>
        /// <value>
        /// The allowed schedule types.
        /// </value>
        [DataMember]
        public ScheduleType AllowedScheduleTypes { get; set; }

        /// <summary>
        /// Gets or sets selection mode that the Location Picker should use when adding locations to groups of this type
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Web.UI.Controls.LocationPickerMode"/> to use when adding location(s) to <see cref="Rock.Model.Group">Groups</see>
        /// of this GroupType. This can be one or more of the following values
        /// </value>
        /// <remarks>
        /// Available options include one or more of the following:
        ///     GroupLocationPickerMode.Location -> A named location.
        ///     GroupLocationPickerMode.Address -> Selection by address (i.e. 7007 W Happy Valley Rd Peoria, AZ 85383)
        ///     GroupLocationPickerMode.Point -> A geographic point (i.e. 38.229336, -85.542045)
        ///     GroupLocationPickerMode.Polygon -> A geographic polygon.
        ///     GroupLocationPickerMode.GroupMember -> A group members's address
        /// </remarks>
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
        /// Gets or sets Id of the <see cref="Rock.Model.DefinedValue"/> that represents the purpose of the GroupType.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.DefinedValue"/> that represents the purpose of the GroupType.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.GROUPTYPE_PURPOSE )]
        public int? GroupTypePurposeValueId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable alternate placements].
        /// </summary>
        /// <value>
        /// <c>true</c> if [enable alternate placements]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableAlternatePlacements { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets a collection of the <see cref="Rock.Model.Group">Groups</see> that belong to this GroupType.
        /// </summary>
        /// <value>
        /// A collection containing a collection of the <see cref="Rock.Model.Group">Groups</see> that belong to this GroupType.
        /// </value>
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
        [DataMember, LavaIgnore]
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
        public virtual ICollection<GroupType> ParentGroupTypes
        {
            get { return _parentGroupTypes ?? ( _parentGroupTypes = new Collection<GroupType>() ); }
            set { _parentGroupTypes = value; }
        }
        private ICollection<GroupType> _parentGroupTypes;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.GroupTypeRole">GroupRoles</see> that this GroupType utilizes.
        /// </summary>
        /// <value>
        /// A collection containing the <see cref="Rock.Model.GroupTypeRole"/>GroupRoles that are associated with this GroupType.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupTypeRole> Roles
        {
            get { return _roles ?? ( _roles = new Collection<GroupTypeRole>() ); }
            set { _roles = value; }
        }
        private ICollection<GroupTypeRole> _roles;

        /// <summary>
        /// Gets or sets the group member workflow triggers.
        /// </summary>
        /// <value>
        /// The group member workflow triggers.
        /// </value>
        public virtual ICollection<GroupMemberWorkflowTrigger> GroupMemberWorkflowTriggers
        {
            get { return _triggers ?? ( _triggers = new Collection<GroupMemberWorkflowTrigger>() ); }
            set { _triggers = value; }
        }
        private ICollection<GroupMemberWorkflowTrigger> _triggers;
        
        /// <summary>
        /// Gets or sets the group schedule exclusions.
        /// </summary>
        /// <value>
        /// The group schedule exclusions.
        /// </value>
        public virtual ICollection<GroupScheduleExclusion> GroupScheduleExclusions
        {
            get { return _groupScheduleExclusions ?? ( _groupScheduleExclusions = new Collection<GroupScheduleExclusion>() ); }
            set { _groupScheduleExclusions = value; }
        }
        private ICollection<GroupScheduleExclusion> _groupScheduleExclusions;

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
        [DataMember]
        public virtual GroupTypeRole DefaultGroupRole { get; set; }

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
                var groupService = new GroupService( new RockContext() );
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
        /// Pres the save.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="state">The state.</param>
        public override void PreSaveChanges( DbContext dbContext, System.Data.Entity.EntityState state )
        {
            if (state == System.Data.Entity.EntityState.Deleted)
            {
                ChildGroupTypes.Clear();
            }
        }

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
    /// Represents and indicates the type of location picker to use when setting a location for a group and/or when searching for group(s)
    /// </summary>
    [Flags]
    public enum GroupLocationPickerMode
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,

        /// <summary>
        /// An Address
        /// </summary>
        Address = 1,

        /// <summary>
        /// A Named location (Building, Room)
        /// </summary>
        Named = 2,

        /// <summary>
        /// A Geographic point (Latitude/Longitude)
        /// </summary>
        Point = 4,

        /// <summary>
        /// A Geographic Polygon
        /// </summary>
        Polygon = 8,

        /// <summary>
        /// A Group Member's address
        /// </summary>
        GroupMember = 16,

        /// <summary>
        /// All
        /// </summary>
        All = Address | Named | Point | Polygon | GroupMember

    }
    #endregion

}
