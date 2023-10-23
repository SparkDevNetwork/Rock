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

using Rock.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Data.Entity.ModelConfiguration;

namespace Rock.Model
{
    /// <summary>
    /// Represents a ContentTopic instance in Rock.
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "ContentTopic" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.CONTENT_TOPIC)]
    public partial class ContentTopic : Model<ContentTopic>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Name of this ContentTopic.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the name of this ContentTopic.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of this ContentTopic.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the description of this ContentTopic.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the display order of this ContentTopic. The lower the number the higher the display priority. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="int"/> that represents the display order of this ContentTopic.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this ContentTopic is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="bool"/> that is <c>true</c> if this ContentTopic is part of the Rock core system/framework; otherwise this value is <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this ContentTopic is active.
        /// </summary>
        /// <value>
        /// A <see cref="bool"/> that is <c>true</c> if this ContentTopic is active; otherwise this value is <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the ContentTopicDomainId of the <see cref="Rock.Model.ContentTopicDomain"/> that this ContentTopic belongs to. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="int"/> representing the ContentTopicDomainId of the <see cref="Rock.Model.ContentTopicDomain"/> that this ContentTopic belongs to.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int ContentTopicDomainId { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the content topic domain.
        /// </summary>
        /// <value>
        /// The content topic domain.
        /// </value>
        [DataMember]
        public virtual ContentTopicDomain ContentTopicDomain { get; set; }

        #endregion Navigation Properties

        #region Entity Configuration

        /// <summary>
        /// Configuration for the content library item entity type.
        /// </summary>
        public partial class ContentTopicConfiguration : EntityTypeConfiguration<ContentTopic>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ContentTopicConfiguration"/> class.
            /// </summary>
            public ContentTopicConfiguration()
            {
                this.HasRequired( c => c.ContentTopicDomain ).WithMany().HasForeignKey( c => c.ContentTopicDomainId ).WillCascadeOnDelete( true );
            }
        }

        #endregion
    }
}
