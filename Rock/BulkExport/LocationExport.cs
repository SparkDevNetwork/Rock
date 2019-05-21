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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.BulkExport
{
    /// <summary>
    /// 
    /// </summary>
    [RockClientInclude( "Export class for Addresses from ~/api/People/Export" )]
    public class LocationExport
    {
        private Location _location;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationExport" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        public LocationExport( Location location )
        {
            _location = location;
        }

        /// <summary>
        /// Gets the latitude.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        [DataMember]
        public double? Latitude => _location?.Latitude;

        /// <summary>
        /// Gets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        [DataMember]
        public double? Longitude => _location?.Longitude;

        /// <summary>
        /// Gets the street1.
        /// </summary>
        /// <value>
        /// The street1.
        /// </value>
        [DataMember]
        public string Street1 => _location?.Street1;

        /// <summary>
        /// Gets the street2.
        /// </summary>
        /// <value>
        /// The street2.
        /// </value>
        [DataMember]
        public string Street2 => _location?.Street2;

        /// <summary>
        /// Gets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        [DataMember]
        public string City => _location?.City;

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        [DataMember]
        public string State => _location?.State;

        /// <summary>
        /// Gets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        [DataMember]
        public string PostalCode => _location?.PostalCode;

        /// <summary>
        /// Gets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        [DataMember]
        public string Country => _location?.Country;

        /// <summary>
        /// Gets the county.
        /// </summary>
        /// <value>
        /// The county.
        /// </value>
        [DataMember]
        public string County => _location?.County;
    }
}
