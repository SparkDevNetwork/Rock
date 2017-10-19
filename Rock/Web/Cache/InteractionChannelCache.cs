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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a interactionChannel that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class InteractionChannelCache : CachedModel<InteractionChannel>
    {
        #region Constructors

        private InteractionChannelCache( InteractionChannel model )
        {
            CopyFromModel( model );
        }

        #endregion

        #region Properties

        private object _obj = new object();

        public string Name { get; set; }

        public int? ComponentEntityTypeId { get; set; }

        public int? InteractionEntityTypeId { get; set; }

        public int? ChannelEntityId { get; set; }

        public int? ChannelTypeMediumValueId { get; set; }

        public int? RetentionDuration { get; set; }

        public EntityTypeCache ComponentEntityType
        {
            get
            {
                if ( ComponentEntityTypeId.HasValue )
                {
                    return EntityTypeCache.Read( ComponentEntityTypeId.Value );
                }

                return null;
            }
        }


        public EntityTypeCache InteractionEntityType
        {
            get
            {
                if ( InteractionEntityTypeId.HasValue )
                {
                    return EntityTypeCache.Read( InteractionEntityTypeId.Value );
                }

                return null;
            }
        }

        public DefinedValueCache ChannelTypeMediumValue
        {
            get
            {
                if ( ChannelTypeMediumValueId.HasValue )
                {
                    return DefinedValueCache.Read( ChannelTypeMediumValueId.Value );
                }

                return null;
            }
        }

        public List<InteractionComponentCache> InteractionComponents
        {
            get
            {
                var components = new List<InteractionComponentCache>();

                InitComponentIds();

                foreach ( int id in componentIds )
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
        private List<int> componentIds = null;

        #endregion

        #region Public Methods

        private void InitComponentIds()
        {
            lock ( _obj )
            {
                if ( componentIds == null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        componentIds = new Model.InteractionComponentService( rockContext )
                            .GetByChannelId( this.Id )
                            .Select( v => v.Id )
                            .ToList();
                    }
                }
            }
        }

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
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is InteractionChannel )
            {
                var interactionChannel = (InteractionChannel)model;
                this.Name = interactionChannel.Name;
                this.ChannelEntityId = interactionChannel.ChannelEntityId;
                this.ChannelTypeMediumValueId = interactionChannel.ChannelTypeMediumValueId;
                this.ComponentEntityTypeId = interactionChannel.ComponentEntityTypeId;
                this.ForeignGuid = interactionChannel.ForeignGuid;
                this.ForeignKey = interactionChannel.ForeignKey;
                this.InteractionEntityTypeId = interactionChannel.InteractionEntityTypeId;
                this.RetentionDuration = interactionChannel.RetentionDuration;

                // set componentIds to null so it load them all at once on demand
                this.componentIds = null;
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
            return string.Format( "Rock:InteractionChannel:{0}", id );
        }

        /// <summary>
        /// Returns InteractionChannel object from cache.  If interactionChannel does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static InteractionChannelCache Read( int id, RockContext rockContext = null )
        {
            return GetOrAddExisting( InteractionChannelCache.CacheKey( id ),
                () => LoadById( id, rockContext ) );
        }

        private static InteractionChannelCache LoadById( int id, RockContext rockContext )
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

        private static InteractionChannelCache LoadById2( int id, RockContext rockContext )
        {
            var interactionChannelService = new InteractionChannelService( rockContext );
            var interactionChannelModel = interactionChannelService
                .Queryable()
                .Where( t => t.Id == id )
                .FirstOrDefault();

            if ( interactionChannelModel != null )
            {
                return new InteractionChannelCache( interactionChannelModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static InteractionChannelCache Read( Guid guid, RockContext rockContext = null )
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
            var interactionChannelService = new InteractionChannelService( rockContext );
            return interactionChannelService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ) )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Reads the specified model.
        /// </summary>
        /// <param name="interactionChannelModel">The model.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static InteractionChannelCache Read( InteractionChannel interactionChannelModel, RockContext rockContext = null )
        {
            return GetOrAddExisting( InteractionChannelCache.CacheKey( interactionChannelModel.Id ),
                () => LoadByModel( interactionChannelModel ) );
        }

        private static InteractionChannelCache LoadByModel( InteractionChannel interactionChannelModel )
        {
            if ( interactionChannelModel != null )
            {
                return new InteractionChannelCache( interactionChannelModel );
            }
            return null;
        }

        /// <summary>
        /// Removes interactionChannel from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            FlushCache( InteractionChannelCache.CacheKey( id ) );
        }

        #endregion
    }
}