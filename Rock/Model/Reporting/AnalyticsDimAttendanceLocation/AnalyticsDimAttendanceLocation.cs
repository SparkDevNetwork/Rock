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
    /// AnalyticsDimAttendanceLocation is a SQL View off of the Location table
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsDimAttendanceLocation" )]
    [DataContract]
    [HideFromReporting]
    [CodeGenExclude( CodeGenFeature.ViewModelFile )]
    public class AnalyticsDimAttendanceLocation
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        [Key]
        public int LocationId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength( 250 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the first line of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the First line of the Location's Street/Mailing Address. If the Location does not have
        /// a Street/Mailing address, this value is null.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Street1 { get; set; }

        /// <summary>
        /// Gets or sets the second line of the Location's Street/Mailing Address. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the second line of the Location's Street/Mailing Address. if this Location does not have 
        /// Street/Mailing Address or if the address does not have a 2nd line, this value is null.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Street2 { get; set; }

        /// <summary>
        /// Gets or sets the city component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the city component of the Location's Street/Mailing Address. If this Location does not have
        /// a Street/Mailing Address this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the county.
        /// </summary>
        /// <value>
        /// The county.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string County { get; set; }

        /// <summary>
        /// Gets or sets the State component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the state component of the Location's Street/Mailing Address. If this Location does not have 
        /// a Street/Mailing Address, this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the country component of the Location's Street/Mailing Address. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the country component of the Location's Street/Mailing Address. If this Location does not have a 
        /// Street/Mailing Address, this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the Zip/Postal Code component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Zip/Postal Code component of the Location's Street/Mailing Address. If this Location does not have 
        /// Street/Mailing Address, this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the GeoPoint (geolocation) for the location
        /// </summary>
        /// <value>
        /// A <see cref="System.Data.Entity.Spatial.DbGeography"/> object that represents the geolocation of the Location.
        /// </value>
        [DataMember]
        [Newtonsoft.Json.JsonConverter( typeof( DbGeographyConverter ) )]
        public DbGeography GeoPoint { get; set; }

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
        public DbGeography GeoFence { get; set; }

        /// <summary>
        /// Gets or sets the latitude (From GeoPoint)
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        [DataMember]
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude (From GeoPoint)
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        [DataMember]
        public double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets the full address.
        /// </summary>
        /// <value>
        /// The  address full.
        /// </value>
        [DataMember]
        public string FullAddress { get; set; }

        #endregion Entity Properties

        #region Entity Properties Specific to Analytics

        /// <summary>
        /// Gets or sets the count.
        /// NOTE:  This always has a (hard-coded) value of 1. It is stored in the table to assist with analytics calculations.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        [DataMember]
        public int Count { get; set; }

        #endregion Entity Properties Specific to Analytics
    }

    #region Entity Configuration

    /// <summary>
    /// AnalyticsDimAttendanceLocation Configuration Class
    /// </summary>
    public partial class AnalyticsDimAttendanceLocationConfiguration : EntityTypeConfiguration<AnalyticsDimAttendanceLocation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsDimAttendanceLocationConfiguration"/> class.
        /// </summary>
        public AnalyticsDimAttendanceLocationConfiguration()
        {
            // Empty constructor. This is required to tell EF that this model exists.
        }
    }

    #endregion Entity Configuration
}
