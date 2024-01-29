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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Rock.Data;
using Rock.Lava;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "AdaptiveMessageAdaptation" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.ADAPTIVE_MESSAGE_ADAPTATION )]
    public partial class AdaptiveMessageAdaptation : Model<AdaptiveMessageAdaptation>, IHasActiveFlag, IOrdered, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
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
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the view saturation count.
        /// </summary>
        /// <value>
        /// The view saturation count.
        /// </value>
        [DataMember]
        public int? ViewSaturationCount { get; set; }

        /// <summary>
        /// Gets or sets the view saturation in days.
        /// </summary>
        /// <value>
        /// The view saturation in days.
        /// </value>
        [DataMember]
        public int? ViewSaturationInDays { get; set; }

        /// <summary>
        /// Gets or sets the adaptive message identifier.
        /// </summary>
        /// <value>
        /// The adaptive message identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int AdaptiveMessageId { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the adaptive message.
        /// </summary>
        /// <value>
        /// The adaptive message.
        /// </value>
        [LavaVisible]
        public virtual AdaptiveMessage AdaptiveMessage { get; set; }

        /// <summary>
        /// Gets or sets the collection of AdaptiveMessageAdaptationSegments.
        /// </summary>
        /// <value>
        /// A collection of AdaptiveMessageAdaptationSegments.
        /// </value>
        [DataMember, JsonIgnore]
        public virtual ICollection<AdaptiveMessageAdaptationSegment> AdaptiveMessageAdaptationSegments
        {
            get { return _adaptiveMessageAdaptationSegments ?? ( _adaptiveMessageAdaptationSegments = new Collection<AdaptiveMessageAdaptationSegment>() ); }
            set { _adaptiveMessageAdaptationSegments = value; }
        }

        private ICollection<AdaptiveMessageAdaptationSegment> _adaptiveMessageAdaptationSegments;

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class AdaptiveMessageAdaptationConfiguration : EntityTypeConfiguration<AdaptiveMessageAdaptation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveMessageAdaptationConfiguration" /> class.
        /// </summary>
        public AdaptiveMessageAdaptationConfiguration()
        {
            this.HasRequired( p => p.AdaptiveMessage ).WithMany( p => p.AdaptiveMessageAdaptations ).HasForeignKey( p => p.AdaptiveMessageId ).WillCascadeOnDelete( true );
        }
    }

    #endregion Entity Configuration
}