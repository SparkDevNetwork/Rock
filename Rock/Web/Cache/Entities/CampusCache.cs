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
using System.Data.Entity.Spatial;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Lava;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a campus that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class CampusCache : ModelCache<CampusCache, Campus>
    {

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the is active.
        /// </summary>
        /// <value>
        /// The is active.
        /// </value>
        [DataMember]
        public bool? IsActive { get; private set; }

        /// <summary>
        /// Gets or sets the short code.
        /// </summary>
        /// <value>
        /// The short code.
        /// </value>
        [DataMember]
        public string ShortCode { get; private set; }

        /// <summary>
        /// Gets the condensed name of the campus. This will be the short code
        /// if set, otherwise the name of the campus by stripping off any
        /// trailing " Campus" text.
        /// </summary>
        /// <value>
        /// The condensed name of the campus.
        /// </value>
        [DataMember]
        public string CondensedName { get; private set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        [DataMember]
        public string Url { get; private set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        [DataMember]
        public int? LocationId { get; private set; }

        /// <summary>
        /// Gets or sets the time zone identifier (<see cref="System.TimeZoneInfo.Id"/>)
        /// If this is not set, the Campus time zone will be the default Rock time zone (<see cref="Rock.RockDateTime.OrgTimeZoneInfo" /> )
        /// </summary>
        /// <value>
        /// The time zone identifier. 
        /// </value>
        [DataMember]
        public string TimeZoneId { get; private set; }

        /// <summary>
        /// Gets or sets the campus status value identifier.
        /// </summary>
        /// <value>
        /// The campus status value identifier.
        /// </value>
        [DataMember]
        public int? CampusStatusValueId { get; private set; }

        /// <summary>
        /// Gets or sets the campus type value identifier.
        /// </summary>
        /// <value>
        /// The campus type value identifier.
        /// </value>
        [DataMember]
        public int? CampusTypeValueId { get; private set; }

        /// <summary>
        /// Gets or sets the team group identifier.
        /// </summary>
        /// <value>
        /// The team group identifier.
        /// </value>
        [DataMember]
        public int? TeamGroupId { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        [DataMember]
        public CampusLocation Location { get; private set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>
        /// The phone number.
        /// </value>
        [DataMember]
        public string PhoneNumber { get; private set; }

        /// <summary>
        /// Gets or sets the leader person alias identifier.
        /// </summary>
        /// <value>
        /// The leader person alias identifier.
        /// </value>
        [DataMember]
        public int? LeaderPersonAliasId { get; private set; }

        /// <summary>
        /// Gets or sets the service times.
        /// </summary>
        /// <value>
        /// The service times.
        /// </value>
        [DataMember]
        public string RawServiceTimes { get; private set; }

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

                var keyValues = RawServiceTimes.Split( new[] { '|' }, StringSplitOptions.RemoveEmptyEntries );

                serviceTimes.AddRange(
                    keyValues
                        .Select( k => k.Split( '^' ) )
                        .Where( d => d.Length == 2 )
                        .Select( d => new ServiceTime
                        {
                            Day = d[0],
                            Time = d[1]
                        } )
                    );

                return serviceTimes;
            }
        }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.Schedule"/> ids that are associated with this Campus.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.Schedule"/> ids that are associated with this Campus.
        /// </value>
        public virtual List<int> CampusScheduleIds { get; set; } = new List<int>();

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets the current date time.
        /// </summary>
        /// <value>
        /// The current date time.
        /// </value>
        public DateTime CurrentDateTime
        {
            get
            {
                if ( TimeZoneId.IsNotNullOrWhiteSpace() )
                {
                    var campusTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById( TimeZoneId );
                    if ( campusTimeZoneInfo != null )
                    {
                        return TimeZoneInfo.ConvertTime( DateTime.UtcNow, campusTimeZoneInfo );
                    }
                }

                return RockDateTime.Now;
            }
        }

        /// <summary>
        /// Gets the single campus from cache, if only one active campus exists.
        /// </summary>
        /// <value>
        /// The single campus.
        /// </value>
        public static CampusCache SingleCampus
        {
            get
            {
                var all = All( false );
                return all.Count == 1
                    ? all.First()
                    : null;
            }
        }

        /// <summary>
        /// Gets the single campus id from cache, if only one active campus exists.
        /// </summary>
        /// <value>
        /// The single campus identifier.
        /// </value>
        public static int? SingleCampusId => SingleCampus?.Id;

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks if the campus geofence contains the specified point.
        /// </summary>
        /// <param name="latitude">The latitude of the point.</param>
        /// <param name="longitude">The longitude of the point.</param>
        /// <returns><c>true</c> if the campus has a geofence and it contains the point; otherwise <c>false</c>.</returns>
        internal bool ContainsGeoPoint( double latitude, double longitude )
        {
            var geoPoint = DbGeography.FromText( $"POINT({longitude} {latitude})" );

            return ContainsGeoPoint( geoPoint );
        }

        /// <summary>
        /// Checks if the campus geofence contains the specified point.
        /// </summary>
        /// <param name="geoPoint">The point to be checked.</param>
        /// <returns><c>true</c> if the campus has a geofence and it contains the point; otherwise <c>false</c>.</returns>
        internal bool ContainsGeoPoint( DbGeography geoPoint )
        {
            if ( Location?.GeoFence == null )
            {
                return false;
            }

            return geoPoint.Intersects( Location.GeoFence );
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var campus = entity as Campus;
            if ( campus == null ) return;

            IsSystem = campus.IsSystem;
            Name = campus.Name;
            Description = campus.Description;
            IsActive = campus.IsActive;
            ShortCode = campus.ShortCode;
            CondensedName = campus.CondensedName;
            Url = campus.Url;
            LocationId = campus.LocationId;
            TimeZoneId = campus.TimeZoneId;
            CampusStatusValueId = campus.CampusStatusValueId;
            CampusTypeValueId = campus.CampusTypeValueId;
            TeamGroupId = campus.TeamGroupId;
            PhoneNumber = campus.PhoneNumber;
            LeaderPersonAliasId = campus.LeaderPersonAliasId;
            RawServiceTimes = campus.ServiceTimes;
            CampusScheduleIds = campus.CampusSchedules.Select( s => s.ScheduleId ).ToList();
            Order = campus.Order;

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
        /// Returns all campuses from cache ordered by the campus's order property.
        /// </summary>
        /// <returns></returns>
        public static new List<CampusCache> All()
        {
            // Calls the method below including inactive campuses too.
            return All( true );
        }

        /// <summary>
        /// Returns all campuses from cache ordered by the campus's order property.
        /// </summary>
        /// <param name="includeInactive">if set to true to include inactive campuses; otherwise set to false to exclude them.</param>
        /// <returns></returns>
        public static List<CampusCache> All( bool includeInactive )
        {
            // WARNING: We need to call the All(RockContext) static method here, not the All(bool) or All() static method.
            // Otherwise a stack overflow loop will occur.
            var allCampuses = All( null );

            if ( includeInactive )
            {
                return allCampuses.OrderBy( c => c.Order ).ToList();
            }
            else
            {
                return allCampuses.Where( c => c.IsActive.HasValue && c.IsActive.Value ).OrderBy( c => c.Order ).ToList();
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Special class for adding service times as available liquid fields
        /// </summary>
		[Serializable]
        [DataContract]
        [LavaType( "Day", "Time" )]
        [DotLiquid.LiquidType( "Day", "Time" )]
        public class ServiceTime
        {
            /// <summary>
            /// Gets or sets the day.
            /// </summary>
            /// <value>
            /// The day.
            /// </value>
            [DataMember]
            public string Day { get; internal set; }

            /// <summary>
            /// Gets or sets the time.
            /// </summary>
            /// <value>
            /// The time.
            /// </value>
            [DataMember]
            public string Time { get; internal set; }
        }

        /// <summary>
        /// Special class for adding location info as available liquid fields
        /// </summary>
        [Serializable]
        [DataContract]
        [LavaType( "Street1", "Street2", "City", "State", "PostalCode", "Country", "Latitude", "Longitude", "ImageUrl" )]
        [DotLiquid.LiquidType( "Street1", "Street2", "City", "State", "PostalCode", "Country", "Latitude", "Longitude", "ImageUrl" )]
        public class CampusLocation
        {
            /// <summary>
            /// Gets or sets the street1.
            /// </summary>
            /// <value>
            /// The street1.
            /// </value>
			[DataMember]
            public string Street1 { get; private set; }

            /// <summary>
            /// Gets or sets the street2.
            /// </summary>
            /// <value>
            /// The street2.
            /// </value>
            [DataMember]
            public string Street2 { get; private set; }

            /// <summary>
            /// Gets or sets the city.
            /// </summary>
            /// <value>
            /// The city.
            /// </value>
            [DataMember]
            public string City { get; private set; }

            /// <summary>
            /// Gets or sets the state.
            /// </summary>
            /// <value>
            /// The state.
            /// </value>
            [DataMember]
            public string State { get; private set; }

            /// <summary>
            /// Gets or sets the postal code.
            /// </summary>
            /// <value>
            /// The postal code.
            /// </value>
            [DataMember]
            public string PostalCode { get; private set; }

            /// <summary>
            /// Gets or sets the country.
            /// </summary>
            /// <value>
            /// The country.
            /// </value>
            [DataMember]
            public string Country { get; private set; }

            /// <summary>
            /// Gets or sets the latitude.
            /// </summary>
            /// <value>
            /// The latitude.
            /// </value>
            [DataMember]
            public double? Latitude { get; private set; }

            /// <summary>
            /// Gets or sets the longitude.
            /// </summary>
            /// <value>
            /// The longitude.
            /// </value>
            [DataMember]
            public double? Longitude { get; private set; }

            /// <summary>
            /// Gets the geofence defined for the campuses location.
            /// </summary>
            /// <value>
            /// The geofence defined for the campuses location.
            /// </value>
            internal System.Data.Entity.Spatial.DbGeography GeoFence { get; }

            /// <summary>
            /// Gets or sets the URL for the image.
            /// </summary>
            /// <value>
            /// The image url.
            /// </value>
            [DataMember]
            public string ImageUrl { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="CampusLocation"/> class.
            /// </summary>
            /// <param name="locationModel">The location model.</param>
            public CampusLocation( Location locationModel )
            {
                if ( locationModel == null )
                {
                    return;
                }

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

                GeoFence = locationModel.GeoFence;
            }
        }

        #endregion
    }
}