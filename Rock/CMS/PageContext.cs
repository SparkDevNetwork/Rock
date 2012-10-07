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
    /// Page Route POCO Entity.
    /// </summary>
    [Table( "cmsPageContext" )]
    public partial class PageContext : Model<PageContext>
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
		[Required]
		[DataMember]
		public int PageId { get; set; }
		
		/// <summary>
		/// Gets or sets the Entity.
		/// </summary>
		/// <value>
		/// Entity.
		/// </value>
		[Required]
		[MaxLength( 200 )]
		[DataMember]
		public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the page parameter that contains the entity's id.
        /// </summary>
        /// <value>
        /// Id Parameter.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember]
        public string IdParameter { get; set; }

        /// <summary>
		/// Gets or sets the Created Date Time.
		/// </summary>
		/// <value>
		/// Created Date Time.
		/// </value>
		[DataMember]
		public DateTime? CreatedDateTime { get; set; }
		
        /// <summary>
        /// Gets the auth entity.
        /// </summary>
		[NotMapped]
		public override string EntityTypeName { get { return "Cms.PageContext"; } }
        
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
