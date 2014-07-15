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
using System.Collections.Generic;
using System.Runtime.Caching;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a campus that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class CampusCache : CachedModel<Campus>
    {
        #region Constructors

        private CampusCache() 
        {
        }

        private CampusCache( Campus campus )
        {
            CopyFromModel( campus );
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the is active.
        /// </summary>
        /// <value>
        /// The is active.
        /// </value>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets the short code.
        /// </summary>
        /// <value>
        /// The short code.
        /// </value>
        public string ShortCode { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>
        /// The phone number.
        /// </value>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the leader person alias identifier.
        /// </summary>
        /// <value>
        /// The leader person alias identifier.
        /// </value>
        public int? LeaderPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the service times.
        /// </summary>
        /// <value>
        /// The service times.
        /// </value>
        public string ServiceTimes { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is Campus )
            {
                var campus = (Campus)model;
                this.IsSystem = campus.IsSystem;
                this.Name = campus.Name;
                this.Description = campus.Description;
                this.IsActive = campus.IsActive;
                this.ShortCode = campus.ShortCode;
                this.Url = campus.Url;
                this.LocationId = campus.LocationId;
                this.PhoneNumber = campus.PhoneNumber;
                this.LeaderPersonAliasId = campus.LeaderPersonAliasId;
                this.ServiceTimes = campus.ServiceTimes;
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

        /// <summary>
        /// Gets the cache key for the selected campu id.
        /// </summary>
        /// <param name="id">The campus id.</param>
        /// <returns></returns>
        public static string CacheKey( int id )
        {
            return string.Format( "Rock:Campus:{0}", id );
        }

        /// <summary>
        /// Returns Campus object from cache.  If campus does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static CampusCache Read( int id, RockContext rockContext = null )
        {
            string cacheKey = CampusCache.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            CampusCache campus = cache[cacheKey] as CampusCache;

            if ( campus != null )
            {
                return campus;
            }
            else
            {
                CampusService campusService = new CampusService( rockContext ?? new RockContext() );
                Campus campusModel = campusService.Get( id );
                if ( campusModel != null )
                {
                    campusModel.LoadAttributes();
                    campus = new CampusCache( campusModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( cacheKey, campus, cachePolicy );
                    cache.Set( campus.Guid.ToString(), campus.Id, cachePolicy );
                    
                    return campus;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static CampusCache Read( Guid guid, RockContext rockContext = null )
        {
            ObjectCache cache = MemoryCache.Default;
            object cacheObj = cache[guid.ToString()];

            if ( cacheObj != null )
            {
                return Read( (int)cacheObj );
            }
            else
            {
                var campusService = new CampusService( rockContext ?? new RockContext() );
                var campusModel = campusService.Get( guid );
                if ( campusModel != null )
                {
                    campusModel.LoadAttributes();
                    var campus = new CampusCache( campusModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( CampusCache.CacheKey( campus.Id ), campus, cachePolicy );
                    cache.Set( campus.Guid.ToString(), campus.Id, cachePolicy );

                    return campus;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// Adds Campus model to cache, and returns cached object
        /// </summary>
        /// <param name="campusModel"></param>
        /// <returns></returns>
        public static CampusCache Read( Campus campusModel )
        {
            string cacheKey = CampusCache.CacheKey( campusModel.Id );

            ObjectCache cache = MemoryCache.Default;
            CampusCache campus = cache[cacheKey] as CampusCache;

            if ( campus != null )
            {
                campus.CopyFromModel( campusModel );
                return campus;
            }
            else
            {
                campus = new CampusCache( campusModel );
                
                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, campus, cachePolicy );
                cache.Set( campus.Guid.ToString(), campus.Id, cachePolicy );
                
                return campus;
            }
        }

        /// <summary>
        /// Removes campus from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( CampusCache.CacheKey( id ) );
        }

        #endregion
    }
}