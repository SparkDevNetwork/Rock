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
using System.Linq;

using Rock.Cache;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a interactionChannel that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.CacheInteractionChannel instead" )]
    public class InteractionChannelCache : CachedModel<InteractionChannel>
    {
        #region Constructors

        private InteractionChannelCache( CacheInteractionChannel cacheInteractionChannel )
        {
            CopyFromNewCache( cacheInteractionChannel );
        }

        #endregion

        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the component entity type identifier.
        /// </summary>
        /// <value>
        /// The component entity type identifier.
        /// </value>
        public int? ComponentEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the interaction entity type identifier.
        /// </summary>
        /// <value>
        /// The interaction entity type identifier.
        /// </value>
        public int? InteractionEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the channel entity identifier.
        /// </summary>
        /// <value>
        /// The channel entity identifier.
        /// </value>
        public int? ChannelEntityId { get; set; }

        /// <summary>
        /// Gets or sets the channel type medium value identifier.
        /// </summary>
        /// <value>
        /// The channel type medium value identifier.
        /// </value>
        public int? ChannelTypeMediumValueId { get; set; }

        /// <summary>
        /// Gets or sets the duration of the retention.
        /// </summary>
        /// <value>
        /// The duration of the retention.
        /// </value>
        public int? RetentionDuration { get; set; }

        /// <summary>
        /// Gets or sets the length of time that components of this channel should be cached
        /// </summary>
        /// <value>
        /// The duration of the component cache.
        /// </value>
        public int? ComponentCacheDuration { get; set; }

        /// <summary>
        /// Gets the type of the component entity.
        /// </summary>
        /// <value>
        /// The type of the component entity.
        /// </value>
        public EntityTypeCache ComponentEntityType => ComponentEntityTypeId.HasValue ? EntityTypeCache.Read( ComponentEntityTypeId.Value ) : null;


        /// <summary>
        /// Gets the type of the interaction entity.
        /// </summary>
        /// <value>
        /// The type of the interaction entity.
        /// </value>
        public EntityTypeCache InteractionEntityType => InteractionEntityTypeId.HasValue ? EntityTypeCache.Read( InteractionEntityTypeId.Value ) : null;

        /// <summary>
        /// Gets the channel type medium value.
        /// </summary>
        /// <value>
        /// The channel type medium value.
        /// </value>
        public DefinedValueCache ChannelTypeMediumValue => ChannelTypeMediumValueId.HasValue ? DefinedValueCache.Read( ChannelTypeMediumValueId.Value ) : null;

        /// <summary>
        /// Gets the interaction components.
        /// </summary>
        /// <value>
        /// The interaction components.
        /// </value>
        public List<InteractionComponentCache> InteractionComponents
        {
            get
            {
                var components = new List<InteractionComponentCache>();

                InitComponentIds();

                foreach ( var id in componentIds )
                {
                    var component = InteractionComponentCache.Read( id );
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
        private List<int> componentIds;

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the component ids.
        /// </summary>
        private void InitComponentIds()
        {
            lock ( _obj )
            {
                if (componentIds != null) return;

                using ( var rockContext = new RockContext() )
                {
                    componentIds = new InteractionComponentService( rockContext )
                        .GetByChannelId( Id )
                        .Select( v => v.Id )
                        .ToList();
                }
            }
        }

        /// <summary>
        /// Adds the component identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void AddComponentId( int id )
        {
            InitComponentIds();

            lock ( _obj )
            {
                if ( !componentIds.Contains( id ) )
                {
                    componentIds.Add( id );
                }
            }
        }

        /// <summary>
        /// Removes the component identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void RemoveComponentId( int id )
        {
            InitComponentIds();

            lock ( _obj )
            {
                if ( componentIds.Contains( id ) )
                {
                    componentIds.Remove( id );
                }
            }
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if (!(model is InteractionChannel)) return;

            var interactionChannel = (InteractionChannel)model;
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
            componentIds = null;
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache(IEntityCache cacheEntity)
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheInteractionChannel ) ) return;

            var interactionChannel = (CacheInteractionChannel)cacheEntity;
            Name = interactionChannel.Name;
            ChannelEntityId = interactionChannel.ChannelEntityId;
            ChannelTypeMediumValueId = interactionChannel.ChannelTypeMediumValueId;
            ComponentEntityTypeId = interactionChannel.ComponentEntityTypeId;
            ForeignGuid = interactionChannel.ForeignGuid;
            ForeignKey = interactionChannel.ForeignKey;
            InteractionEntityTypeId = interactionChannel.InteractionEntityTypeId;
            RetentionDuration = interactionChannel.RetentionDuration;
            ComponentCacheDuration = interactionChannel.ComponentCacheDuration;
            componentIds = interactionChannel.InteractionComponentIds;
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

        #region Static Methods

        /// <summary>
        /// Returns InteractionChannel object from cache.  If interactionChannel does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static InteractionChannelCache Read( int id, RockContext rockContext = null )
        {
            return new InteractionChannelCache( CacheInteractionChannel.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static InteractionChannelCache Read( Guid guid, RockContext rockContext = null )
        {
            return new InteractionChannelCache( CacheInteractionChannel.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Reads the specified model.
        /// </summary>
        /// <param name="interactionChannelModel">The model.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static InteractionChannelCache Read( InteractionChannel interactionChannelModel, RockContext rockContext = null )
        {
            return new InteractionChannelCache( CacheInteractionChannel.Get( interactionChannelModel ) );
        }

        /// <summary>
        /// Removes interactionChannel from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CacheInteractionChannel.Remove( id );
        }

        #endregion
    }
}