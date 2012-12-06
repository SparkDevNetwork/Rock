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
    /// Page Route POCO Entity.
    /// </summary>
    [Table( "PageContext" )]
    public partial class PageContext : Model<PageContext>, IExportable
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the Page Id.
        /// </summary>
        /// <value>
        /// Page Id.
        /// </value>
        [Required]
        public int PageId { get; set; }
        
        /// <summary>
        /// Gets or sets the Entity.
        /// </summary>
        /// <value>
        /// Entity.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the page parameter that contains the entity's id.
        /// </summary>
        /// <value>
        /// Id Parameter.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        public string IdParameter { get; set; }

        /// <summary>
        /// Gets or sets the Created Date Time.
        /// </summary>
        /// <value>
        /// Created Date Time.
        /// </value>
        public DateTime? CreatedDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the Page.
        /// </summary>
        /// <value>
        /// A <see cref="Page"/> object.
        /// </value>
        [NotExportable]
        public virtual Page Page { get; set; }

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
        public static PageContext Read( int id )
        {
            return Read<PageContext>( id );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0}:{1}", this.Entity, this.IdParameter );
        }

        public object ExportObject()
        {
            return this.ToDynamic();
        }

        public string ExportJson()
        {
            return ExportObject().ToJSON();
        }

        public void ImportJson( string data )
        {
            JsonConvert.PopulateObject( data, this );
        }
    }

    /// <summary>
    /// Page Route Configuration class.
    /// </summary>
    public partial class PageContextConfiguration : EntityTypeConfiguration<PageContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageContextConfiguration"/> class.
        /// </summary>
        public PageContextConfiguration()
        {
            this.HasRequired( p => p.Page ).WithMany( p => p.PageContexts ).HasForeignKey( p => p.PageId ).WillCascadeOnDelete(true);
        }
    }
}
