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

using Rock.Core.Geography.Classes;

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// POCO result for a location.
    /// </summary>
    internal class LocationResult : EntityResultBase
    {
        /// <summary>
        /// Gets or sets the type of the location (e.g., Home, Work, Campus).
        /// </summary>
        public string LocationType { get; set; }

        /// <summary>
        /// Gets or sets the name of the location.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the first line of the street address.
        /// </summary>
        public string Street1 { get; set; }

        /// <summary>
        /// Gets or sets the second line of the street address.
        /// </summary>
        public string Street2 { get; set; }

        /// <summary>
        /// Gets or sets the city where the location is situated.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state or province of the location.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the postal or ZIP code of the location.
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the country of the location.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the county of the location.
        /// </summary>
        public string County { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this location is the mailing address.
        /// </summary>
        public bool? IsMailingAddress { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether this location is a mapped location.
        /// </summary>
        public bool? IsMappedLocation { get; set; } = null;

        /// <summary>
        /// Gets or sets the geographical point (latitude and longitude) of the location.
        /// </summary>
        public GeographyPoint GeographyPoint { get; set; }
    }
}
