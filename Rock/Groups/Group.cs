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
    /// Group POCO Entity.
    /// </summary>
    [Table( "groupsGroup" )]
    public partial class Group : ModelWithAttributes<Group>, IAuditable
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
		/// Gets or sets the Parent Group Id.
		/// </summary>
		/// <value>
		/// Parent Group Id.
		/// </value>
		[DataMember]
		public int? ParentGroupId { get; set; }
		
		/// <summary>
		/// Gets or sets the Group Type Id.
		/// </summary>
		/// <value>
		/// Group Type Id.
		/// </value>
		[Required]
		[DataMember]
		public int GroupTypeId { get; set; }
		
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
		/// Gets or sets the Is Security Role.
		/// </summary>
		/// <value>
		/// Is Security Role.
		/// </value>
		[Required]
		[DataMember]
		public bool IsSecurityRole { get; set; }
		
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
		public override string AuthEntity { get { return "Groups.Group"; } }
        
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
		public virtual ICollection<Member> Members { get; set; }
        
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
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ParentGroup ).WithMany( p => p.Groups ).HasForeignKey( p => p.ParentGroupId ).WillCascadeOnDelete(false);
			this.HasRequired( p => p.GroupType ).WithMany( p => p.Groups ).HasForeignKey( p => p.GroupTypeId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class GroupDTO : DTO<Group>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
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
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
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
        public bool IsSecurityRole { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public GroupDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public GroupDTO( Group group )
        {
            CopyFromModel( group );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="group"></param>
        public override void CopyFromModel( Group group )
        {
            this.Id = group.Id;
            this.Guid = group.Guid;
            this.IsSystem = group.IsSystem;
            this.ParentGroupId = group.ParentGroupId;
            this.GroupTypeId = group.GroupTypeId;
            this.Name = group.Name;
            this.Description = group.Description;
            this.IsSecurityRole = group.IsSecurityRole;
            this.CreatedDateTime = group.CreatedDateTime;
            this.ModifiedDateTime = group.ModifiedDateTime;
            this.CreatedByPersonId = group.CreatedByPersonId;
            this.ModifiedByPersonId = group.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="group"></param>
        public override void CopyToModel( Group group )
        {
            group.Id = this.Id;
            group.Guid = this.Guid;
            group.IsSystem = this.IsSystem;
            group.ParentGroupId = this.ParentGroupId;
            group.GroupTypeId = this.GroupTypeId;
            group.Name = this.Name;
            group.Description = this.Description;
            group.IsSecurityRole = this.IsSecurityRole;
            group.CreatedDateTime = this.CreatedDateTime;
            group.ModifiedDateTime = this.ModifiedDateTime;
            group.CreatedByPersonId = this.CreatedByPersonId;
            group.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }
}
