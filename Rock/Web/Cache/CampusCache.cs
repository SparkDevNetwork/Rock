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
using System.Data.Entity;
using System.Linq;

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
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public CampusLocation Location { get; set; }

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

                this.Location = new CampusLocation( campus.Location );
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
            return GetOrAddExisting( CampusCache.CacheKey( id ), 
                () => LoadById( id, rockContext ) );
        }

        private static CampusCache LoadById( int id, RockContext rockContext )
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

        private static CampusCache LoadById2( int id, RockContext rockContext )
        {
            var campusService = new CampusService( rockContext );
            var campusModel = campusService
                .Queryable( "Location" ).AsNoTracking()
                .FirstOrDefault( c => c.Id == id );
            if ( campusModel != null )
            {
                campusModel.LoadAttributes( rockContext );
                return new CampusCache( campusModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static CampusCache Read( Guid guid, RockContext rockContext = null )
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
            var campusService = new CampusService( rockContext );
            return campusService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ))
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Adds Campus model to cache, and returns cached object
        /// </summary>
        /// <param name="campusModel"></param>
        /// <returns></returns>
        public static CampusCache Read( Campus campusModel )
        {
            return GetOrAddExisting( CampusCache.CacheKey( campusModel.Id ),
                () => LoadByModel( campusModel ) );
        }

        private static CampusCache LoadByModel( Campus campusModel )
        {
            if ( campusModel != null )
            {
                return new CampusCache( campusModel );
            }
            return null;
        }

        /// <summary>
        /// Returns all campuses
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        [Obsolete( "Use All() method instead. RockContext parameter is no longer needed." )]
        public static List<CampusCache> All( RockContext rockContext )
        {
            return All();
        }

        /// <summary>
        /// Returns all campuses
        /// </summary>
        /// <returns></returns>
        public static List<CampusCache> All()
        {
            List<CampusCache> campuses = new List<CampusCache>();
            var campusIds = GetOrAddExisting( "Rock:Campus:All", () => LoadAll() );
            if ( campusIds != null )
            {
                foreach ( int campusId in campusIds )
                {
                    campuses.Add( CampusCache.Read( campusId ) );
                }
            }
            return campuses;
        }

        private static List<int> LoadAll()
        {
            using ( var rockContext = new RockContext() )
            {
                return new CampusService( rockContext )
                    .Queryable().AsNoTracking()
                    .OrderBy( c => c.Name )
                    .Select( c => c.Id )
                    .ToList();
            }
        }

        /// <summary>
        /// Removes campus from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            FlushCache( CampusCache.CacheKey( id ) );
            FlushCache( "Rock:Campus:All" );
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Special class for adding service times as available liquid fields
        /// </summary>
        [DotLiquid.LiquidType( "Day", "Time" )]
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

        /// <summary>
        /// Special class for adding location info as available liquid fields
        /// </summary>
        [DotLiquid.LiquidType( "Street1", "Street2", "City", "State", "PostalCode", "Country", "Latitude", "Longitude", "ImageUrl" )]
        public class CampusLocation
        {
            /// <summary>
            /// Gets or sets the street1.
            /// </summary>
            /// <value>
            /// The street1.
            /// </value>
            public string Street1 { get; set; }

            /// <summary>
            /// Gets or sets the street2.
            /// </summary>
            /// <value>
            /// The street2.
            /// </value>
            public string Street2 { get; set; }

            /// <summary>
            /// Gets or sets the city.
            /// </summary>
            /// <value>
            /// The city.
            /// </value>
            public string City { get; set; }

            /// <summary>
            /// Gets or sets the state.
            /// </summary>
            /// <value>
            /// The state.
            /// </value>
            public string State { get; set; }

            /// <summary>
            /// Gets or sets the postal code.
            /// </summary>
            /// <value>
            /// The postal code.
            /// </value>
            public string PostalCode { get; set; }

            /// <summary>
            /// Gets or sets the country.
            /// </summary>
            /// <value>
            /// The country.
            /// </value>
            public string Country { get; set; }

            /// <summary>
            /// Gets or sets the latitude.
            /// </summary>
            /// <value>
            /// The latitude.
            /// </value>
            public double? Latitude { get; set; }

            /// <summary>
            /// Gets or sets the longitude.
            /// </summary>
            /// <value>
            /// The longitude.
            /// </value>
            public double? Longitude { get; set; }

            /// <summary>
            /// Gets or sets the URL for the image.
            /// </summary>
            /// <value>
            /// The image url.
            /// </value>
            public string ImageUrl { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="CampusLocation"/> class.
            /// </summary>
            /// <param name="locationModel">The location model.</param>
            public CampusLocation( Rock.Model.Location locationModel )
            {
                if ( locationModel != null )
                {
                    Street1 = locationModel.Street1;
                    Street2 = locationModel.Street2;
                    City = locationModel.City;
                    State = locationModel.State;
                    PostalCode = locationModel.PostalCode;
                    Country = locationModel.Country;

                    if ( locationModel.Image != null )
                    {
                        ImageUrl = locationModel.Image.Url;
                    }

                    if ( locationModel.GeoPoint != null )
                    {
                        Latitude = locationModel.GeoPoint.Latitude;
                        Longitude = locationModel.GeoPoint.Longitude;
                    }
                }
            }
        }

        #endregion
    }
}