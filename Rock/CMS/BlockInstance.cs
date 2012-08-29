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
    public partial class BlockInstance : ModelWithAttributes<BlockInstance>, IAuditable, IOrdered, IExportable
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

        public string ExportJson()
        {
            return ExportObject().ToJSON();
        }

        public object ExportObject()
        {
            dynamic exportObject = this.ToDynamic();

            if (Block != null)
            {
                exportObject.Block = Block.ExportObject();
            }

            if (HtmlContents == null)
            {
                return exportObject;
            }

            exportObject.HtmlContents = new List<dynamic>();

            foreach (var content in HtmlContents)
            {
                exportObject.HtmlContents.Add( content.ExportObject() );
            }

            return exportObject;
        }

        public void ImportJson(string data)
        {
            
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
}
