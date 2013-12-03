//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using System.Text;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a user or group's security authorization to perform a specified action on a securable entity in RockChMS. Authorization can either be allowed or denied and local (to the entity) security will override
    /// the parent (inherited) authority. Order of Auth's does matter. The first Auth for a specific action on an entity that the user qualifies for determines if they are allowed or denied.
    /// </summary>
    [Table( "Auth" )]
    [DataContract]
    public partial class Auth : Model<Auth>, IOrdered
    {
        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> for the entity that this Auth object applies to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> of the entity that this Auth object applies to.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityTypeId { get; set; }
        
        /// <summary>
        /// Gets or sets the EntityId of the entity that this Auth entity applies to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityId of the entity that this Auth entity applies to.
        /// </value>
        [DataMember]
        public int? EntityId { get; set; }
        
        /// <summary>
        /// Gets or sets the order or priority of the Auth entity. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the order of the Auth entity.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }
        
        /// <summary>
        /// Gets or sets the name of action that this Auth entity covers (i.e. view, edit, administrate, etc.).
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the action that is covered by this Auth entity.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Action { get; set; }
        
        /// <summary>
        /// Gets or sets a flag indicating if this Auth entity allows or denies this action for the role.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> value that equals A for allow and D for deny.
        /// </value>
        [Required]
        [MaxLength( 1 )]
        [DataMember( IsRequired = true )]
        public string AllowOrDeny { get; set; }
        
        /// <summary>
        /// Gets or sets the special role that this Auth entity applies to.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.SpecialRole"/> enum indicating the special role that this Auth entity applies to.
        /// If this Auth entity does not apply to any special role then the value will be <c>SpecialRole.None</c> or (0); 
        /// If this Auth entity applies to All Users (authenticated and unauthenticated) then the value will be <c>SpecialRole.AllUsers</c> or 1;
        /// If this Auth entity applies to All Authenticated Users then the value will be <c>SpecialRole.AllAuthenticatedUsers</c> or 2;
        /// If this Auth entity applies to All Un-authenticated Users then this value will be <c>SpecialRole.AllUnAuthenticatedUsers</c> or 3.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public SpecialRole SpecialRole { get; set; }

        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> that this Auth entity allows or denies access to. This is used for user based authorization
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> that this Auth entity allows or denies access to. This applies to user/person based authorization.
        /// If user/person based Authorization is not used this value will be null.
        /// </value>
        [DataMember]
        public int? PersonId { get; set; }
        
        /// <summary>
        /// Gets or sets the GroupId of the Security Role <see cref="Rock.Model.Group"/> that this Auth entity allows or denies access to. This is used for group based authorization.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the GroupId of the <see cref="Rock.Model.Person" /> that this Auth entity allows or denies access to. This applies to group based authorization.
        /// If group based Authorization is not used this value will be null.
        /// </value>
        [DataMember]
        public int? GroupId { get; set; }
        
        /// <summary>
        /// Gets or sets the Security Role <see cref="Rock.Model.Group"/> that this Auth entity allows or denies access to. This is used for Group based authorization. 
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Group"/> that this Auth entity allows or denies access to. If group based authorization is not used, this value will be null.
        /// </value>
        [DataMember]
        public virtual Model.Group Group { get; set; }
        
        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> that this Auth entity allows or denies access to. This is used for Person based authorization.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Person"/> that this Auth entity allows or denies access to. If person based authorization is not used, this value will be null.
        /// </value>
        [DataMember]
        public virtual Model.Person Person { get; set; }

        /// <summary>
        /// Gets or sets the the <see cref="Rock.Model.EntityType"/> of the entity that is being secured.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> of of the entity that is being secured.
        /// </value>
        [DataMember]
        public virtual Model.EntityType EntityType { get; set; }

        /// <summary>
        /// Returns the default authorization for a specific action.
        /// </summary>
        /// <param name="action">A <see cref="System.String"/> representing the name of the action.</param>
        /// <returns>A <see cref="System.Boolean"/> that is <c>true</c> if the specified action is allowed by default; otherwise <c>false</c>.</returns>
        public override bool IsAllowedByDefault( string action )
        {
            return false;
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

            if (SpecialRole != Model.SpecialRole.None)
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
