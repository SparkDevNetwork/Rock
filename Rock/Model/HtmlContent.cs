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
using Newtonsoft.Json;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Html Content POCO Entity.
    /// </summary>
    [Table( "HtmlContent" )]
    public partial class HtmlContent : Model<HtmlContent>
    {
        /// <summary>
        /// Gets or sets the Block Id.
        /// </summary>
        /// <value>
        /// Block Id.
        /// </value>
        [Required]
        public int BlockId { get; set; }
        
        /// <summary>
        /// Gets or sets the Entity Value.
        /// </summary>
        /// <value>
        /// Entity Value.
        /// </value>
        [MaxLength( 200 )]
        public string EntityValue { get; set; }
        
        /// <summary>
        /// Gets or sets the Version.
        /// </summary>
        /// <value>
        /// Version.
        /// </value>
        [Required]
        public int Version { get; set; }
        
        /// <summary>
        /// Gets or sets the Content.
        /// </summary>
        /// <value>
        /// Content.
        /// </value>
        [Required]
        public string Content { get; set; }
        
        /// <summary>
        /// Gets or sets the Approved.
        /// </summary>
        /// <value>
        /// Approved.
        /// </value>
        [Required]
        public bool IsApproved { get; set; }
        
        /// <summary>
        /// Gets or sets the Approved By Person Id.
        /// </summary>
        /// <value>
        /// Approved By Person Id.
        /// </value>
        public int? ApprovedByPersonId { get; set; }
        
        /// <summary>
        /// Gets or sets the Approved Date Time.
        /// </summary>
        /// <value>
        /// Approved Date Time.
        /// </value>
        public DateTime? ApprovedDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the Start Date Time.
        /// </summary>
        /// <value>
        /// Start Date Time.
        /// </value>
        public DateTime? StartDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the Expire Date Time.
        /// </summary>
        /// <value>
        /// Expire Date Time.
        /// </value>
        public DateTime? ExpireDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the Block.
        /// </summary>
        /// <value>
        /// A <see cref="Block"/> object.
        /// </value>
        [NotExportable]
        public virtual Block Block { get; set; }
        
        /// <summary>
        /// Gets or sets the Approved By Person.
        /// </summary>
        /// <value>
        /// A <see cref="Model.Person"/> object.
        /// </value>
        public virtual Model.Person ApprovedByPerson { get; set; }

        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        [NotExportable]
        public override IDto Dto
        {
            get { return this.ToDto(); }
        }

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
            this.HasRequired( p => p.Block ).WithMany().HasForeignKey( p => p.BlockId ).WillCascadeOnDelete(true);
            this.HasOptional( p => p.ApprovedByPerson ).WithMany().HasForeignKey( p => p.ApprovedByPersonId ).WillCascadeOnDelete(false);
        }
    }
}
