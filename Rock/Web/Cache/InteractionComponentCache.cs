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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a interactionComponent that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class InteractionComponentCache : CachedModel<InteractionComponent>
    {
        #region Constructors

        private InteractionComponentCache()
        {
        }

        private InteractionComponentCache( InteractionComponent model )
        {
            CopyFromModel( model );
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
        public InteractionChannelCache InteractionChannel
        {
            get { return InteractionChannelCache.Read( ChannelId ); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies the model property values to the DTO properties
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is InteractionComponent )
            {
                var interactionComponent = (InteractionComponent)model;
                this.Name = interactionComponent.Name;
                this.EntityId = interactionComponent.EntityId;
                this.ChannelId = interactionComponent.ChannelId;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:InteractionComponent:{0}", id );
        }

        /// <summary>
        /// Returns InteractionComponent object from cache.  If interactionComponent does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static InteractionComponentCache Read( int id, RockContext rockContext = null )
        {
            return GetOrAddExisting( InteractionComponentCache.CacheKey( id ),
                () => LoadById( id, rockContext ) );
        }

        private static InteractionComponentCache LoadById( int id, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadById2( id, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadById2( id, rockContext2 );
            }
        }

        private static InteractionComponentCache LoadById2( int id, RockContext rockContext )
        {
            var interactionComponentService = new InteractionComponentService( rockContext );
            var interactionComponentModel = interactionComponentService.Get( id );
            if ( interactionComponentModel != null )
            {
                return new InteractionComponentCache( interactionComponentModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static InteractionComponentCache Read( string guid )
        {
            Guid realGuid = guid.AsGuid();
            if ( realGuid.Equals( Guid.Empty ) )
            {
                return null;
            }

            return Read( realGuid );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static InteractionComponentCache Read( Guid guid, RockContext rockContext = null )
        {
            int id = GetOrAddExisting( guid.ToString(),
                () => LoadByGuid( guid, rockContext ) );

            return Read( id, rockContext );
        }

        private static int LoadByGuid( Guid guid, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadByGuid2( guid, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadByGuid2( guid, rockContext2 );
            }
        }


        private static int LoadByGuid2( Guid guid, RockContext rockContext )
        {
            var interactionComponentService = new InteractionComponentService( rockContext );
            return interactionComponentService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ) )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Reads the specified model.
        /// </summary>
        /// <param name="interactionComponentModel">The model.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static InteractionComponentCache Read( InteractionComponent interactionComponentModel, RockContext rockContext = null )
        {
            return GetOrAddExisting( InteractionComponentCache.CacheKey( interactionComponentModel.Id ),
                () => LoadByModel( interactionComponentModel ) );
        }

        /// <summary>
        /// Gets the or add existing.
        /// </summary>
        /// <remarks>
        /// Because ComponentCacheDuration is determined by the channel, this class needs it's own GetOrAddExisting
        /// method.
        /// </remarks>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        private static InteractionComponentCache GetOrAddExisting( string key, Func<InteractionComponentCache> valueFactory )
        {
            RockMemoryCache cache = RockMemoryCache.Default;

            InteractionComponentCache cacheValue = cache.Get( key ) as InteractionComponentCache;
            if ( cacheValue != null )
            {
                return cacheValue;
            }

            InteractionComponentCache value = valueFactory();
            if ( value != null )
            {
                // Because the cache policy for interaction components is defined on the channel, get the channel.
                int? cacheDuration = null;
                var channel = InteractionChannelCache.Read( value.ChannelId );
                if ( channel != null )
                {
                    cacheDuration = channel.ComponentCacheDuration;
                }

                if ( !cacheDuration.HasValue || cacheDuration.Value > 0 )
                {
                    var cacheItemPolicy = new CacheItemPolicy();
                    if ( cacheDuration.HasValue )
                    {
                        cacheItemPolicy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds( cacheDuration.Value );
                    }
                    cache.Set( key, value, cacheItemPolicy );
                }
            }
            return value;
        }

        /// <summary>
        /// Loads by model.
        /// </summary>
        /// <param name="interactionComponentModel">The interaction component model.</param>
        /// <returns></returns>
        private static InteractionComponentCache LoadByModel( InteractionComponent interactionComponentModel )
        {
            if ( interactionComponentModel != null )
            {
                return new InteractionComponentCache( interactionComponentModel );
            }
            return null;
        }

        /// <summary>
        /// Removes interactionComponent from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            FlushCache( InteractionComponentCache.CacheKey( id ) );
        }

        #endregion
    }
}