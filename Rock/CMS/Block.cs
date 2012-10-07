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

namespace Rock.Cms
{
    /// <summary>
    /// Block POCO Entity.
    /// </summary>
    [Table( "cmsBlock" )]
    public partial class Block : Model<Block>, IAuditable, IOrdered, IExportable
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
		/// Gets or sets the Block Type Id.
		/// </summary>
		/// <value>
		/// Block Type Id.
		/// </value>
		[Required]
		[DataMember]
		public int BlockTypeId { get; set; }
		
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
		public override string EntityTypeName { get { return "Cms.Block"; } }
        
		/// <summary>
        /// Gets or sets the Html Contents.
        /// </summary>
        /// <value>
        /// Collection of Html Contents.
        /// </value>
		public virtual ICollection<HtmlContent> HtmlContents { get; set; }
        
		/// <summary>
        /// Gets or sets the Block Type.
        /// </summary>
        /// <value>
        /// A <see cref="BlockType"/> object.
        /// </value>
		public virtual BlockType BlockType { get; set; }
        
		/// <summary>
        /// Gets or sets the Page.
        /// </summary>
        /// <value>
        /// A <see cref="Page"/> object.
        /// </value>
		public virtual Page Page { get; set; }
        
		/// <summary>
		/// Static Method to return an object based on the id
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		public static Block Read( int id )
		{
			return Read<Block>( id );
		}


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

            if (BlockType != null)
            {
                exportObject.BlockType = BlockType.ExportObject();
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
    public partial class BlockConfiguration : EntityTypeConfiguration<Block>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockConfiguration"/> class.
        /// </summary>
        public BlockConfiguration()
        {
			this.HasRequired( p => p.BlockType ).WithMany( p => p.Blocks ).HasForeignKey( p => p.BlockTypeId ).WillCascadeOnDelete(true);
			this.HasOptional( p => p.Page ).WithMany( p => p.Blocks ).HasForeignKey( p => p.PageId ).WillCascadeOnDelete(true);
		}
    }
}
