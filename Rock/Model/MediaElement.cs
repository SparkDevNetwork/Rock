using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Media Element
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "MediaElement" )]
    [DataContract]
    public partial class MediaElement : Model<MediaElement>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the MediaFolderId of the <see cref="Rock.Model.MediaFolder"/> that this MediaElement belongs to. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the MediaFolderId of the <see cref="Rock.Model.MediaFolder"/> that this MediaElement belongs to.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int MediaFolderId { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Element. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the Element.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of the Element.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or set the duration of media element.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> representing the duration of media element.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal? Duration { get; set; }

        /// <summary>
        /// Gets or sets the source created date time.
        /// </summary>
        /// <value>
        /// The source created date time.
        /// </value>
        public DateTime? SourceCreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the source modified date time.
        /// </summary>
        /// <value>
        /// The source modified date time.
        /// </value>
        public DateTime? SourceModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the source data.
        /// </summary>
        /// <value>
        /// The source data.
        /// </value>
        [DataMember]
        public string SourceData { get; set; }

        /// <summary>
        /// Gets or sets the source metric.
        /// </summary>
        /// <value>
        /// The source metric.
        /// </value>
        [DataMember]
        public string SourceMetric { get; set; }

        /// <summary>
        /// Gets or sets the source key.
        /// </summary>
        /// <value>
        /// The source key.
        /// </value>
        [DataMember]
        [MaxLength( 60 )]
        public string SourceKey { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail data.
        /// </summary>
        /// <value>
        /// The thumbnail data.
        /// </value>
        [DataMember]
        public string ThumbnailData { get; set; }

        /// <summary>
        /// Gets or sets the media element data.
        /// </summary>
        /// <value>
        /// The media element data.
        /// </value>
        [DataMember]
        public string MediaElementData { get; set; }

        /// <summary>
        /// Gets or sets the download data.
        /// </summary>
        /// <value>
        /// The download data.
        /// </value>
        [DataMember]
        public string DownloadData { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the Media Folder that this Element belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.MediaFolder"/> that this Element belongs to.
        /// </value>
        [LavaInclude]
        public virtual MediaFolder MediaFolder { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Media Element Configuration class.
    /// </summary>
    public partial class MediaElementConfiguration : EntityTypeConfiguration<MediaElement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaElementConfiguration"/> class.
        /// </summary>
        public MediaElementConfiguration()
        {
            this.HasRequired( p => p.MediaFolder ).WithMany( p => p.MediaElements ).HasForeignKey( p => p.MediaFolderId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}