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
using Newtonsoft.Json;

using Rock.Cache;
using Rock.Data;
using Rock.Model;
using Rock.PersonProfile;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a personBadge that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.CachePersonBadge instead" )]
    public class PersonBadgeCache : CachedModel<PersonBadge>
    {
        #region Constructors

        private PersonBadgeCache()
        {
        }

        private PersonBadgeCache( CachePersonBadge cachePersonBadge )
        {
            CopyFromNewCache( cachePersonBadge );
        }

        #endregion

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
        /// Gets or sets the description
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets the Entity Type.
        /// </summary>
        public EntityTypeCache EntityType => EntityTypeId.HasValue ? EntityTypeCache.Read( EntityTypeId.Value ) : null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is PersonBadge ) ) return;

            var personBadge = (PersonBadge)model;
            Name = personBadge.Name;
            Description = personBadge.Description;
            EntityTypeId = personBadge.EntityTypeId;
            Order = personBadge.Order;
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CachePersonBadge ) ) return;

            var personBadge = (CachePersonBadge)cacheEntity;
            Name = personBadge.Name;
            Description = personBadge.Description;
            EntityTypeId = personBadge.EntityTypeId;
            Order = personBadge.Order;
        }

        /// <summary>
        /// Gets the badge component.
        /// </summary>
        /// <value>
        /// The badge component.
        /// </value>
        public virtual BadgeComponent BadgeComponent => EntityType != null ? BadgeContainer.GetComponent( EntityType.Name ) : null;

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns PersonBadge object from cache.  If personBadge does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static PersonBadgeCache Read( int id, RockContext rockContext = null )
        {
            return new PersonBadgeCache( CachePersonBadge.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static PersonBadgeCache Read( Guid guid, RockContext rockContext = null )
        {
            return new PersonBadgeCache( CachePersonBadge.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Adds PersonBadge model to cache, and returns cached object
        /// </summary>
        /// <param name="personBadgeModel">The personBadge model.</param>
        /// <returns></returns>
        public static PersonBadgeCache Read( PersonBadge personBadgeModel )
        {
            return new PersonBadgeCache( CachePersonBadge.Get( personBadgeModel ) );
        }


        /// <summary>
        /// Removes personBadge from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CachePersonBadge.Remove( id );
        }

        /// <summary>
        /// Froms the json.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public static PersonBadgeCache FromJson( string json )
        {
            return JsonConvert.DeserializeObject( json, typeof( PersonBadgeCache ) ) as PersonBadgeCache;
        }

        #endregion
    }
}