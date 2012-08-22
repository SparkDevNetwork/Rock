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
    /// Block POCO Entity.
    /// </summary>
    [Table( "cmsBlock" )]
    public partial class Block : ModelWithAttributes<Block>, IAuditable
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
		/// Gets or sets the Path.
		/// </summary>
		/// <value>
		/// Path.
		/// </value>
		[Required]
		[MaxLength( 200 )]
		[DataMember]
		public string Path { get; set; }
		
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
		public override string AuthEntity { get { return "CMS.Block"; } }
        
		/// <summary>
        /// Gets or sets the Block Instances.
        /// </summary>
        /// <value>
        /// Collection of Block Instances.
        /// </value>
		public virtual ICollection<BlockInstance> BlockInstances { get; set; }
        
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
    /// Block Configuration class.
    /// </summary>
    public partial class BlockConfiguration : EntityTypeConfiguration<Block>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockConfiguration"/> class.
        /// </summary>
        public BlockConfiguration()
        {
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class BlockDTO : DTO<Block>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Path.
        /// </summary>
        /// <value>
        /// Path.
        /// </value>
        public string Path { get; set; }

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
        /// Instantiate new DTO object
        /// </summary>
        public BlockDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public BlockDTO( Block block )
        {
            CopyFromModel( block );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="block"></param>
        public override void CopyFromModel( Block block )
        {
            this.Id = block.Id;
            this.Guid = block.Guid;
            this.IsSystem = block.IsSystem;
            this.Path = block.Path;
            this.Name = block.Name;
            this.Description = block.Description;
            this.CreatedDateTime = block.CreatedDateTime;
            this.ModifiedDateTime = block.ModifiedDateTime;
            this.CreatedByPersonId = block.CreatedByPersonId;
            this.ModifiedByPersonId = block.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="block"></param>
        public override void CopyToModel( Block block )
        {
            block.Id = this.Id;
            block.Guid = this.Guid;
            block.IsSystem = this.IsSystem;
            block.Path = this.Path;
            block.Name = this.Name;
            block.Description = this.Description;
            block.CreatedDateTime = this.CreatedDateTime;
            block.ModifiedDateTime = this.ModifiedDateTime;
            block.CreatedByPersonId = this.CreatedByPersonId;
            block.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }
}
