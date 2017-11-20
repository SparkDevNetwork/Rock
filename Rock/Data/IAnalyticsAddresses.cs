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
using System.Data.Entity.Spatial;

namespace Rock.Data
{
    /// <summary>
    /// Interface for Analytics tables that contain Mailing and Mapped Address fields
    /// </summary>
    public interface IAnalyticsAddresses
    {
        /// <summary>
        /// Gets or sets the mailing address city.
        /// </summary>
        /// <value>
        /// The mailing address city.
        /// </value>
        string MailingAddressCity { get; set; }

        /// <summary>
        /// Gets or sets the mailing address country.
        /// </summary>
        /// <value>
        /// The mailing address country.
        /// </value>
        string MailingAddressCountry { get; set; }

        /// <summary>
        /// Gets or sets the mailing address county.
        /// </summary>
        /// <value>
        /// The mailing address county.
        /// </value>
        string MailingAddressCounty { get; set; }

        /// <summary>
        /// Gets or sets the mailing address geo fence.
        /// </summary>
        /// <value>
        /// The mailing address geo fence.
        /// </value>
        DbGeography MailingAddressGeoFence { get; set; }

        /// <summary>
        /// Gets or sets the mailing address geo point.
        /// </summary>
        /// <value>
        /// The mailing address geo point.
        /// </value>
        DbGeography MailingAddressGeoPoint { get; set; }

        /// <summary>
        /// Gets or sets the mailing address latitude.
        /// </summary>
        /// <value>
        /// The mailing address latitude.
        /// </value>
        double? MailingAddressLatitude { get; set; }

        /// <summary>
        /// Gets or sets the mailing address longitude.
        /// </summary>
        /// <value>
        /// The mailing address longitude.
        /// </value>
        double? MailingAddressLongitude { get; set; }

        /// <summary>
        /// Gets or sets the mailing address postal code.
        /// </summary>
        /// <value>
        /// The mailing address postal code.
        /// </value>
        string MailingAddressPostalCode { get; set; }

        /// <summary>
        /// Gets or sets the state of the mailing address.
        /// </summary>
        /// <value>
        /// The state of the mailing address.
        /// </value>
        string MailingAddressState { get; set; }

        /// <summary>
        /// Gets or sets the mailing address street1.
        /// </summary>
        /// <value>
        /// The mailing address street1.
        /// </value>
        string MailingAddressStreet1 { get; set; }

        /// <summary>
        /// Gets or sets the mailing address street2.
        /// </summary>
        /// <value>
        /// The mailing address street2.
        /// </value>
        string MailingAddressStreet2 { get; set; }

        /// <summary>
        /// Gets or sets the full mailing address.
        /// </summary>
        /// <value>
        /// The mailing address full.
        /// </value>
        string MailingAddressFull { get; set; }

        /// <summary>
        /// Gets or sets the mapped address city.
        /// </summary>
        /// <value>
        /// The mapped address city.
        /// </value>
        string MappedAddressCity { get; set; }

        /// <summary>
        /// Gets or sets the mapped address country.
        /// </summary>
        /// <value>
        /// The mapped address country.
        /// </value>
        string MappedAddressCountry { get; set; }

        /// <summary>
        /// Gets or sets the mapped address county.
        /// </summary>
        /// <value>
        /// The mapped address county.
        /// </value>
        string MappedAddressCounty { get; set; }

        /// <summary>
        /// Gets or sets the mapped address geo fence.
        /// </summary>
        /// <value>
        /// The mapped address geo fence.
        /// </value>
        DbGeography MappedAddressGeoFence { get; set; }

        /// <summary>
        /// Gets or sets the mapped address geo point.
        /// </summary>
        /// <value>
        /// The mapped address geo point.
        /// </value>
        DbGeography MappedAddressGeoPoint { get; set; }

        /// <summary>
        /// Gets or sets the mapped address latitude.
        /// </summary>
        /// <value>
        /// The mapped address latitude.
        /// </value>
        double? MappedAddressLatitude { get; set; }

        /// <summary>
        /// Gets or sets the mapped address longitude.
        /// </summary>
        /// <value>
        /// The mapped address longitude.
        /// </value>
        double? MappedAddressLongitude { get; set; }

        /// <summary>
        /// Gets or sets the mapped address postal code.
        /// </summary>
        /// <value>
        /// The mapped address postal code.
        /// </value>
        string MappedAddressPostalCode { get; set; }

        /// <summary>
        /// Gets or sets the state of the mapped address.
        /// </summary>
        /// <value>
        /// The state of the mapped address.
        /// </value>
        string MappedAddressState { get; set; }

        /// <summary>
        /// Gets or sets the mapped address street1.
        /// </summary>
        /// <value>
        /// The mapped address street1.
        /// </value>
        string MappedAddressStreet1 { get; set; }

        /// <summary>
        /// Gets or sets the mapped address street2.
        /// </summary>
        /// <value>
        /// The mapped address street2.
        /// </value>
        string MappedAddressStreet2 { get; set; }

        /// <summary>
        /// Gets or sets the full mapped address.
        /// </summary>
        /// <value>
        /// The mapped address full.
        /// </value>
        string MappedAddressFull { get; set; }
    }
}