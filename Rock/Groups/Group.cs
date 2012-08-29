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
        /// A <see cref="Crm.Person"/> object.
        /// </value>
		public virtual Crm.Person CreatedByPerson { get; set; }
        
		/// <summary>
        /// Gets or sets the Modified By Person.
        /// </summary>
        /// <value>
        /// A <see cref="Crm.Person"/> object.
        /// </value>
		public virtual Crm.Person ModifiedByPerson { get; set; }
        
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
}
