// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Runtime.Caching;

using Newtonsoft.Json;

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
    public class PersonBadgeCache : CachedModel<PersonBadge>
    {
        #region Constructors

        private PersonBadgeCache()
        {
        }

        private PersonBadgeCache( PersonBadge personBadge )
        {
            CopyFromModel( personBadge );
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
        /// Gets or sets the description
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets the Entity Type.
        /// </summary>
        public EntityTypeCache EntityType
        {
            get
            {
                if ( EntityTypeId.HasValue )
                {
                    return EntityTypeCache.Read( EntityTypeId.Value );
                }

                return null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is PersonBadge )
            {
                var personBadge = (PersonBadge)model;
                this.Name = personBadge.Name;
                this.Description = personBadge.Description;
                this.EntityTypeId = personBadge.EntityTypeId;
                this.Order = personBadge.Order;
            }
        }

        /// <summary>
        /// Gets the <see cref="Rock.Workflow.ActionComponent"/>
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Workflow.ActionComponent"/>
        /// </value>
        public virtual BadgeComponent BadgeComponent
        {
            get
            {
                if ( EntityType != null )
                {
                    return BadgeContainer.GetComponent( EntityType.Name );
                }
                return null;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:PersonBadge:{0}", id );
        }

        /// <summary>
        /// Returns PersonBadge object from cache.  If personBadge does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static PersonBadgeCache Read( int id, RockContext rockContext = null )
        {
            string cacheKey = PersonBadgeCache.CacheKey( id );

            ObjectCache cache = RockMemoryCache.Default;
            PersonBadgeCache personBadge = cache[cacheKey] as PersonBadgeCache;

            if ( personBadge == null )
            {
                if ( rockContext != null )
                {
                    personBadge = LoadById( id, rockContext );
                }
                else
                {
                    using ( var myRockContext = new RockContext() )
                    {
                        personBadge = LoadById( id, myRockContext );
                    }
                }

                if ( personBadge != null )
                {
                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( cacheKey, personBadge, cachePolicy );
                    cache.Set( personBadge.Guid.ToString(), personBadge.Id, cachePolicy );
                }
            }

            return personBadge;
        }

        private static PersonBadgeCache LoadById( int id, RockContext rockContext )
        {
            var personBadgeService = new PersonBadgeService( rockContext );
            var personBadgeModel = personBadgeService.Get( id );
            if ( personBadgeModel != null )
            {
                personBadgeModel.LoadAttributes( rockContext );
                return new PersonBadgeCache( personBadgeModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static PersonBadgeCache Read( Guid guid, RockContext rockContext = null )
        {
            ObjectCache cache = RockMemoryCache.Default;
            object cacheObj = cache[guid.ToString()];

            PersonBadgeCache personBadge = null;
            if ( cacheObj != null )
            {
                personBadge = Read( (int)cacheObj, rockContext );
            }

            if ( personBadge == null )
            {
                if ( rockContext != null )
                {
                    personBadge = LoadByGuid( guid, rockContext );
                }
                else
                {
                    using ( var myRockContext = new RockContext() )
                    {
                        personBadge = LoadByGuid( guid, myRockContext );
                    }
                }

                if ( personBadge != null )
                {
                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( PersonBadgeCache.CacheKey( personBadge.Id ), personBadge, cachePolicy );
                    cache.Set( personBadge.Guid.ToString(), personBadge.Id, cachePolicy );
                }
            }

            return personBadge;
        }

        private static PersonBadgeCache LoadByGuid( Guid guid, RockContext rockContext )
        {
            var personBadgeService = new PersonBadgeService( rockContext );
            var personBadgeModel = personBadgeService.Get( guid );
            if ( personBadgeModel != null )
            {
                personBadgeModel.LoadAttributes( rockContext );
                return new PersonBadgeCache( personBadgeModel );
            }

            return null;
        }

        /// <summary>
        /// Adds PersonBadge model to cache, and returns cached object
        /// </summary>
        /// <param name="personBadgeModel">The personBadge model.</param>
        /// <returns></returns>
        public static PersonBadgeCache Read( PersonBadge personBadgeModel )
        {
            string cacheKey = PersonBadgeCache.CacheKey( personBadgeModel.Id );
            ObjectCache cache = RockMemoryCache.Default;
            PersonBadgeCache personBadge = cache[cacheKey] as PersonBadgeCache;

            if ( personBadge != null )
            {
                personBadge.CopyFromModel( personBadgeModel );
            }
            else
            {
                personBadge = new PersonBadgeCache( personBadgeModel );
                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, personBadge, cachePolicy );
                cache.Set( personBadge.Guid.ToString(), personBadge.Id, cachePolicy );
            }

            return personBadge;
        }

        /// <summary>
        /// Removes personBadge from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = RockMemoryCache.Default;
            cache.Remove( PersonBadgeCache.CacheKey( id ) );
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