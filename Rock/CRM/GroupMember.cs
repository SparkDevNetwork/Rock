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
    /// Member POCO Entity.
    /// </summary>
    [Table( "crmGroupMember" )]
    public partial class GroupMember : Model<GroupMember>
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
        /// Gets or sets the Group Id.
        /// </summary>
        /// <value>
        /// Group Id.
        /// </value>
        [Required]
        public int GroupId { get; set; }
        
        /// <summary>
        /// Gets or sets the Person Id.
        /// </summary>
        /// <value>
        /// Person Id.
        /// </value>
        [Required]
        public int PersonId { get; set; }
        
        /// <summary>
        /// Gets or sets the Group Role Id.
        /// </summary>
        /// <value>
        /// Group Role Id.
        /// </value>
        [Required]
        public int GroupRoleId { get; set; }
        
        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static GroupMember Read( int id )
        {
            return Read<GroupMember>( id );
        }
        
        /// <summary>
        /// Gets or sets the Person.
        /// </summary>
        /// <value>
        /// A <see cref="Crm.Person"/> object.
        /// </value>
        public virtual Crm.Person Person { get; set; }
        
        /// <summary>
        /// Gets or sets the Group.
        /// </summary>
        /// <value>
        /// A <see cref="Group"/> object.
        /// </value>
        public virtual Group Group { get; set; }
        
        /// <summary>
        /// Gets or sets the Group Role.
        /// </summary>
        /// <value>
        /// A <see cref="GroupRole"/> object.
        /// </value>
        public virtual GroupRole GroupRole { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( Person != null )
                return Person.FullName;
            return string.Empty;
        }
    }

    /// <summary>
    /// Member Configuration class.
    /// </summary>
    public partial class MemberConfiguration : EntityTypeConfiguration<GroupMember>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberConfiguration"/> class.
        /// </summary>
        public MemberConfiguration()
        {
            this.HasRequired( p => p.Person ).WithMany( p => p.Members ).HasForeignKey( p => p.PersonId ).WillCascadeOnDelete(true);
            this.HasRequired( p => p.Group ).WithMany( p => p.Members ).HasForeignKey( p => p.GroupId ).WillCascadeOnDelete(true);
            this.HasRequired( p => p.GroupRole ).WithMany().HasForeignKey( p => p.GroupRoleId ).WillCascadeOnDelete(false);
        }
    }
}
