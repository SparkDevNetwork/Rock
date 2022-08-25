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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Type of <see cref="Rock.Model.ContentCollectionSource"/>.
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "ContentCollectionSource" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.CONTENT_COLLECTION_SOURCE )]
    public partial class ContentCollectionSource : Model<ContentCollectionSource>, IOrdered, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the entity. This property is required.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity id. This property is required.
        /// </summary>
        /// <value>
        /// The entity id.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the occurrences to show.
        /// </summary>
        /// <value>
        /// The occurrences to show.
        /// </value>
        [DataMember]
        public int OccurrencesToShow { get; set; }

        /// <summary>
        /// Gets or sets the additional settings.
        /// </summary>
        /// <value>
        /// The additional settings.
        /// </value>
        [DataMember]
        public string AdditionalSettings { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.ContentCollection"/>. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="int"/> representing the Id of the <see cref="Rock.Model.ContentCollection"/>
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int ContentCollectionId { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public int Order { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ContentCollection"/>.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.ContentCollection"/> of this content collection source.
        /// </value>
        [DataMember]
        public virtual ContentCollection ContentCollection { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> of the entity.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> of the entity.
        /// </value>
        [DataMember]
        public virtual Model.EntityType EntityType { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Content Collection Source Configuration class.
    /// </summary>
    public partial class ContentCollectionSourceConfiguration : EntityTypeConfiguration<ContentCollectionSource>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentCollectionSourceConfiguration"/> class.
        /// </summary>
        public ContentCollectionSourceConfiguration()
        {
            this.HasRequired( c => c.ContentCollection ).WithMany( c => c.ContentCollectionSources ).HasForeignKey( c => c.ContentCollectionId ).WillCascadeOnDelete( true );
            this.HasRequired( c => c.EntityType ).WithMany().HasForeignKey( c => c.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
