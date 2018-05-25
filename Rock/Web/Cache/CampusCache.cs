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

using Rock.Cache;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a campus that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.CacheCampus instead" )]
    public class CampusCache : CachedModel<Campus>
    {
        #region Constructors

        private CampusCache()
        {
        }

        private CampusCache( CacheCampus cacheCampus )
        {
            CopyFromNewCache( cacheCampus );
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
        /// Gets or sets the time zone identifier (<see cref="System.TimeZoneInfo.Id"/>)
        /// If this is not set, the Campus time zone will be the default Rock time zone (<see cref="Rock.RockDateTime.OrgTimeZoneInfo" /> )
        /// </summary>
        /// <value>
        /// The time zone identifier. 
        /// </value>
        public string TimeZoneId { get; set; }

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

                if ( string.IsNullOrWhiteSpace( RawServiceTimes ) ) return serviceTimes;
                var KeyValues = RawServiceTimes.Split( new[] { '|' }, StringSplitOptions.RemoveEmptyEntries );

                serviceTimes.AddRange(
                    from keyValue in KeyValues
                    select keyValue.Split( '^' )
                    into dayTime
                    where dayTime.Length == 2
                    select new ServiceTime
                    {
                        Day = dayTime[0],
                        Time = dayTime[1]
                    } );

                return serviceTimes;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is Campus ) ) return;

            var campus = (Campus)model;
            IsSystem = campus.IsSystem;
            Name = campus.Name;
            Description = campus.Description;
            IsActive = campus.IsActive;
            ShortCode = campus.ShortCode;
            Url = campus.Url;
            LocationId = campus.LocationId;
            TimeZoneId = campus.TimeZoneId;
            PhoneNumber = campus.PhoneNumber;
            LeaderPersonAliasId = campus.LeaderPersonAliasId;
            RawServiceTimes = campus.ServiceTimes;

            Location = new CampusLocation( campus.Location );
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheCampus ) ) return;

            var campus = (CacheCampus)cacheEntity;
            IsSystem = campus.IsSystem;
            Name = campus.Name;
            Description = campus.Description;
            IsActive = campus.IsActive;
            ShortCode = campus.ShortCode;
            Url = campus.Url;
            LocationId = campus.LocationId;
            TimeZoneId = campus.TimeZoneId;
            PhoneNumber = campus.PhoneNumber;
            LeaderPersonAliasId = campus.LeaderPersonAliasId;
            RawServiceTimes = campus.RawServiceTimes;
            Location = new CampusLocation( campus.Location );
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
        /// Returns Campus object from cache.  If campus does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static CampusCache Read( int id, RockContext rockContext = null )
        {
            return new CampusCache( CacheCampus.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static CampusCache Read( Guid guid, RockContext rockContext = null )
        {
            return new CampusCache( CacheCampus.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Adds Campus model to cache, and returns cached object
        /// </summary>
        /// <param name="campusModel"></param>
        /// <returns></returns>
        public static CampusCache Read( Campus campusModel )
        {
            return new CampusCache( CacheCampus.Get( campusModel ) );
        }

        /// <summary>
        /// Returns all campuses
        /// </summary>
        /// <returns></returns>
        public static List<CampusCache> All()
        {
            return All( true );
        }

        /// <summary>
        /// Returns all campuses
        /// </summary>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <returns></returns>
        public static List<CampusCache> All( bool includeInactive )
        {
            var campuses = new List<CampusCache>();

            var cacheCampuses = CacheCampus.All( includeInactive );
            if ( cacheCampuses == null ) return campuses;

            foreach ( var cacheCampus in cacheCampuses )
            {
                campuses.Add( new CampusCache( cacheCampus ) );
            }

            return campuses;
        }

        /// <summary>
        /// Removes campus from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CacheCampus.Remove( id );
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
            public CampusLocation( Location locationModel )
            {
                if ( locationModel == null ) return;

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

                if ( locationModel.GeoPoint == null ) return;

                Latitude = locationModel.GeoPoint.Latitude;
                Longitude = locationModel.GeoPoint.Longitude;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CampusLocation"/> class.
            /// </summary>
            /// <param name="campusLocation">The campus location.</param>
            public CampusLocation( CacheCampus.CampusLocation campusLocation )
            {
                if ( campusLocation == null ) return;

                Street1 = campusLocation.Street1;
                Street2 = campusLocation.Street2;
                City = campusLocation.City;
                State = campusLocation.State;
                PostalCode = campusLocation.PostalCode;
                Country = campusLocation.Country;
                ImageUrl = campusLocation.ImageUrl;
                Latitude = campusLocation.Latitude;
                Longitude = campusLocation.Longitude;
            }
        }

        #endregion
    }
}