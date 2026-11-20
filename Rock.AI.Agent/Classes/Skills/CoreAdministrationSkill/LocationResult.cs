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
/// A single location as it appears in a list. A location is either an address
/// or a named place such as a building or room.
/// </summary>
internal class LocationResult : EntityResultBase
{
    /// <summary>
    /// The name of the location, or <c>null</c> for a plain address.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The full address formatted for the location's country, or an empty
    /// string when the location has no usable address. This stays in the list
    /// result because an address location has no name, so without it the rows
    /// could not be told apart.
    /// </summary>
    public string FormattedAddress { get; set; }

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
    /// Indicates that the location is active.
    /// </summary>
    public bool IsActive { get; set; }
}
