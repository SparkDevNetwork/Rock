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
using System.Data.Entity;
using System.Linq;

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
            return GetOrAddExisting( PersonBadgeCache.CacheKey( id ),
                () => LoadById( id, rockContext ) );
        }

        private static PersonBadgeCache LoadById( int id, RockContext rockContext )
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

        private static PersonBadgeCache LoadById2( int id, RockContext rockContext )
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
            var personBadgeService = new PersonBadgeService( rockContext );
            return personBadgeService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ) )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Adds PersonBadge model to cache, and returns cached object
        /// </summary>
        /// <param name="personBadgeModel">The personBadge model.</param>
        /// <returns></returns>
        public static PersonBadgeCache Read( PersonBadge personBadgeModel )
        {
            return GetOrAddExisting( PersonBadgeCache.CacheKey( personBadgeModel.Id ),
                () => LoadByModel( personBadgeModel ) );
        }

        private static PersonBadgeCache LoadByModel( PersonBadge personBadgeModel )
        {
            if ( personBadgeModel != null )
            {
                return new PersonBadgeCache( personBadgeModel );
            }
            return null;
        }

        /// <summary>
        /// Removes personBadge from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            FlushCache( PersonBadgeCache.CacheKey( id ) );
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