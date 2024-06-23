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
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;

namespace Rock.Model
{
    /// <summary>
    /// Represents a ContentTopicDomain instance in Rock.
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "ContentTopicDomain" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.CONTENT_TOPIC_DOMAIN )]
    public partial class ContentTopicDomain : Model<ContentTopicDomain>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Name of this ContentTopicDomain.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the name of this ContentTopicDomain.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of this ContentTopicDomain.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the description of this ContentTopicDomain.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the display order of this ContentTopicDomain. The lower the number the higher the display priority. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="int"/> that represents the display order of this ContentTopicDomain.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this ContentTopicDomain is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="bool"/> that is <c>true</c> if this ContentTopicDomain is part of the Rock core system/framework; otherwise this value is <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this ContentTopicDomain is active.
        /// </summary>
        /// <value>
        /// A <see cref="bool"/> that is <c>true</c> if this ContentTopicDomain is active; otherwise this value is <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        #endregion Entity Properties
    }

    #region Entity Configuration

    /// <summary>
    /// ContentTopicDomain Configuration Class
    /// </summary>
    public partial class ContentTopicDomainConfiguration : EntityTypeConfiguration<ContentTopicDomain>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentTopicDomainConfiguration"/> class.
        /// </summary>
        public ContentTopicDomainConfiguration()
        {
            // Empty constructor. This is required to tell EF that this model exists.
        }
    }

    #endregion Entity Configuration
}
