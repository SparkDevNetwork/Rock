//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Rock.Data;

namespace Rock.Crm    
{
    /// <summary>
    /// Group POCO Entity.
    /// </summary>
    [Table( "Group" )]
    public partial class Group : Model<Group>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the Parent Group Id.
        /// </summary>
        /// <value>
        /// Parent Group Id.
        /// </value>
        public int? ParentGroupId { get; set; }
        
        /// <summary>
        /// Gets or sets the Group Type Id.
        /// </summary>
        /// <value>
        /// Group Type Id.
        /// </value>
        [Required]
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Campus Id.
        /// </summary>
        /// <value>
        /// Campus Id.
        /// </value>
        public int? CampusId { get; set; }

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
        /// Gets or sets the Is Security Role.
        /// </summary>
        /// <value>
        /// Is Security Role.
        /// </value>
        [Required]
        public bool IsSecurityRole { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [Required]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        public override IDto Dto
        {
            get { return this.ToDto(); }
        }

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static Group Read( int id )
        {
            return Read<Group>( id );
        }
        
        /// <summary>
        /// Gets or sets the Groups.
        /// </summary>
        /// <value>
        /// Collection of Groups.
        /// </value>
        public virtual ICollection<Group> Groups { get; set; }
        
        /// <summary>
        /// Gets or sets the Members.
        /// </summary>
        /// <value>
        /// Collection of Members.
        /// </value>
        public virtual ICollection<GroupMember> Members { get; set; }

        /// <summary>
        /// Gets or sets the Locations.
        /// </summary>
        /// <value>
        /// Collection of Locations.
        /// </value>
        public virtual ICollection<GroupLocation> Locations { get; set; }

        /// <summary>
        /// Gets or sets the Parent Group.
        /// </summary>
        /// <value>
        /// A <see cref="Group"/> object.
        /// </value>
        public virtual Group ParentGroup { get; set; }
        
        /// <summary>
        /// Gets or sets the Group Type.
        /// </summary>
        /// <value>
        /// A <see cref="GroupType"/> object.
        /// </value>
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// Gets or sets the Campus.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Crm.Campus"/> object.
        /// </value>
        public virtual Rock.Crm.Campus Campus { get; set; }

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
    /// Group Configuration class.
    /// </summary>
    public partial class GroupConfiguration : EntityTypeConfiguration<Group>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupConfiguration"/> class.
        /// </summary>
        public GroupConfiguration()
        {
            this.HasOptional( p => p.ParentGroup ).WithMany( p => p.Groups ).HasForeignKey( p => p.ParentGroupId ).WillCascadeOnDelete(false);
            this.HasRequired( p => p.GroupType ).WithMany( p => p.Groups ).HasForeignKey( p => p.GroupTypeId ).WillCascadeOnDelete(false);
            this.HasOptional( p => p.Campus ).WithMany().HasForeignKey( p => p.CampusId).WillCascadeOnDelete( false );
        }
    }
}
