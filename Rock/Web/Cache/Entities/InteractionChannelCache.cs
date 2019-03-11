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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a interactionChannel that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class InteractionChannelCache : ModelCache<InteractionChannelCache, InteractionChannel>
    {

        #region Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the component entity type identifier.
        /// </summary>
        /// <value>
        /// The component entity type identifier.
        /// </value>
        [DataMember]
        public int? ComponentEntityTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the interaction entity type identifier.
        /// </summary>
        /// <value>
        /// The interaction entity type identifier.
        /// </value>
        [DataMember]
        public int? InteractionEntityTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the channel entity identifier.
        /// </summary>
        /// <value>
        /// The channel entity identifier.
        /// </value>
        [DataMember]
        public int? ChannelEntityId { get; private set; }

        /// <summary>
        /// Gets or sets the channel type medium value identifier.
        /// </summary>
        /// <value>
        /// The channel type medium value identifier.
        /// </value>
        [DataMember]
        public int? ChannelTypeMediumValueId { get; private set; }

        /// <summary>
        /// Gets or sets the duration of the retention.
        /// </summary>
        /// <value>
        /// The duration of the retention.
        /// </value>
        [DataMember]
        public int? RetentionDuration { get; private set; }

        /// <summary>
        /// Gets or sets the length of time that components of this channel should be cached
        /// </summary>
        /// <value>
        /// The duration of the component cache.
        /// </value>
        [DataMember]
        public int? ComponentCacheDuration { get; private set; }

        /// <summary>
        /// Gets the type of the component entity.
        /// </summary>
        /// <value>
        /// The type of the component entity.
        /// </value>
        public EntityTypeCache ComponentEntityType
        {
            get
            {
                if ( ComponentEntityTypeId.HasValue )
                {
                    return EntityTypeCache.Get( ComponentEntityTypeId.Value );
                }

                return null;
            }
        }


        /// <summary>
        /// Gets the type of the interaction entity.
        /// </summary>
        /// <value>
        /// The type of the interaction entity.
        /// </value>
        public EntityTypeCache InteractionEntityType
        {
            get
            {
                if ( InteractionEntityTypeId.HasValue )
                {
                    return EntityTypeCache.Get( InteractionEntityTypeId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the channel type medium value.
        /// </summary>
        /// <value>
        /// The channel type medium value.
        /// </value>
        public DefinedValueCache ChannelTypeMediumValue
        {
            get
            {
                if ( ChannelTypeMediumValueId.HasValue )
                {
                    return DefinedValueCache.Get( ChannelTypeMediumValueId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the interaction components.
        /// </summary>
        /// <value>
        /// The interaction components.
        /// </value>
        [Obsolete( "This is not performant. Instead get the ID for the InteractionComponent from the DB using " +
            "the InteractionComponentService and then use the ID to get the InteractionComponentCache obj from the cache." )]
        [RockObsolete( "1.9.15" )]
        public List<InteractionComponentCache> InteractionComponents
        {
            get
            {
                var components = new List<InteractionComponentCache>();

                InitComponentIds();

                if ( InteractionComponentIds == null ) return components;

                foreach ( var id in InteractionComponentIds.Keys )
                {
                    var component = InteractionComponentCache.Get( id );
                    if ( component != null )
                    {
                        components.Add( component );
                    }
                }

                return components;
            }
        }
        /// <summary>
        /// The component ids
        /// </summary>
        internal ConcurrentDictionary<int, int> InteractionComponentIds { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the component ids.
        /// </summary>
        private void InitComponentIds()
        {
            if ( InteractionComponentIds != null )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                InteractionComponentIds = new ConcurrentDictionary<int, int>( new InteractionComponentService( rockContext )
                    .GetByChannelId( Id )
                    .Select( v => v.Id )
                    .ToList().ToDictionary( k => k, v => v ) );
            }
        }

        /// <summary>
        /// Adds the component identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void AddComponentId( int id )
        {
            InitComponentIds();

            if ( InteractionComponentIds == null )
            {
                return;
            }

            InteractionComponentIds.TryAdd( id, id );
        }

        /// <summary>
        /// Removes the component identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void RemoveComponentId( int id )
        {
            InitComponentIds();

            if ( InteractionComponentIds == null )
            {
                return;
            }

            int value;
            InteractionComponentIds.TryRemove( id, out value );
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var interactionChannel = entity as InteractionChannel;
            if ( interactionChannel == null ) return;

            Name = interactionChannel.Name;
            ChannelEntityId = interactionChannel.ChannelEntityId;
            ChannelTypeMediumValueId = interactionChannel.ChannelTypeMediumValueId;
            ComponentEntityTypeId = interactionChannel.ComponentEntityTypeId;
            ForeignGuid = interactionChannel.ForeignGuid;
            ForeignKey = interactionChannel.ForeignKey;
            InteractionEntityTypeId = interactionChannel.InteractionEntityTypeId;
            RetentionDuration = interactionChannel.RetentionDuration;
            ComponentCacheDuration = interactionChannel.ComponentCacheDuration;

            // set componentIds to null so it load them all at once on demand
            InteractionComponentIds = null;
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