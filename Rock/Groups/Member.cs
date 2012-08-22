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

namespace Rock.Groups
{
    /// <summary>
    /// Member POCO Entity.
    /// </summary>
    [Table( "groupsMember" )]
    public partial class Member : ModelWithAttributes<Member>, IAuditable
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
		/// Gets or sets the Group Id.
		/// </summary>
		/// <value>
		/// Group Id.
		/// </value>
		[Required]
		[DataMember]
		public int GroupId { get; set; }
		
		/// <summary>
		/// Gets or sets the Person Id.
		/// </summary>
		/// <value>
		/// Person Id.
		/// </value>
		[Required]
		[DataMember]
		public int PersonId { get; set; }
		
		/// <summary>
		/// Gets or sets the Group Role Id.
		/// </summary>
		/// <value>
		/// Group Role Id.
		/// </value>
		[Required]
		[DataMember]
		public int GroupRoleId { get; set; }
		
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
		public override string AuthEntity { get { return "Groups.Member"; } }
        
		/// <summary>
        /// Gets or sets the Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person Person { get; set; }
        
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

    }

    /// <summary>
    /// Member Configuration class.
    /// </summary>
    public partial class MemberConfiguration : EntityTypeConfiguration<Member>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberConfiguration"/> class.
        /// </summary>
        public MemberConfiguration()
        {
			this.HasRequired( p => p.Person ).WithMany( p => p.Members ).HasForeignKey( p => p.PersonId ).WillCascadeOnDelete(true);
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
			this.HasRequired( p => p.Group ).WithMany( p => p.Members ).HasForeignKey( p => p.GroupId ).WillCascadeOnDelete(true);
			this.HasRequired( p => p.GroupRole ).WithMany( p => p.Members ).HasForeignKey( p => p.GroupRoleId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class MemberDTO : DTO<Member>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Group Id.
        /// </summary>
        /// <value>
        /// Group Id.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the Person Id.
        /// </summary>
        /// <value>
        /// Person Id.
        /// </value>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the Group Role Id.
        /// </summary>
        /// <value>
        /// Group Role Id.
        /// </value>
        public int GroupRoleId { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public MemberDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public MemberDTO( Member member )
        {
            CopyFromModel( member );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="member"></param>
        public override void CopyFromModel( Member member )
        {
            this.Id = member.Id;
            this.Guid = member.Guid;
            this.IsSystem = member.IsSystem;
            this.GroupId = member.GroupId;
            this.PersonId = member.PersonId;
            this.GroupRoleId = member.GroupRoleId;
            this.CreatedDateTime = member.CreatedDateTime;
            this.ModifiedDateTime = member.ModifiedDateTime;
            this.CreatedByPersonId = member.CreatedByPersonId;
            this.ModifiedByPersonId = member.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="member"></param>
        public override void CopyToModel( Member member )
        {
            member.Id = this.Id;
            member.Guid = this.Guid;
            member.IsSystem = this.IsSystem;
            member.GroupId = this.GroupId;
            member.PersonId = this.PersonId;
            member.GroupRoleId = this.GroupRoleId;
            member.CreatedDateTime = this.CreatedDateTime;
            member.ModifiedDateTime = this.ModifiedDateTime;
            member.CreatedByPersonId = this.CreatedByPersonId;
            member.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }
}
