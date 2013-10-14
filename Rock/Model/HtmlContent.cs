//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Html Content POCO Entity.
    /// </summary>
    [Table( "HtmlContent" )]
    [DataContract]
    public partial class HtmlContent : Model<HtmlContent>
    {
        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Block"/> that the HTML content should appear on. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Block" /> that the HTML content should be a part of.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int BlockId { get; set; }
        
        /// <summary>
        /// Gets or sets the Entity Value that must be present on the page for this HTML Content to be displayed. If this value will null
        /// there will not be an entity restriction on the HTMLContent object.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the EntityValue restriction for the HTMLContent object.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string EntityValue { get; set; }
        
        /// <summary>
        /// Gets or sets the version number for the HTMLContent
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the version number for the HTML content.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Version { get; set; }
        
        /// <summary>
        /// Gets or sets the HTML content that will display on the block when conditions (if any) are met.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the HTML content that will appear as part of the block.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public string Content { get; set; }
        
        /// <summary>
        /// Gets or sets a flag indicating if the content has been approved. If approval is required, the content will not be displayed until it has been approved.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the HTML content has been approved; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsApproved { get; set; }
        
        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Person"/> who approved the HTMLContent.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> who approved the HTMLContent. If the HTMLContent had not been approved, this value will be null.
        /// </value>
        [DataMember]
        public int? ApprovedByPersonId { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time that the HTMLContent was approved.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> value that represents when the HTMLContent was approved. If the HTMLContent had not been approved, this value will be null.
        /// </value>
        [DataMember]
        public DateTime? ApprovedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the HTMLContent becomes active and available to be displayed on the web.  If a date and time is provided, the HTMLContent will not be available until then; if null
        /// the HTMLContent will be available immediately.  Please note that the start date is overridden by the approval status, if the HTMLContent is subject to approval, it will not be displayed until it is approved.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime" /> representing the date and time that the content becomes available. If the HTMLContent does not have start date (immediately available) this value will be null.
        /// </value>
        [DataMember]
        public DateTime? StartDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time that the HTMLContent expires and is no longer available. If this value is null the HTMLContent remains available until it is overwritten or replaced with a new version.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that the HTMLContent expires. If the content does not expire this value will be null.
        /// </value>
        [DataMember]
        public DateTime? ExpireDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Block"/> that this HTMLContent appears on. 
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Block"/> that this HTML content appears on.
        /// </value>
        [DataMember]
        public virtual Block Block { get; set; }
        
        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Perosn"/> who approved the HTML content.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Person"/> who approved the HTMLContent.
        /// </value>
        [DataMember]
        public virtual Model.Person ApprovedByPerson { get; set; }

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
            this.HasRequired( p => p.Block ).WithMany().HasForeignKey( p => p.BlockId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.ApprovedByPerson ).WithMany().HasForeignKey( p => p.ApprovedByPersonId ).WillCascadeOnDelete(false);
        }
    }
}
