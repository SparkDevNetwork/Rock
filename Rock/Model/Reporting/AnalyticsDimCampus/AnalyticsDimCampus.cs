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
using Rock.Data;
using Rock.Utility;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.Spatial;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// AnalyticsDimCampus is SQL View based on AnalyticsSourceCampus
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsDimCampus" )]
    [DataContract]
    [CodeGenExclude( CodeGenFeature.ViewModelFile )]
    [Rock.SystemGuid.EntityTypeGuid( "DCEB0575-1351-4CFF-BA4F-410BA2D638CB")]
    public class AnalyticsDimCampus : AnalyticsSourceCampusBase<AnalyticsDimCampus>
    {
        // Note:  Additional fields inherited from AnalyticsSourceCampusBase.

        #region Entity Properties

        /// <summary>
        /// Gets or sets the full name of the leader (from LeaderAliasPerson.Person.FullName)
        /// </summary>
        /// <value>
        /// The full name of the leader.
        /// </value>
        [DataMember]
        public string LeaderFullName { get; set; }

        /// <summary>
        /// Gets or sets the leader person identifier (from LeaderAliasPerson.PersonId)
        /// </summary>
        /// <value>
        /// The leader person identifier.
        /// </value>
        [DataMember]
        public int? LeaderPersonId { get; set; }

        #region Address Fields

        /// <summary>
        /// Gets or sets the first line of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the First line of the Location's Street/Mailing Address. If the Location does not have
        /// a Street/Mailing address, this value is null.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string AddressStreet1 { get; set; }

        /// <summary>
        /// Gets or sets the second line of the Location's Street/Mailing Address. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the second line of the Location's Street/Mailing Address. if this Location does not have 
        /// Street/Mailing Address or if the address does not have a 2nd line, this value is null.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string AddressStreet2 { get; set; }

        /// <summary>
        /// Gets or sets the city component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the city component of the Location's Street/Mailing Address. If this Location does not have
        /// a Street/Mailing Address this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string AddressCity { get; set; }

        /// <summary>
        /// Gets or sets the county.
        /// </summary>
        /// <value>
        /// The county.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string AddressCounty { get; set; }

        /// <summary>
        /// Gets or sets the State component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the state component of the Location's Street/Mailing Address. If this Location does not have 
        /// a Street/Mailing Address, this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string AddressState { get; set; }

        /// <summary>
        /// Gets or sets the country component of the Location's Street/Mailing Address. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the country component of the Location's Street/Mailing Address. If this Location does not have a 
        /// Street/Mailing Address, this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string AddressCountry { get; set; }

        /// <summary>
        /// Gets or sets the Zip/Postal Code component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Zip/Postal Code component of the Location's Street/Mailing Address. If this Location does not have 
        /// Street/Mailing Address, this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string AddressPostalCode { get; set; }

        /// <summary>
        /// Gets or sets the GeoPoint (geolocation) for the location
        /// </summary>
        /// <value>
        /// A <see cref="System.Data.Entity.Spatial.DbGeography"/> object that represents the geolocation of the Location.
        /// </value>
        [DataMember]
        [Newtonsoft.Json.JsonConverter( typeof( DbGeographyConverter ) )]
        public DbGeography AddressGeoPoint { get; set; }

        /// <summary>
        /// Gets or sets the geographic parameter around the a Location's Geopoint. This can also be used to define a large area
        /// like a neighborhood.  
        /// </summary>
        /// <remarks>
        /// Examples of this could be  a radius around a church campus to allow mobile check in if a person is located within a certain radius of 
        /// the campus, or it could be used to define the parameter of an area (i.e. neighborhood, park, etc.)
        /// </remarks>
        /// <value>
        /// A <see cref="System.Data.Entity.Spatial.DbGeography"/> object representing the parameter of a location.
        /// </value>
        [DataMember]
        [Newtonsoft.Json.JsonConverter( typeof( DbGeographyConverter ) )]
        public DbGeography AddressGeoFence { get; set; }

        /// <summary> 
        /// Gets or sets the latitude. (From AddressGeoPoint)
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        [DataMember]
        public double? AddressLatitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude. (From AddressGeoPoint)
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        [DataMember]
        public double? AddressLongitude { get; set; }

        /// <summary>
        /// Gets or sets the full  address.
        /// </summary>
        /// <value>
        /// The  address full.
        /// </value>
        [DataMember]
        public string AddressFull { get; set; }

        #endregion Address Fields

        #endregion Entity Properties
    }

    #region Entity Configuration

    /// <summary>
    /// AnalyticsDimCampus Configuration Class
    /// </summary>
    public partial class AnalyticsDimCampusConfiguration : EntityTypeConfiguration<AnalyticsDimCampus>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsDimCampusConfiguration"/> class.
        /// </summary>
        public AnalyticsDimCampusConfiguration()
        {
            // Empty constructor. This is required to tell EF that this model exists.
        }
    }

    #endregion Entity Configuration
}
