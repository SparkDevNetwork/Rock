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
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.CMS
{
    /// <summary>
    /// Auth POCO Entity.
    /// </summary>
    [Table( "cmsAuth" )]
    public partial class Auth : ModelWithAttributes<Auth>, IAuditable, IOrdered
    {
		/// <summary>
		/// Gets or sets the Entity Type.
		/// </summary>
		/// <value>
		/// Entity Type.
		/// </value>
		[Required]
		[MaxLength( 200 )]
		[DataMember]
		public string EntityType { get; set; }
		
		/// <summary>
		/// Gets or sets the Entity Id.
		/// </summary>
		/// <value>
		/// Entity Id.
		/// </value>
		[DataMember]
		public int? EntityId { get; set; }
		
		/// <summary>
		/// Gets or sets the Order.
		/// </summary>
		/// <value>
		/// Order.
		/// </value>
		[Required]
		[DataMember]
		public int Order { get; set; }
		
		/// <summary>
		/// Gets or sets the Action.
		/// </summary>
		/// <value>
		/// Action.
		/// </value>
		[Required]
		[MaxLength( 50 )]
		[DataMember]
		public string Action { get; set; }
		
		/// <summary>
		/// Gets or sets the Allow Or Deny.
		/// </summary>
		/// <value>
		/// A = Allow, D = Deny.
		/// </value>
		[Required]
		[MaxLength( 1 )]
		[DataMember]
		public string AllowOrDeny { get; set; }
		
		/// <summary>
		/// Gets or sets the Special Role.
		/// </summary>
		/// <value>
		/// Enum[SpecialRole].
		/// </value>
		[Required]
		[DataMember]
		public SpecialRole SpecialRole { get; set; }

		/// <summary>
		/// Gets or sets the Person Id.
		/// </summary>
		/// <value>
		/// Person Id.
		/// </value>
		[DataMember]
		public int? PersonId { get; set; }
		
		/// <summary>
		/// Gets or sets the Group Id.
		/// </summary>
		/// <value>
		/// Group Id.
		/// </value>
		[DataMember]
		public int? GroupId { get; set; }
		
		/// <summary>
		/// Gets or sets the Created Date Time.
		/// </summary>
		/// <value>
		/// Created Date Time.
		/// </value>
		[DataMember]
		public DateTime? CreatedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Modified Date Time.
		/// </summary>
		/// <value>
		/// Modified Date Time.
		/// </value>
		[DataMember]
		public DateTime? ModifiedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Created By Person Id.
		/// </summary>
		/// <value>
		/// Created By Person Id.
		/// </value>
		[DataMember]
		public int? CreatedByPersonId { get; set; }
		
		/// <summary>
		/// Gets or sets the Modified By Person Id.
		/// </summary>
		/// <value>
		/// Modified By Person Id.
		/// </value>
		[DataMember]
		public int? ModifiedByPersonId { get; set; }

        /// <summary>
        /// Gets the auth entity.
        /// </summary>
		[NotMapped]
		public override string AuthEntity { get { return "CMS.Auth"; } }
        
		/// <summary>
        /// Gets or sets the Created By Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person CreatedByPerson { get; set; }
        
		/// <summary>
        /// Gets or sets the Modified By Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person ModifiedByPerson { get; set; }
        
		/// <summary>
        /// Gets or sets the Group.
        /// </summary>
        /// <value>
        /// A <see cref="Groups.Group"/> object.
        /// </value>
		public virtual Groups.Group Group { get; set; }
        
		/// <summary>
        /// Gets or sets the Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person Person { get; set; }

        /// <summary>
        /// The default authorization for a specific action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public override bool IsAllowedByDefault( string action )
        {
            return false;
        }

    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class AuthDTO : DTO<Auth>
    {
        /// <summary>
        /// Gets or sets the Entity Type.
        /// </summary>
        /// <value>
        /// Entity Type.
        /// </value>
        public string EntityType { get; set; }

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
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the Action.
        /// </summary>
        /// <value>
        /// Action.
        /// </value>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the Allow Or Deny.
        /// </summary>
        /// <value>
        /// Allow Or Deny.
        /// </value>
        public string AllowOrDeny { get; set; }

        /// <summary>
        /// Gets or sets the Special Role.
        /// </summary>
        /// <value>
        /// Special Role.
        /// </value>
        public int SpecialRole { get; set; }

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
        /// Instantiate new DTO object
        /// </summary>
        public AuthDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public AuthDTO( Auth auth )
        {
            CopyFromModel( auth );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="page"></param>
        public override void CopyFromModel( Auth auth )
        {
            this.Id = auth.Id;
            this.Guid = auth.Guid;
            this.EntityType = auth.EntityType;
            this.EntityId = auth.EntityId;
            this.Order = auth.Order;
            this.Action = auth.Action;
            this.AllowOrDeny = auth.AllowOrDeny;
            this.SpecialRole = ( int )auth.SpecialRole;
            this.PersonId = auth.PersonId;
            this.GroupId = auth.GroupId;
            this.CreatedDateTime = auth.CreatedDateTime;
            this.ModifiedDateTime = auth.ModifiedDateTime;
            this.CreatedByPersonId = auth.CreatedByPersonId;
            this.ModifiedByPersonId = auth.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="page"></param>
        public override void CopyToModel( Auth auth )
        {
            auth.Id = this.Id;
            auth.Guid = this.Guid;
            auth.EntityType = this.EntityType;
            auth.EntityId = this.EntityId;
            auth.Order = this.Order;
            auth.Action = this.Action;
            auth.AllowOrDeny = this.AllowOrDeny;
            auth.SpecialRole = ( SpecialRole )this.SpecialRole;
            auth.PersonId = this.PersonId;
            auth.GroupId = this.GroupId;
            auth.CreatedDateTime = this.CreatedDateTime;
            auth.ModifiedDateTime = this.ModifiedDateTime;
            auth.CreatedByPersonId = this.CreatedByPersonId;
            auth.ModifiedByPersonId = this.ModifiedByPersonId;
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
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.Group ).WithMany().HasForeignKey( p => p.GroupId ).WillCascadeOnDelete(true);
			this.HasOptional( p => p.Person ).WithMany().HasForeignKey( p => p.PersonId ).WillCascadeOnDelete(true);
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
