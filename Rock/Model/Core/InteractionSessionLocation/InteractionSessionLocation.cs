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

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.Spatial;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents Location for <see cref="Rock.Model.InteractionSession">Interaction Session</see>
    /// </summary>
    [RockDomain( "Core" )]
    [NotAudited]
    [Table( "InteractionSessionLocation" )]
    [DataContract]
    public class InteractionSessionLocation : Model<InteractionSessionLocation>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the IP address of the request.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> of the IP address of the request.
        /// </value>
        [DataMember]
        [MaxLength( 45 )]
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the lookup datetime.
        /// </summary>
        /// <value>
        /// The lookup datetime.
        /// </value>
        [DataMember]
        public DateTime LookupDateTime { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> of the postal code.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> of the location.
        /// </value>
        [DataMember]
        [MaxLength( 250 )]
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the ISP.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> of the ISP.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string ISP { get; set; }

        /// <summary>
        /// Gets or sets the country code.
        /// </summary>
        /// <value>
        /// The country code.
        /// </value>
        [MaxLength( 2 )]
        [DataMember]
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the <see cref="Rock.Model.DefinedValue"/> that represents the country.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing DefinedValueId of the country.
        /// </value>
        [DataMember]
        public int? CountryValueId { get; set; }

        /// <summary>
        /// Gets or sets the region code.
        /// </summary>
        /// <value>
        /// The region code.
        /// </value>
        [MaxLength( 2 )]
        [DataMember]
        public string RegionCode { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the <see cref="Rock.Model.DefinedValue"/> that represents the region.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing DefinedValueId of the region.
        /// </value>
        [DataMember]
        public int? RegionValueId { get; set; }

        /// <summary>
        /// Gets or sets the GeoPoint (GeoLocation) for the session
        /// </summary>
        /// <value>
        /// A <see cref="System.Data.Entity.Spatial.DbGeography"/> object that represents the GeoLocation of the session.
        /// </value>
        [DataMember]
        [Newtonsoft.Json.JsonConverter( typeof( DbGeographyConverter ) )]
        public DbGeography GeoPoint { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the country value.
        /// </summary>
        /// <value>
        /// The country value.
        /// </value>
        [DataMember]
        public virtual DefinedValue CountryValue { get; set; }

        /// <summary>
        /// Gets or sets the region value.
        /// </summary>
        /// <value>
        /// The region value.
        /// </value>
        [DataMember]
        public virtual DefinedValue RegionValue { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Location Configuration class.
    /// </summary>
    public partial class InteractionSessionLocationConfiguration : EntityTypeConfiguration<InteractionSessionLocation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionSessionLocationConfiguration"/> class.
        /// </summary>
        public InteractionSessionLocationConfiguration()
        {
            this.HasOptional( l => l.CountryValue ).WithMany().HasForeignKey( l => l.CountryValueId ).WillCascadeOnDelete( false );
            this.HasOptional( l => l.RegionValue ).WithMany().HasForeignKey( l => l.RegionValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
