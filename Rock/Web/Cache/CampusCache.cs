﻿// <copyright>
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
using System.Linq;
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
        public string RawServiceTimes { get; set; }

        /// <summary>
        /// Gets the service times.
        /// </summary>
        /// <value>
        /// The service times.
        /// </value>
        public List<ServiceTime> ServiceTimes
        {
            get
            {
                var serviceTimes = new List<ServiceTime>();

                if ( !string.IsNullOrWhiteSpace( RawServiceTimes ) )
                {

                    string[] KeyValues = RawServiceTimes.Split( new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries );
                    foreach ( string keyValue in KeyValues )
                    {
                        var dayTime = keyValue.Split( new char[] { '^' } );
                        if ( dayTime.Length == 2 )
                        {
                            var serviceTime = new ServiceTime();
                            serviceTime.Day = dayTime[0];
                            serviceTime.Time = dayTime[1];
                            serviceTimes.Add( serviceTime );
                        }
                    }
                }

                return serviceTimes;
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
                this.RawServiceTimes = campus.ServiceTimes;
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
            ObjectCache cache = RockMemoryCache.Default;
            CampusCache campus = cache[cacheKey] as CampusCache;

            if ( campus == null )
            {
                rockContext = rockContext ?? new RockContext();
                var campusService = new CampusService( rockContext );
                var campusModel = campusService.Get( id );
                if ( campusModel != null )
                {
                    campusModel.LoadAttributes( rockContext );
                    campus = new CampusCache( campusModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( cacheKey, campus, cachePolicy );
                    cache.Set( campus.Guid.ToString(), campus.Id, cachePolicy );
                }
            }

            return campus;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static CampusCache Read( Guid guid, RockContext rockContext = null )
        {
            ObjectCache cache = RockMemoryCache.Default;
            object cacheObj = cache[guid.ToString()];

            CampusCache campus = null;
            if ( cacheObj != null )
            {
                campus = Read( (int)cacheObj, rockContext );
            }

            if ( campus == null )
            {
                rockContext = rockContext ?? new RockContext();
                var campusService = new CampusService( rockContext );
                var campusModel = campusService.Get( guid );
                if ( campusModel != null )
                {
                    campusModel.LoadAttributes( rockContext );
                    campus = new CampusCache( campusModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( CampusCache.CacheKey( campus.Id ), campus, cachePolicy );
                    cache.Set( campus.Guid.ToString(), campus.Id, cachePolicy );
                }
            }

            return campus;
        }

        /// <summary>
        /// Adds Campus model to cache, and returns cached object
        /// </summary>
        /// <param name="campusModel"></param>
        /// <returns></returns>
        public static CampusCache Read( Campus campusModel )
        {
            string cacheKey = CampusCache.CacheKey( campusModel.Id );
            ObjectCache cache = RockMemoryCache.Default;
            CampusCache campus = cache[cacheKey] as CampusCache;

            if ( campus != null )
            {
                campus.CopyFromModel( campusModel );
            }
            else
            {
                campus = new CampusCache( campusModel );
                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, campus, cachePolicy );
                cache.Set( campus.Guid.ToString(), campus.Id, cachePolicy );
            }

            return campus;
        }

        /// <summary>
        /// Returns all campuses
        /// </summary>
        /// <returns></returns>
        public static List<CampusCache> All( RockContext rockContext = null )
        {
            List<CampusCache> campuses = new List<CampusCache>();

            string allCacheKey = "Rock:Campus:All";
            ObjectCache cache = RockMemoryCache.Default;
            List<int> campusIds = cache[allCacheKey] as List<int>;

            if ( campusIds != null )
            {
                foreach ( int campusId in campusIds )
                {
                    campuses.Add( CampusCache.Read( campusId ) );
                }
            }
            else
            {
                campusIds = new List<int>();

                var cachePolicy = new CacheItemPolicy();

                rockContext = rockContext ?? new RockContext();
                var campusService = new CampusService( rockContext );
                foreach ( var campusModel in campusService.Queryable()
                    .OrderBy( c => c.Name) )
                {
                    campusIds.Add( campusModel.Id );

                    string cacheKey = CampusCache.CacheKey( campusModel.Id );
                    CampusCache campus = cache[cacheKey] as CampusCache;

                    if ( campus == null )
                    {
                        campusModel.LoadAttributes( rockContext );

                        campus = new CampusCache( campusModel );
                        cache.Set( cacheKey, campus, cachePolicy );
                        cache.Set( campus.Guid.ToString(), campus.Id, cachePolicy );
                    }

                    campuses.Add( campus );
                }

                cache.Set( allCacheKey, campusIds, cachePolicy );
            }

            return campuses;
        }

        /// <summary>
        /// Removes campus from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = RockMemoryCache.Default;
            cache.Remove( CampusCache.CacheKey( id ) );
            cache.Remove( "Rock:Campus:All" );
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Special class for adding service times as available liquid fields
        /// </summary>
        [DotLiquid.LiquidType("Day", "Time")]
        public class ServiceTime
        {
            /// <summary>
            /// Gets or sets the day.
            /// </summary>
            /// <value>
            /// The day.
            /// </value>
            public string Day { get; set; }

            /// <summary>
            /// Gets or sets the time.
            /// </summary>
            /// <value>
            /// The time.
            /// </value>
            public string Time { get; set; }

        }

        #endregion    
    }
}