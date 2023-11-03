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
using System.Linq;
using System.Runtime.Serialization;
using Rock.Achievement.Component;
using Rock.Data;
using Rock.Model;
using Rock.Reporting.DataFilter.ContentChannelItem;

namespace Rock.Web.Cache.Entities
{
    /// <summary>
    /// Cache object for <see cref="AdaptiveMessageAdaptation" />
    /// </summary>
    [Serializable]
    [DataContract]
    public class AdaptiveMessageAdaptationCache : ModelCache<AdaptiveMessageAdaptationCache, AdaptiveMessageAdaptation>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
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
        [DataMember]
        public int AdaptiveMessageId { get; set; }

        /// <summary>
        /// Gets or sets the segment ids.
        /// </summary>
        /// <value>
        /// The segment ids.
        /// </value>
        [DataMember]
        public List<int> SegmentIds { get; private set; }

        /// <summary>
        /// Gets the channel type medium value.
        /// </summary>
        /// <value>
        /// The channel type medium value.
        /// </value>
        public AdaptiveMessageCache AdaptiveMessage
        {
            get
            {
                return AdaptiveMessageCache.Get( AdaptiveMessageId );
            }
        }

        #endregion Entity Properties

        #region Public Methods

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity"></param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );
            var adaptiveMessageAdaptation = entity as AdaptiveMessageAdaptation;

            if ( adaptiveMessageAdaptation == null )
            {
                return;
            }

            Name = adaptiveMessageAdaptation.Name;
            Description = adaptiveMessageAdaptation.Description;
            IsActive = adaptiveMessageAdaptation.IsActive;
            Order = adaptiveMessageAdaptation.Order;
            AdaptiveMessageId = adaptiveMessageAdaptation.AdaptiveMessageId;
            ViewSaturationCount = adaptiveMessageAdaptation.ViewSaturationCount;
            ViewSaturationInDays = adaptiveMessageAdaptation.ViewSaturationInDays;
            SegmentIds = adaptiveMessageAdaptation.AdaptiveMessageAdaptationSegments.Select( c => c.PersonalizationSegmentId ).ToList();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance Title.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance Title.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion Public Methods
    }
}