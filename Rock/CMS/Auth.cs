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
using System.Text;

using Rock;
using Rock.Data;

namespace Rock.Cms
{
    /// <summary>
    /// Auth POCO Entity.
    /// </summary>
    [Table( "cmsAuth" )]
    public partial class Auth : Model<Auth>, IOrdered
    {
        /// <summary>
        /// Gets or sets the Entity Type Id.
        /// </summary>
        /// <value>
        /// Entity Type Id.
        /// </value>
        [Required]
        public int EntityTypeId { get; set; }
        
        /// <summary>
        /// Gets or sets the Entity Id.
        /// </summary>
        /// <value>
        /// Entity Id.
        /// </value>
        public int? EntityId { get; set; }
        
        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        [Required]
        public int Order { get; set; }
        
        /// <summary>
        /// Gets or sets the Action.
        /// </summary>
        /// <value>
        /// Action.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        public string Action { get; set; }
        
        /// <summary>
        /// Gets or sets the Allow Or Deny.
        /// </summary>
        /// <value>
        /// A = Allow, D = Deny.
        /// </value>
        [Required]
        [MaxLength( 1 )]
        public string AllowOrDeny { get; set; }
        
        /// <summary>
        /// Gets or sets the Special Role.
        /// </summary>
        /// <value>
        /// Enum[SpecialRole].
        /// </value>
        [Required]
        public SpecialRole SpecialRole { get; set; }

        /// <summary>
        /// Gets or sets the Person Id.
        /// </summary>
        /// <value>
        /// Person Id.
        /// </value>
        public int? PersonId { get; set; }
        
        /// <summary>
        /// Gets or sets the Group Id.
        /// </summary>
        /// <value>
        /// Group Id.
        /// </value>
        public int? GroupId { get; set; }
        
        /// <summary>
        /// Gets or sets the Group.
        /// </summary>
        /// <value>
        /// A <see cref="Crm.Group"/> object.
        /// </value>
        public virtual Crm.Group Group { get; set; }
        
        /// <summary>
        /// Gets or sets the Person.
        /// </summary>
        /// <value>
        /// A <see cref="Crm.Person"/> object.
        /// </value>
        public virtual Crm.Person Person { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public virtual Core.EntityType EntityType { get; set; }

        /// <summary>
        /// The default authorization for a specific action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public override bool IsAllowedByDefault( string action )
        {
            return false;
        }

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static Auth Read( int id )
        {
            return Read<Auth>( id );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0} ", this.AllowOrDeny == "A" ? "Allow" : "Deny");

            if (SpecialRole != Cms.SpecialRole.None)
                sb.AppendFormat("{0} ", SpecialRole.ToString().SplitCase());
            else if(PersonId.HasValue)
                sb.AppendFormat("{0} ", Person.ToString());
            else if(GroupId.HasValue)
                sb.AppendFormat("{0} ", Group.ToString());

            sb.AppendFormat("{0} Access", Action);

            return sb.ToString();
        }
    }

    /// <summary>
    /// Auth Configuration class.
    /// </summary>
    public partial class AuthConfiguration : EntityTypeConfiguration<Auth>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthConfiguration"/> class.
        /// </summary>
        public AuthConfiguration()
        {
            this.HasOptional( p => p.Group ).WithMany().HasForeignKey( p => p.GroupId ).WillCascadeOnDelete(true);
            this.HasOptional( p => p.Person ).WithMany().HasForeignKey( p => p.PersonId ).WillCascadeOnDelete(true);
            this.HasRequired( p => p.EntityType ).WithMany().HasForeignKey( p => p.EntityTypeId).WillCascadeOnDelete( false );
        }
    }

    /// <summary>
    /// Authorization for a special group of users not defined by a specific role or person
    /// </summary>
    public enum SpecialRole
    {
        /// <summary>
        /// No special role
        /// </summary>
        None = 0,

        /// <summary>
        /// Authorize all users
        /// </summary>
        AllUsers = 1,

        /// <summary>
        /// Authorize all authenticated users
        /// </summary>
        AllAuthenticatedUsers = 2,

        /// <summary>
        /// Authorize all un-authenticated users
        /// </summary>
        AllUnAuthenticatedUsers = 3,
    }

}
