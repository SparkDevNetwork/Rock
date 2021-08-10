// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Rock.Data;
using Rock.Media;

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
        #region Fields

        /// <summary>
        /// <c>true</c> if the <see cref="PostSaveChanges(Data.DbContext)"/> method
        /// call will be for an add operation.
        /// </summary>
        private bool _saveOperationIsAdd;

        #endregion

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
        /// Gets or set the duration in seconds of media element.
        /// </summary>
        /// <value>
        /// A integer representing the duration in seconds of media element.
        /// </value>
        [DataMember]
        public int? DurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> this instance was created on the provider.
        /// </summary>
        /// <value>
        /// The <see cref="DateTime"/> this instance was created on the provider.
        /// </value>
        public DateTime? SourceCreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> this instance was modified on the provider.
        /// </summary>
        /// <value>
        /// The <see cref="DateTime"/> this instance was modified on the provider.
        /// </value>
        public DateTime? SourceModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the custom provider data for this instance.
        /// </summary>
        /// <value>
        /// The custom provider data for this instance.
        /// </value>
        [DataMember]
        public string SourceData { get; set; }

        /// <summary>
        /// Gets or sets the custom provider metric data for this instance.
        /// </summary>
        /// <value>
        /// The custom provider metric data for this instance.
        /// </value>
        [DataMember]
        public string MetricData { get; set; }

        /// <summary>
        /// Gets or sets the provider's unique identifier for this instance.
        /// </summary>
        /// <value>
        /// The provider's unique identifier for this instance.
        /// </value>
        [DataMember]
        [MaxLength( 60 )]
        public string SourceKey { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail data JSON content that will stored
        /// in the database.
        /// </summary>
        /// <value>
        /// The thumbnail data.
        /// </value>
        [DataMember]
        public string ThumbnailDataJson
        {
            get
            {
                return ThumbnailData.ToJson();
            }
            set
            {
                ThumbnailData = value.FromJsonOrNull<List<MediaElementThumbnailData>>() ?? new List<MediaElementThumbnailData>();
            }
        }

        /// <summary>
        /// Gets or sets the file data JSON content that will be stored in
        /// the database.
        /// </summary>
        /// <value>
        /// The file data.
        /// </value>
        [DataMember]
        public string FileDataJson
        {
            get
            {
                return FileData.ToJson();
            }
            set
            {
                FileData = value.FromJsonOrNull<List<MediaElementFileData>>() ?? new List<MediaElementFileData>();
            }
        }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the Media Folder that this Element belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.MediaFolder"/> that this Element belongs to.
        /// </value>
        [Rock.Lava.LavaVisibleAttribute]
        public virtual MediaFolder MediaFolder { get; set; }

        /// <summary>
        /// Gets or sets the media element file data. This contains all the
        /// information about the different file URLs available for the user
        /// to stream or download.
        /// </summary>
        /// <value>
        /// The media element file data.
        /// </value>
        [NotMapped]
        public virtual List<MediaElementFileData> FileData { get; set; } = new List<MediaElementFileData>();

        /// <summary>
        /// Gets or sets the thumbnail data.
        /// </summary>
        /// <value>
        /// The thumbnail data.
        /// </value>
        [NotMapped]
        public virtual List<MediaElementThumbnailData> ThumbnailData { get; set; } = new List<MediaElementThumbnailData>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default file URL to use for media playback.
        /// </summary>
        /// <value>
        /// The default file URL or an empty string if one is not available.
        /// </value>
        [DataMember]
        public string DefaultFileUrl
        {
            get
            {
                // OrderByDescending is correct when doing a boolean, true > false.
                return FileData.OrderByDescending( f => f.Quality == MediaElementQuality.HLS )
                    .ThenByDescending( f => f.Quality == MediaElementQuality.UltraHD )
                    .ThenByDescending( f => f.Quality == MediaElementQuality.HD )
                    .ThenByDescending( f => f.Quality == MediaElementQuality.SD )
                    .ThenByDescending( f => f.Quality == MediaElementQuality.Embed )
                    .ThenByDescending( f => f.Quality == MediaElementQuality.Audio )
                    .ThenByDescending( f => f.Width )
                    .FirstOrDefault()?.Link ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets the default thumbnail URL.
        /// </summary>
        /// <value>
        /// The default thumbnail URL or an empty string if one is not available.
        /// </value>
        [DataMember]
        public string DefaultThumbnailUrl
        {
            get
            {
                return ThumbnailData.OrderByDescending( t => t.Width )
                    .FirstOrDefault()?.Link ?? string.Empty;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context this operation was called on.</param>
        /// <param name="entry">The entry that identifies this entity in the change tracker.</param>
        public override void PreSaveChanges( Data.DbContext dbContext, DbEntityEntry entry )
        {
            _saveOperationIsAdd = entry.State == EntityState.Added;

            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context this operation was called on.</param>
        public override void PostSaveChanges( Data.DbContext dbContext )
        {
            if ( _saveOperationIsAdd )
            {
                // We don't need to wait for this to complete.
                Task.Run( () => MediaElementService.TriggerPostSaveTasks( this.Id ) );
            }

            base.PostSaveChanges( dbContext );
        }

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
            this.HasRequired( e => e.MediaFolder ).WithMany( f => f.MediaElements ).HasForeignKey( e => e.MediaFolderId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}