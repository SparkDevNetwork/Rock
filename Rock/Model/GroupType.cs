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
    /// Group Type POCO Entity.
    /// </summary>
    [Table( "GroupType" )]
    [FriendlyTypeName( "Group Type" )]
    [DataContract]
    public partial class GroupType : Model<GroupType>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the group term.
        /// </summary>
        /// <value>
        /// The group term.
        /// </value>
        [MaxLength(100)]
        [DataMember]
        public string GroupTerm { get; set; }

        /// <summary>
        /// Gets or sets the group member term.
        /// </summary>
        /// <value>
        /// The group member term.
        /// </value>
        [MaxLength(100)]
        [DataMember]
        public string GroupMemberTerm { get; set; }

        /// <summary>
        /// Gets or sets the Default Group Role Id.
        /// </summary>
        /// <value>
        /// Default Group Role Id.
        /// </value>
        [DataMember]
        public int? DefaultGroupRoleId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether groups of this type support multiple locations
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
        public bool ShowInGroupList
        {
            get { return showInGroupList; }
            set { showInGroupList = value; }
        }
        private bool showInGroupList = true;

        /// <summary>
        /// Gets or sets the small icon.
        /// </summary>
        /// <value>
        /// The small icon.
        /// </value>
        [DataMember]
        public int? IconSmallFileId { get; set; }

        /// <summary>
        /// Gets or sets the large icon.
        /// </summary>
        /// <value>
        /// The large icon.
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
        /// Gets or sets a value indicating whether groups of this type support taking attendance
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

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the Groups.
        /// </summary>
        /// <value>
        /// Collection of Groups.
        /// </value>
        [DataMember]
        public virtual ICollection<Group> Groups
        {
            get { return _groups ?? ( _groups = new Collection<Group>() ); }
            set { _groups = value; }
        }
        private ICollection<Group> _groups;

        /// <summary>
        /// Gets or sets the Child Group Types.
        /// </summary>
        /// <value>
        /// Collection of Child Group Types.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupType> ChildGroupTypes
        {
            get { return _childGroupTypes ?? ( _childGroupTypes = new Collection<GroupType>() ); }
            set { _childGroupTypes = value; }
        }
        private ICollection<GroupType> _childGroupTypes;

        /// <summary>
        /// Gets or sets the Parent Group Types.
        /// </summary>
        /// <value>
        /// Collection of Parent Group Types.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupType> ParentGroupTypes
        {
            get { return _parentGroupTypes ?? ( _parentGroupTypes = new Collection<GroupType>() ); }
            set { _parentGroupTypes = value; }
        }
        private ICollection<GroupType> _parentGroupTypes;

        /// <summary>
        /// Gets or sets the Group Roles.
        /// </summary>
        /// <value>
        /// Collection of Group Roles.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupRole> Roles
        {
            get { return _roles ?? ( _roles = new Collection<GroupRole>() ); }
            set { _roles = value; }
        }
        private ICollection<GroupRole> _roles;

        /// <summary>
        /// Gets or sets the location types.
        /// </summary>
        /// <value>
        /// The location types.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupTypeLocationType> LocationTypes
        {
            get { return _locationTypes ?? ( _locationTypes = new Collection<GroupTypeLocationType>() ); }
            set { _locationTypes = value; }
        }
        private ICollection<GroupTypeLocationType> _locationTypes;

        /// <summary>
        /// Gets or sets the Default Group Role.
        /// </summary>
        /// <value>
        /// A <see cref="GroupRole"/> object.
        /// </value>
        [DataMember]
        public virtual GroupRole DefaultGroupRole { get; set; }

        /// <summary>
        /// Gets or sets the small icon.
        /// </summary>
        /// <value>
        /// The small icon.
        /// </value>
        [DataMember]
        public virtual BinaryFile IconSmallFile { get; set; }

        /// <summary>
        /// Gets or sets the large icon.
        /// </summary>
        /// <value>
        /// The large icon.
        /// </value>
        [DataMember]
        public virtual BinaryFile IconLargeFile { get; set; }

        /// <summary>
        /// Gets the group query.
        /// </summary>
        /// <value>
        /// The group query.
        /// </value>
        public virtual int GroupCount
        {
            get
            {
                return GroupQuery.Count();
            }
        }

        /// <summary>
        /// Gets the group query.
        /// </summary>
        /// <value>
        /// The group query.
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

        #endregion

        #region Public Methods

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
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// The attendance rule to use when person checks in to a group of this type
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

    #endregion

}
