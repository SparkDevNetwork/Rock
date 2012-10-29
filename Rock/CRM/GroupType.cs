//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Crm
{
    /// <summary>
    /// Group Type POCO Entity.
    /// </summary>
    [Table( "crmGroupType" )]
    [FriendlyTypeName( "Group Type" )]
    public partial class GroupType : Model<GroupType>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember]
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
        /// Gets or sets the Default Group Role Id.
        /// </summary>
        /// <value>
        /// Default Group Role Id.
        /// </value>
        [DataMember]
        public int? DefaultGroupRoleId { get; set; }

        /// <summary>
        /// Gets the auth entity.
        /// </summary>
        [NotMapped]
        public string DeprecatedEntityTypeName { get { return "Crm.GroupType"; } }

        /// <summary>
        /// Gets or sets the Groups.
        /// </summary>
        /// <value>
        /// Collection of Groups.
        /// </value>
        public virtual ICollection<Group> Groups { get; set; }

        /// <summary>
        /// Gets or sets the Child Group Types.
        /// </summary>
        /// <value>
        /// Collection of Child Group Types.
        /// </value>
        public virtual ICollection<GroupType> ChildGroupTypes { get; set; }

        /// <summary>
        /// Gets or sets the Parent Group Types.
        /// </summary>
        /// <value>
        /// Collection of Parent Group Types.
        /// </value>
        public virtual ICollection<GroupType> ParentGroupTypes { get; set; }

        /// <summary>
        /// Gets or sets the Group Roles.
        /// </summary>
        /// <value>
        /// Collection of Group Roles.
        /// </value>
        public virtual ICollection<GroupRole> Roles { get; set; }

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
        /// Gets or sets the Default Group Role.
        /// </summary>
        /// <value>
        /// A <see cref="GroupRole"/> object.
        /// </value>
        public virtual GroupRole DefaultGroupRole { get; set; }

        /// <summary>
        /// Gets or sets the location types.
        /// </summary>
        /// <value>
        /// The location types.
        /// </value>
        public virtual ICollection<GroupTypeLocationType> LocationTypes { get; set; }

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
            this.HasMany( p => p.ChildGroupTypes ).WithMany( c => c.ParentGroupTypes ).Map( m => { m.MapLeftKey( "GroupTypeId" ); m.MapRightKey( "ChildGroupTypeId" ); m.ToTable( "crmGroupTypeAssociation" ); } );
            this.HasOptional( p => p.DefaultGroupRole ).WithMany().HasForeignKey( p => p.DefaultGroupRoleId ).WillCascadeOnDelete( false );
        }
    }
}
