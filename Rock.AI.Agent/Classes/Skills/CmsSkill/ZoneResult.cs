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

namespace Rock.AI.Agent.Classes.Skills.CmsSkill;

/// <summary>
/// Result model for a single zone of a page layout, nested inside
/// <see cref="LayoutResult"/>.
/// </summary>
internal class ZoneResult
{
    /// <summary>
    /// The name of the zone with spaces removed, which is the form a block
    /// stores its zone in and the value AddOrUpdateBlock expects.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The zone name as the theme declares it, present only when it differs
    /// from <see cref="Name"/> by spacing, such as "Badge Bar" for "BadgeBar".
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// How many blocks already render in this zone on the page being looked
    /// at, counting the page, layout and site scopes together.
    /// </summary>
    public int BlockCount { get; set; }
}
