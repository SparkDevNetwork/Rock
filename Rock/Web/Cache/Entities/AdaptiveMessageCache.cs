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
using Rock.Achievement;
using Rock.Achievement.Component;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache.Entities;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cache object for <see cref="AdaptiveMessage" />
    /// </summary>
    [Serializable]
    [DataContract]
    public class AdaptiveMessageCache : ModelCache<AdaptiveMessageCache, AdaptiveMessage>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
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
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        #endregion Entity Properties

        #region Related Cache Objects

        /// <summary>
        /// Gets the prerequisites.
        /// </summary>
        /// <value>
        /// The prerequisites.
        /// </value>
        public List<AdaptiveMessageAdaptationCache> Adaptations
            => AdaptiveMessageAdaptationCache.All().Where( amc => amc.AdaptiveMessageId == Id ).ToList();

        #endregion Related Cache Objects

        #region Public Methods

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity"></param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );
            var adaptiveMessage = entity as AdaptiveMessage;

            if ( adaptiveMessage == null )
            {
                return;
            }

            Name = adaptiveMessage.Name;
            Description = adaptiveMessage.Description;
            IsActive = adaptiveMessage.IsActive;
            Key = adaptiveMessage.Key;
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