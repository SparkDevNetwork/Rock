using System;
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
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Group Type POCO Entity.
    /// </summary>
    [Table( "GroupType" )]
    [FriendlyTypeName( "Group Type" )]
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
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the group term.
        /// </summary>
        /// <value>
        /// The group term.
        /// </value>
        [MaxLength(100)]
        public string GroupTerm { get; set; }

        /// <summary>
        /// Gets or sets the group member term.
        /// </summary>
        /// <value>
        /// The group member term.
        /// </value>
        [MaxLength(100)]
        public string GroupMemberTerm { get; set; }

        /// <summary>
        /// Gets or sets the Default Group Role Id.
        /// </summary>
        /// <value>
        /// Default Group Role Id.
        /// </value>
        public int? DefaultGroupRoleId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether groups of this type support multiple locations
        /// </summary>
        /// <value>
        /// <c>true</c> if [allow multiple locations]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowMultipleLocations { get; set; }

        /// <summary>
        /// Gets or sets the small icon.
        /// </summary>
        /// <value>
        /// The small icon.
        /// </value>
        public int? SmallIconFileId { get; set; }

        /// <summary>
        /// Gets or sets the large icon.
        /// </summary>
        /// <value>
        /// The large icon.
        /// </value>
        public int? LargeIconFileId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether groups of this type support taking attendance
        /// </summary>
        /// <value>
        ///   <c>true</c> if [takes attendance]; otherwise, <c>false</c>.
        /// </value>
        public bool TakesAttendance { get; set; }

        /// <summary>
        /// Gets or sets the attendance rule.
        /// </summary>
        /// <value>
        /// The attendance rule.
        /// </value>
        public AttendanceRule AttendanceRule { get; set; }

        /// <summary>
        /// Gets or sets the attendance print to.
        /// </summary>
        /// <value>
        /// The attendance print to.
        /// </value>
        public PrintTo AttendancePrintTo { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the Groups.
        /// </summary>
        /// <value>
        /// Collection of Groups.
        /// </value>
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
        public virtual GroupRole DefaultGroupRole { get; set; }

        /// <summary>
        /// Gets or sets the small icon.
        /// </summary>
        /// <value>
        /// The small icon.
        /// </value>
        public virtual BinaryFile SmallIconFile { get; set; }

        /// <summary>
        /// Gets or sets the large icon.
        /// </summary>
        /// <value>
        /// The large icon.
        /// </value>
        public virtual BinaryFile LargeIconFile { get; set; }

        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        public override IDto Dto
        {
            get { return this.ToDto(); }
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

        #region Private Methods

        #endregion

        #region Static Methods

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static GroupType Read( int id )
        {
            return Read<GroupType>( id );
        }

        /// <summary>
        /// Static method to return an object based on the GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static GroupType Read( Guid guid )
        {
            return Read<GroupType>( guid );
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
            this.HasOptional( p => p.SmallIconFile ).WithMany().HasForeignKey( p => p.SmallIconFileId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.LargeIconFile ).WithMany().HasForeignKey( p => p.LargeIconFileId ).WillCascadeOnDelete( false );
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
