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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// AnalyticsDimFamilyHistorical is SQL View based on AnalyticsSourceFamilyHistorical
    /// and represents the historic and current records from AnalyticsSourceFamilyHistorical
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsDimFamilyHistorical" )]
    [DataContract]
    public class AnalyticsDimFamilyHistorical : AnalyticsDimFamilyBase<AnalyticsDimFamilyHistorical>
    {
        // intentionally blank. See AnalyticsDimFamilyBase, etc for the fields
    }

    /// <summary>
    /// *Another* Abstract Layer since AnalyticDimFamilyHistorical and AnalyticsDimFamilyCurrent share all the same fields
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Rock.Model.AnalyticsSourceFamilyBase{T}" />
    [RockDomain( "Reporting" )]
    public abstract class AnalyticsDimFamilyBase<T> : AnalyticsSourceFamilyBase<T>, IAnalyticsAddresses
        where T : AnalyticsDimFamilyBase<T>, new()
    {
        #region Denormalized Lookup Values

        /// <summary>
        /// Gets or sets the name of the campus.
        /// </summary>
        /// <value>
        /// The name of the campus.
        /// </value>
        [DataMember]
        public string CampusName { get; set; }

        /// <summary>
        /// Gets or sets the campus short code.
        /// </summary>
        /// <value>
        /// The campus short code.
        /// </value>
        [DataMember]
        public string CampusShortCode { get; set; }

        #endregion

        #region Primary Mailing Address

        /// <summary>
        /// Gets or sets the first line of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the First line of the Location's Street/Mailing Address. If the Location does not have
        /// a Street/Mailing address, this value is null.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string MailingAddressStreet1 { get; set; }

        /// <summary>
        /// Gets or sets the second line of the Location's Street/Mailing Address. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the second line of the Location's Street/Mailing Address. if this Location does not have 
        /// Street/Mailing Address or if the address does not have a 2nd line, this value is null.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string MailingAddressStreet2 { get; set; }

        /// <summary>
        /// Gets or sets the city component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the city component of the Location's Street/Mailing Address. If this Location does not have
        /// a Street/Mailing Address this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string MailingAddressCity { get; set; }

        /// <summary>
        /// Gets or sets the county.
        /// </summary>
        /// <value>
        /// The county.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string MailingAddressCounty { get; set; }

        /// <summary>
        /// Gets or sets the State component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the state component of the Location's Street/Mailing Address. If this Location does not have 
        /// a Street/Mailing Address, this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string MailingAddressState { get; set; }

        /// <summary>
        /// Gets or sets the country component of the Location's Street/Mailing Address. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the country component of the Location's Street/Mailing Address. If this Location does not have a 
        /// Street/Mailing Address, this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string MailingAddressCountry { get; set; }

        /// <summary>
        /// Gets or sets the Zip/Postal Code component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Zip/Postal Code component of the Location's Street/Mailing Address. If this Location does not have 
        /// Street/Mailing Address, this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string MailingAddressPostalCode { get; set; }

        /// <summary>
        /// Gets or sets the GeoPoint (geolocation) for the location
        /// </summary>
        /// <value>
        /// A <see cref="System.Data.Entity.Spatial.DbGeography"/> object that represents the geolocation of the Location.
        /// </value>
        [DataMember]
        [Newtonsoft.Json.JsonConverter( typeof( DbGeographyConverter ) )]
        public DbGeography MailingAddressGeoPoint { get; set; }

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
        public DbGeography MailingAddressGeoFence { get; set; }

        /// <summary> 
        /// Gets or sets the latitude. (From MailingAddressGeoPoint)
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        [DataMember]
        public double? MailingAddressLatitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude. (From MailingAddressGeoPoint)
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        [DataMember]
        public double? MailingAddressLongitude { get; set; }

        /// <summary>
        /// Gets or sets the full mailing address.
        /// </summary>
        /// <value>
        /// The mailing address full.
        /// </value>
        [DataMember]
        public string MailingAddressFull { get; set; }

        #endregion

        #region Primary Mailing Address

        /// <summary>
        /// Gets or sets the first line of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the First line of the Location's Street/Mailing Address. If the Location does not have
        /// a Street/Mailing address, this value is null.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string MappedAddressStreet1 { get; set; }

        /// <summary>
        /// Gets or sets the second line of the Location's Street/Mailing Address. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the second line of the Location's Street/Mailing Address. if this Location does not have 
        /// Street/Mailing Address or if the address does not have a 2nd line, this value is null.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string MappedAddressStreet2 { get; set; }

        /// <summary>
        /// Gets or sets the city component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the city component of the Location's Street/Mailing Address. If this Location does not have
        /// a Street/Mailing Address this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string MappedAddressCity { get; set; }

        /// <summary>
        /// Gets or sets the county.
        /// </summary>
        /// <value>
        /// The county.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string MappedAddressCounty { get; set; }

        /// <summary>
        /// Gets or sets the State component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the state component of the Location's Street/Mailing Address. If this Location does not have 
        /// a Street/Mailing Address, this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string MappedAddressState { get; set; }

        /// <summary>
        /// Gets or sets the country component of the Location's Street/Mailing Address. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the country component of the Location's Street/Mailing Address. If this Location does not have a 
        /// Street/Mailing Address, this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string MappedAddressCountry { get; set; }

        /// <summary>
        /// Gets or sets the Zip/Postal Code component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Zip/Postal Code component of the Location's Street/Mailing Address. If this Location does not have 
        /// Street/Mailing Address, this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string MappedAddressPostalCode { get; set; }

        /// <summary>
        /// Gets or sets the GeoPoint (geolocation) for the location
        /// </summary>
        /// <value>
        /// A <see cref="System.Data.Entity.Spatial.DbGeography"/> object that represents the geolocation of the Location.
        /// </value>
        [DataMember]
        [Newtonsoft.Json.JsonConverter( typeof( DbGeographyConverter ) )]
        public DbGeography MappedAddressGeoPoint { get; set; }

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
        public DbGeography MappedAddressGeoFence { get; set; }

        /// <summary>
        /// Gets or sets the latitude. (From MappedAddressGeoPoint)
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        [DataMember]
        public double? MappedAddressLatitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude. (From MappedAddressGeoPoint)
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        [DataMember]
        public double? MappedAddressLongitude { get; set; }

        /// <summary>
        /// Gets or sets the full mapped address.
        /// </summary>
        /// <value>
        /// The mapped address full.
        /// </value>
        [DataMember]
        public string MappedAddressFull { get; set; }

        #endregion
    }
}
