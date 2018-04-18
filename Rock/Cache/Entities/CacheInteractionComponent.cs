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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Cache
{
    /// <summary>
    /// Information about a interactionComponent that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class CacheInteractionComponent : ModelCache<CacheInteractionComponent, InteractionComponent>
    {

        #region Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        [DataMember]
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the channel identifier.
        /// </summary>
        /// <value>
        /// The channel identifier.
        /// </value>
        [DataMember]
        public int ChannelId { get; set; }

        /// <summary>
        /// Gets the interaction channel.
        /// </summary>
        /// <value>
        /// The interaction channel.
        /// </value>
        public CacheInteractionChannel InteractionChannel => CacheInteractionChannel.Get( ChannelId );

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies the model property values to the DTO properties
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var interactionComponent = entity as InteractionComponent;
            if ( interactionComponent == null ) return;

            Name = interactionComponent.Name;
            EntityId = interactionComponent.EntityId;
            ChannelId = interactionComponent.ChannelId;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

    }
}