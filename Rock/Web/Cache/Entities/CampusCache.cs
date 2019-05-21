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
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
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
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
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

        #endregion

        #region Public Methods

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
            Url = campus.Url;
            LocationId = campus.LocationId;
            TimeZoneId = campus.TimeZoneId;
            PhoneNumber = campus.PhoneNumber;
            LeaderPersonAliasId = campus.LeaderPersonAliasId;
            RawServiceTimes = campus.ServiceTimes;
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
        /// Returns all campuses from cache
        /// </summary>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <returns></returns>
        public static List<CampusCache> All( bool includeInactive )
        {
            var allCampuses = All();
            return includeInactive ? allCampuses : allCampuses.Where( c => c.IsActive.HasValue && c.IsActive.Value ).ToList();
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Special class for adding service times as available liquid fields
        /// </summary>
		[Serializable]
        [DataContract]
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
        }

        #endregion
    }
}