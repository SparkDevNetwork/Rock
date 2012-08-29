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

namespace Rock.Core
{
    /// <summary>
    /// Entity Change POCO Entity.
    /// </summary>
    [Table( "coreEntityChange" )]
    public partial class EntityChange : ModelWithAttributes<EntityChange>
    {
		/// <summary>
		/// Gets or sets the Change Set.
		/// </summary>
		/// <value>
		/// Change Set.
		/// </value>
		[Required]
		[DataMember]
		public Guid ChangeSet { get; set; }
		
		/// <summary>
		/// Gets or sets the Change Type.
		/// </summary>
		/// <value>
		/// Change Type.
		/// </value>
		[Required]
		[MaxLength( 10 )]
		[DataMember]
		public string ChangeType { get; set; }
		
		/// <summary>
		/// Gets or sets the Entity Type.
		/// </summary>
		/// <value>
		/// Entity Type.
		/// </value>
		[Required]
		[MaxLength( 100 )]
		[DataMember]
		public string EntityType { get; set; }
		
		/// <summary>
		/// Gets or sets the Entity Id.
		/// </summary>
		/// <value>
		/// Entity Id.
		/// </value>
		[Required]
		[DataMember]
		public int EntityId { get; set; }
		
		/// <summary>
		/// Gets or sets the Property.
		/// </summary>
		/// <value>
		/// Property.
		/// </value>
		[Required]
		[MaxLength( 100 )]
		[DataMember]
		public string Property { get; set; }
		
		/// <summary>
		/// Gets or sets the Original Value.
		/// </summary>
		/// <value>
		/// Original Value.
		/// </value>
		[DataMember]
		public string OriginalValue { get; set; }
		
		/// <summary>
		/// Gets or sets the Current Value.
		/// </summary>
		/// <value>
		/// Current Value.
		/// </value>
		[DataMember]
		public string CurrentValue { get; set; }
		
		/// <summary>
		/// Gets or sets the Created Date Time.
		/// </summary>
		/// <value>
		/// Created Date Time.
		/// </value>
		[DataMember]
		public DateTime? CreatedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Created By Person Id.
		/// </summary>
		/// <value>
		/// Created By Person Id.
		/// </value>
		[DataMember]
		public int? CreatedByPersonId { get; set; }
		
        /// <summary>
        /// Gets the auth entity.
        /// </summary>
		[NotMapped]
		public override string AuthEntity { get { return "Core.EntityChange"; } }
        
		/// <summary>
        /// Gets or sets the Created By Person.
        /// </summary>
        /// <value>
        /// A <see cref="Crm.Person"/> object.
        /// </value>
		public virtual Crm.Person CreatedByPerson { get; set; }

    }
    
    /// <summary>
    /// Entity Change Configuration class.
    /// </summary>
    public partial class EntityChangeConfiguration : EntityTypeConfiguration<EntityChange>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityChangeConfiguration"/> class.
        /// </summary>
        public EntityChangeConfiguration()
        {
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
		}
    }
}
