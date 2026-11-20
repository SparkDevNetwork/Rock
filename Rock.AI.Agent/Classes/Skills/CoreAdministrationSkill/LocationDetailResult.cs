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

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;

/// <summary>
/// A single location in full detail, including its Guid. Address, Location,
/// and Location List attributes store a location by its Guid, so this is
/// where a caller goes to get one.
/// </summary>
internal class LocationDetailResult : EntityResultBase
{
    /// <summary>
    /// The name of the location, or <c>null</c> for a plain address.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The first line of the street address.
    /// </summary>
    public string Street1 { get; set; }

    /// <summary>
    /// The second line of the street address.
    /// </summary>
    public string Street2 { get; set; }

    /// <summary>
    /// The city.
    /// </summary>
    public string City { get; set; }

    /// <summary>
    /// The state or province.
    /// </summary>
    public string State { get; set; }

    /// <summary>
    /// The county or locality.
    /// </summary>
    public string County { get; set; }

    /// <summary>
    /// The postal code.
    /// </summary>
    public string PostalCode { get; set; }

    /// <summary>
    /// The two-letter country code.
    /// </summary>
    public string Country { get; set; }

    /// <summary>
    /// The full address formatted for the location's country, or an empty
    /// string when the location has no usable address.
    /// </summary>
    public string FormattedAddress { get; set; }

    /// <summary>
    /// The latitude of the location, or <c>null</c> when it has not been
    /// geocoded.
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// The longitude of the location, or <c>null</c> when it has not been
    /// geocoded.
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Indicates that the location is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The location type, such as Building or Room, or <c>null</c> when none
    /// is set.
    /// </summary>
    public KeyNameResult LocationType { get; set; }

    /// <summary>
    /// The parent location, or <c>null</c> when this is a root location.
    /// </summary>
    public KeyNameResult ParentLocation { get; set; }

    /// <summary>
    /// How many locations sit directly beneath this one. A location with
    /// children cannot be deleted, so this tells a caller up front.
    /// </summary>
    public int ChildLocationCount { get; set; }
}
