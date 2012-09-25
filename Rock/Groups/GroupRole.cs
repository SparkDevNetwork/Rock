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
		/// Gets or sets the Group Type Id.
		/// </summary>
		/// <value>
		/// Group Type Id.
		/// </value>
		[DataMember]
		public int? GroupTypeId { get; set; }

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
		/// Static Method to return an object based on the id
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		public static GroupRole Read( int id )
		{
			return Read<GroupRole>( id );
		}

        /// <summary>
        /// Gets the auth entity.
        /// </summary>
		[NotMapped]
		public override string AuthEntity { get { return "Groups.GroupRole"; } }

		/// <summary>
		/// Gets or sets the Group Type.
		/// </summary>
		/// <value>
		/// A <see cref="GroupType"/> object.
		/// </value>
		public virtual GroupType GroupType { get; set; }

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
			this.HasRequired( p => p.GroupType ).WithMany( p => p.Roles ).HasForeignKey( p => p.GroupTypeId ).WillCascadeOnDelete( true );
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete( false );
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }
}
