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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Lava;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Type of <see cref="Rock.Model.ContentLibrary"/>.
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "ContentLibrary" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.CONTENT_LIBRARY )]
    public partial class ContentLibrary : Model<ContentLibrary>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the ContentLibrary.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the name of the ContentLibrary.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the library key.
        /// </summary>
        /// <value>
        /// The library key.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string LibraryKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether trending is enabled for this
        /// content library.
        /// </summary>
        /// <value>
        ///   <c>true</c> if trending is enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool TrendingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the trending window day.
        /// </summary>
        /// <value>
        /// The trending window day.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int TrendingWindowDay { get; set; }

        /// <summary>
        /// Gets or sets the trending max items.
        /// </summary>
        /// <value>
        /// The trending max items.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int TrendingMaxItems { get; set; }

        /// <summary>
        /// Gets or sets the trending gravity to apply more weight to items that
        /// are newer.
        /// </summary>
        /// <value>
        /// The trending gravity to apply more weight to items that are newer.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public decimal TrendingGravity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether personalization segments
        /// should be enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if personalization segments should be enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableSegments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether personalization request
        /// filters should be enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if personalization request filters should be enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableRequestFilters { get; set; }

        /// <summary>
        /// Gets or sets the filter settings.
        /// </summary>
        /// <value>
        /// The filter settings.
        /// </value>
        [DataMember]
        public string FilterSettings { get; set; }

        /// <summary>
        /// Gets or sets the last index date time.
        /// </summary>
        /// <value>
        /// The last index date time.
        /// </value>
        [DataMember]
        public DateTime? LastIndexDateTime { get; set; }

        /// <summary>
        /// Gets or sets the last index item count.
        /// </summary>
        /// <value>
        /// The last index index item count.
        /// </value>
        [DataMember]
        public int? LastIndexItemCount { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the content library sources.
        /// </summary>
        /// <value>
        /// The content library sources.
        /// </value>
        [LavaVisible]
        public virtual ICollection<ContentLibrarySource> ContentLibrarySources
        {
            get { return _contentLibrarySources ?? ( _contentLibrarySources = new Collection<ContentLibrarySource>() ); }
            set { _contentLibrarySources = value; }
        }

        private ICollection<ContentLibrarySource> _contentLibrarySources;

        #endregion Navigation Properties

        #region Methods

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

        #endregion Methods
    }

    #region Entity Configuration

    /// <summary>
    /// Content Library Configuration class.
    /// </summary>
    public partial class ContentLibraryConfiguration : EntityTypeConfiguration<ContentLibrary>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentLibraryConfiguration"/> class.
        /// </summary>
        public ContentLibraryConfiguration()
        {
        }
    }

    #endregion Entity Configuration
}
