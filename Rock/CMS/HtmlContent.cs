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
    /// Html Content POCO Entity.
    /// </summary>
    [Table( "cmsHtmlContent" )]
    public partial class HtmlContent : Model<HtmlContent>, IAuditable, IExportable
    {
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
		/// Gets or sets the Entity Value.
		/// </summary>
		/// <value>
		/// Entity Value.
		/// </value>
		[MaxLength( 200 )]
		[DataMember]
		public string EntityValue { get; set; }
		
		/// <summary>
		/// Gets or sets the Version.
		/// </summary>
		/// <value>
		/// Version.
		/// </value>
		[Required]
		[DataMember]
		public int Version { get; set; }
		
		/// <summary>
		/// Gets or sets the Content.
		/// </summary>
		/// <value>
		/// Content.
		/// </value>
		[Required]
		[DataMember]
		public string Content { get; set; }
		
		/// <summary>
		/// Gets or sets the Approved.
		/// </summary>
		/// <value>
		/// Approved.
		/// </value>
		[Required]
		[DataMember]
		public bool IsApproved { get; set; }
		
		/// <summary>
		/// Gets or sets the Approved By Person Id.
		/// </summary>
		/// <value>
		/// Approved By Person Id.
		/// </value>
		[DataMember]
		public int? ApprovedByPersonId { get; set; }
		
		/// <summary>
		/// Gets or sets the Approved Date Time.
		/// </summary>
		/// <value>
		/// Approved Date Time.
		/// </value>
		[DataMember]
		public DateTime? ApprovedDateTime { get; set; }
		
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
		/// Gets or sets the Start Date Time.
		/// </summary>
		/// <value>
		/// Start Date Time.
		/// </value>
		[DataMember]
		public DateTime? StartDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Expire Date Time.
		/// </summary>
		/// <value>
		/// Expire Date Time.
		/// </value>
		[DataMember]
		public DateTime? ExpireDateTime { get; set; }
		
        /// <summary>
        /// Gets the auth entity.
        /// </summary>
		[NotMapped]
		public override string EntityTypeName { get { return "Cms.HtmlContent"; } }
        
		/// <summary>
        /// Gets or sets the Block.
        /// </summary>
        /// <value>
        /// A <see cref="Block"/> object.
        /// </value>
		public virtual Block Block { get; set; }
        
		/// <summary>
        /// Gets or sets the Approved By Person.
        /// </summary>
        /// <value>
        /// A <see cref="Crm.Person"/> object.
        /// </value>
		public virtual Crm.Person ApprovedByPerson { get; set; }
        
		/// <summary>
		/// Static Method to return an object based on the id
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		public static HtmlContent Read( int id )
		{
			return Read<HtmlContent>( id );
		}

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Content;
        }

        /// <summary>
        /// Exports the object as JSON.
        /// </summary>
        /// <returns></returns>
        public string ExportJson()
        {
            return ExportObject().ToJSON();
        }

        /// <summary>
        /// Exports the object.
        /// </summary>
        /// <returns></returns>
        public object ExportObject()
        {
            return this.ToDynamic();
        }

        /// <summary>
        /// Imports the data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void ImportJson(string data)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Html Content Configuration class.
    /// </summary>
    public partial class HtmlContentConfiguration : EntityTypeConfiguration<HtmlContent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlContentConfiguration"/> class.
        /// </summary>
        public HtmlContentConfiguration()
        {
			this.HasRequired( p => p.Block ).WithMany( p => p.HtmlContents ).HasForeignKey( p => p.BlockId ).WillCascadeOnDelete(true);
			this.HasOptional( p => p.ApprovedByPerson ).WithMany().HasForeignKey( p => p.ApprovedByPersonId ).WillCascadeOnDelete(false);
		}
    }
}
