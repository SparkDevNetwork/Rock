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
    /// Block Instance POCO Entity.
    /// </summary>
    [Table( "cmsBlockInstance" )]
    public partial class BlockInstance : ModelWithAttributes<BlockInstance>, IAuditable, IOrdered
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
		/// Gets or sets the Page Id.
		/// </summary>
		/// <value>
		/// Page Id.
		/// </value>
		[DataMember]
		public int? PageId { get; set; }
		
		/// <summary>
		/// Gets or sets the Layout.
		/// </summary>
		/// <value>
		/// Layout.
		/// </value>
		[MaxLength( 100 )]
		[DataMember]
		public string Layout { get; set; }
		
		/// <summary>
		/// Gets or sets the Block Id.
		/// </summary>
		/// <value>
		/// Block Id.
		/// </value>
		[Required]
		[DataMember]
		public int BlockId { get; set; }
		
		/// <summary>
		/// Gets or sets the Zone.
		/// </summary>
		/// <value>
		/// Zone.
		/// </value>
		[Required]
		[MaxLength( 100 )]
		[DataMember]
		public string Zone { get; set; }
		
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
		/// Gets or sets the Name.
		/// </summary>
		/// <value>
		/// Name.
		/// </value>
		[MaxLength( 100 )]
		[TrackChanges]
		[Required( ErrorMessage = "Name is required" )]
		[DataMember]
		public string Name { get; set; }
		
		/// <summary>
		/// Gets or sets the Output Cache Duration.
		/// </summary>
		/// <value>
		/// Output Cache Duration.
		/// </value>
		[Required]
		[DataMember]
		public int OutputCacheDuration { get; set; }
		
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
		public override string AuthEntity { get { return "CMS.BlockInstance"; } }
        
		/// <summary>
        /// Gets or sets the Html Contents.
        /// </summary>
        /// <value>
        /// Collection of Html Contents.
        /// </value>
		public virtual ICollection<HtmlContent> HtmlContents { get; set; }
        
		/// <summary>
        /// Gets or sets the Block.
        /// </summary>
        /// <value>
        /// A <see cref="Block"/> object.
        /// </value>
		public virtual Block Block { get; set; }
        
		/// <summary>
        /// Gets or sets the Page.
        /// </summary>
        /// <value>
        /// A <see cref="Page"/> object.
        /// </value>
		public virtual Page Page { get; set; }
        
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
        /// Gets the supported actions.
        /// </summary>
        public override List<string> SupportedActions
        {
            get { return new List<string>() { "View", "Edit", "Configure" }; }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }
    }

    /// <summary>
    /// Block Instance Configuration class.
    /// </summary>
    public partial class BlockInstanceConfiguration : EntityTypeConfiguration<BlockInstance>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockInstanceConfiguration"/> class.
        /// </summary>
        public BlockInstanceConfiguration()
        {
			this.HasRequired( p => p.Block ).WithMany( p => p.BlockInstances ).HasForeignKey( p => p.BlockId ).WillCascadeOnDelete(true);
			this.HasOptional( p => p.Page ).WithMany( p => p.BlockInstances ).HasForeignKey( p => p.PageId ).WillCascadeOnDelete(true);
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class BlockInstanceDTO : DTO<BlockInstance>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Page Id.
        /// </summary>
        /// <value>
        /// Page Id.
        /// </value>
        public int? PageId { get; set; }

        /// <summary>
        /// Gets or sets the Layout.
        /// </summary>
        /// <value>
        /// Layout.
        /// </value>
        public string Layout { get; set; }

        /// <summary>
        /// Gets or sets the Block Id.
        /// </summary>
        /// <value>
        /// Block Id.
        /// </value>
        public int BlockId { get; set; }

        /// <summary>
        /// Gets or sets the Zone.
        /// </summary>
        /// <value>
        /// Zone.
        /// </value>
        public string Zone { get; set; }

        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Output Cache Duration.
        /// </summary>
        /// <value>
        /// Output Cache Duration.
        /// </value>
        public int OutputCacheDuration { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public BlockInstanceDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public BlockInstanceDTO( BlockInstance blockInstance )
        {
            CopyFromModel( blockInstance );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="blockInstance"></param>
        public override void CopyFromModel( BlockInstance blockInstance )
        {
            this.Id = blockInstance.Id;
            this.Guid = blockInstance.Guid;
            this.IsSystem = blockInstance.IsSystem;
            this.PageId = blockInstance.PageId;
            this.Layout = blockInstance.Layout;
            this.BlockId = blockInstance.BlockId;
            this.Zone = blockInstance.Zone;
            this.Order = blockInstance.Order;
            this.Name = blockInstance.Name;
            this.OutputCacheDuration = blockInstance.OutputCacheDuration;
            this.CreatedDateTime = blockInstance.CreatedDateTime;
            this.ModifiedDateTime = blockInstance.ModifiedDateTime;
            this.CreatedByPersonId = blockInstance.CreatedByPersonId;
            this.ModifiedByPersonId = blockInstance.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="blockInstance"></param>
        public override void CopyToModel( BlockInstance blockInstance )
        {
            blockInstance.Id = this.Id;
            blockInstance.Guid = this.Guid;
            blockInstance.IsSystem = this.IsSystem;
            blockInstance.PageId = this.PageId;
            blockInstance.Layout = this.Layout;
            blockInstance.BlockId = this.BlockId;
            blockInstance.Zone = this.Zone;
            blockInstance.Order = this.Order;
            blockInstance.Name = this.Name;
            blockInstance.OutputCacheDuration = this.OutputCacheDuration;
            blockInstance.CreatedDateTime = this.CreatedDateTime;
            blockInstance.ModifiedDateTime = this.ModifiedDateTime;
            blockInstance.CreatedByPersonId = this.CreatedByPersonId;
            blockInstance.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }
}
