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
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "AdaptiveMessageAdaptationSegment" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.ADAPTIVE_MESSAGE_ADAPTATION_SEGMENT )]
    public partial class AdaptiveMessageAdaptationSegment : Model<AdaptiveMessageAdaptationSegment>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the segment identifier.
        /// </summary>
        /// <value>
        /// The segment identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int PersonalizationSegmentId { get; set; }

        /// <summary>
        /// Gets or sets the adaptive message adaptation identifier.
        /// </summary>
        /// <value>
        /// The adaptive message adaptation identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int AdaptiveMessageAdaptationId { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the personalization segment.
        /// </summary>
        /// <value>
        /// The personalization segment.
        /// </value>
        [LavaVisible]
        public virtual PersonalizationSegment PersonalizationSegment { get; set; }

        /// <summary>
        /// Gets or sets the adaptive message.
        /// </summary>
        /// <value>
        /// The adaptive message.
        /// </value>
        [LavaVisible]
        public virtual AdaptiveMessageAdaptation AdaptiveMessageAdaptation { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class AdaptiveMessageAdaptationSegmentConfiguration : EntityTypeConfiguration<AdaptiveMessageAdaptationSegment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveMessageAdaptationSegmentConfiguration" /> class.
        /// </summary>
        public AdaptiveMessageAdaptationSegmentConfiguration()
        {
            this.HasRequired( p => p.AdaptiveMessageAdaptation ).WithMany( p => p.AdaptiveMessageAdaptationSegments ).HasForeignKey( p => p.AdaptiveMessageAdaptationId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.PersonalizationSegment ).WithMany().HasForeignKey( p => p.PersonalizationSegmentId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}