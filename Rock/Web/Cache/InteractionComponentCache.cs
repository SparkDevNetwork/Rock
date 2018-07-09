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

using Rock.Cache;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a interactionComponent that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    [Obsolete( "Use Rock.Cache.CacheInteractionComponent instead" )]
    public class InteractionComponentCache : CachedModel<InteractionComponent>
    {
        #region Constructors

        private InteractionComponentCache()
        {
        }

        private InteractionComponentCache( CacheInteractionComponent cacheInteractionComponent )
        {
            CopyFromNewCache( cacheInteractionComponent );
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the channel identifier.
        /// </summary>
        /// <value>
        /// The channel identifier.
        /// </value>
        public int ChannelId { get; set; }

        /// <summary>
        /// Gets the interaction channel.
        /// </summary>
        /// <value>
        /// The interaction channel.
        /// </value>
        public InteractionChannelCache InteractionChannel => InteractionChannelCache.Read( ChannelId );

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies the model property values to the DTO properties
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is InteractionComponent ) ) return;

            var interactionComponent = (InteractionComponent)model;
            Name = interactionComponent.Name;
            EntityId = interactionComponent.EntityId;
            ChannelId = interactionComponent.ChannelId;
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheInteractionComponent ) ) return;

            var interactionComponent = (CacheInteractionComponent)cacheEntity;
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

        #region Static Methods

        /// <summary>
        /// Returns InteractionComponent object from cache.  If interactionComponent does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static InteractionComponentCache Read( int id, RockContext rockContext = null )
        {
            return new InteractionComponentCache( CacheInteractionComponent.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static InteractionComponentCache Read( string guid )
        {
            return new InteractionComponentCache( CacheInteractionComponent.Get( guid ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static InteractionComponentCache Read( Guid guid, RockContext rockContext = null )
        {
            return new InteractionComponentCache( CacheInteractionComponent.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Reads the specified model.
        /// </summary>
        /// <param name="interactionComponentModel">The model.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static InteractionComponentCache Read( InteractionComponent interactionComponentModel, RockContext rockContext = null )
        {
            return new InteractionComponentCache( CacheInteractionComponent.Get( interactionComponentModel ) );
        }

        /// <summary>
        /// Removes interactionComponent from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CacheInteractionComponent.Remove( id );
        }

        #endregion
    }
}