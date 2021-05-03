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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Media Folder
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "MediaFolder" )]
    [DataContract]
    public partial class MediaFolder : Model<MediaFolder>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the MediaAccountId of the <see cref="Rock.Model.MediaAccount"/> that this MediaFolder belongs to. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the MediaAccountId of the <see cref="Rock.Model.MediaAccount"/> that this MediaFolder belongs to.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int MediaAccountId { get; set; }

        /// <summary>
        /// Gets or sets the Name of the MediaFolder. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the MediaFolder.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of the MediaFolder.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this Media Folder is public.
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> that is <c>true</c> if this Media Folder is public, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsPublic
        {
            get { return _isPublic; }
            set { _isPublic = value; }
        }
        private bool? _isPublic = true;

        /// <summary>
        /// Gets or sets the source data.
        /// </summary>
        /// <value>
        /// The source data.
        /// </value>
        [DataMember]
        public string SourceData { get; set; }

        /// <summary>
        /// Gets or sets the metric data.
        /// </summary>
        /// <value>
        /// The metric data.
        /// </value>
        [DataMember]
        public string MetricData { get; set; }

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
        /// Gets or sets a value indicating whether the content channel sync is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the content channel sync is enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsContentChannelSyncEnabled { get; set; }

        /// <summary>
        /// Gets or sets the content channel identifier.
        /// </summary>
        /// <value>
        /// The content channel identifier.
        /// </value>
        [DataMember]
        public int? ContentChannelId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ContentChannelItemStatus"/> Content channel Item status.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.ContentChannelItemStatus"/> enumeration value that represents the status of the ContentItem.
        /// </value>
        [DataMember]
        public ContentChannelItemStatus? Status { get; set; }

        /// <summary>
        /// Gets or sets the content channel attribute identifier.
        /// </summary>
        /// <value>
        /// The content channel attribute identifier.
        /// </value>
        [DataMember]
        public int? ContentChannelAttributeId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the Media Account that this MediaFolder belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.MediaAccount"/> that this MediaFolder belongs to.
        /// </value>
        [Rock.Lava.LavaVisibleAttribute]
        public virtual MediaAccount MediaAccount { get; set; }

        /// <summary>
        /// Gets or sets the content channel.
        /// </summary>
        /// <value>
        /// The content channel.
        /// </value>
        [DataMember]
        public virtual ContentChannel ContentChannel { get; set; }

        /// <summary>
        /// Gets or sets the content channel attribute.
        /// </summary>
        /// <value>
        /// The content channel attribute.
        /// </value>
        [DataMember]
        public virtual Attribute ContentChannelAttribute { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.MediaElement">Elements</see> that belong to this Folder.
        /// </summary>
        /// <value>
        /// A collection of the <see cref="Rock.Model.MediaFolder">Elements</see> that belong to this Folder.
        /// </value>
        [DataMember]
        public virtual ICollection<MediaElement> MediaElements { get; set; }

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
    /// Media Folder Configuration class.
    /// </summary>
    public partial class MediaFolderConfiguration : EntityTypeConfiguration<MediaFolder>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaFolderConfiguration"/> class.
        /// </summary>
        public MediaFolderConfiguration()
        {
            this.HasRequired( p => p.MediaAccount ).WithMany( p => p.MediaFolders ).HasForeignKey( p => p.MediaAccountId ).WillCascadeOnDelete( true );
            this.HasOptional( i => i.ContentChannel ).WithMany().HasForeignKey( i => i.ContentChannelId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.ContentChannelAttribute ).WithMany().HasForeignKey( a => a.ContentChannelAttributeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}