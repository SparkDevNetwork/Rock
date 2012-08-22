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
    /// Group Role POCO Entity.
    /// </summary>
    [Table( "groupsGroupRole" )]
    public partial class GroupRole : ModelWithAttributes<GroupRole>, IAuditable
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
		[MaxLength( 100 )]
		[DataMember]
		public string Name { get; set; }
		
		/// <summary>
		/// Gets or sets the Description.
		/// </summary>
		/// <value>
		/// Description.
		/// </value>
		[Required]
		[DataMember]
		public string Description { get; set; }
		
		/// <summary>
		/// Gets or sets the Order.
		/// </summary>
		/// <value>
		/// Order.
		/// </value>
		[DataMember]
		public int? Order { get; set; }
		
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
		public override string AuthEntity { get { return "Groups.GroupRole"; } }
        
		/// <summary>
        /// Gets or sets the Group Types.
        /// </summary>
        /// <value>
        /// Collection of Group Types.
        /// </value>
		public virtual ICollection<GroupType> GroupTypes { get; set; }
        
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

    }

    /// <summary>
    /// Group Role Configuration class.
    /// </summary>
    public partial class GroupRoleConfiguration : EntityTypeConfiguration<GroupRole>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupRoleConfiguration"/> class.
        /// </summary>
        public GroupRoleConfiguration()
        {
			this.HasMany( p => p.GroupTypes ).WithMany( c => c.GroupRoles ).Map( m => { m.MapLeftKey("GroupTypeId"); m.MapRightKey("GroupRoleId"); m.ToTable("groupsGroupTypeRole" ); } );
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class GroupRoleDTO : DTO<GroupRole>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        public bool IsSystem { get; set; }

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
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        public int? Order { get; set; }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public GroupRoleDTO( GroupRole groupRole )
        {
            CopyFromModel( groupRole );
        }

         /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public GroupRoleDTO()
        {
        }

       /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="groupRole"></param>
        public override void CopyFromModel( GroupRole groupRole )
        {
            this.Id = groupRole.Id;
            this.Guid = groupRole.Guid;
            this.IsSystem = groupRole.IsSystem;
            this.Name = groupRole.Name;
            this.Description = groupRole.Description;
            this.Order = groupRole.Order;
            this.CreatedDateTime = groupRole.CreatedDateTime;
            this.ModifiedDateTime = groupRole.ModifiedDateTime;
            this.CreatedByPersonId = groupRole.CreatedByPersonId;
            this.ModifiedByPersonId = groupRole.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="groupRole"></param>
        public override void CopyToModel( GroupRole groupRole )
        {
            groupRole.Id = this.Id;
            groupRole.Guid = this.Guid;
            groupRole.IsSystem = this.IsSystem;
            groupRole.Name = this.Name;
            groupRole.Description = this.Description;
            groupRole.Order = this.Order;
            groupRole.CreatedDateTime = this.CreatedDateTime;
            groupRole.ModifiedDateTime = this.ModifiedDateTime;
            groupRole.CreatedByPersonId = this.CreatedByPersonId;
            groupRole.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }
}
