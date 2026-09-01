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

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Entity;
using Rock.Enums.AI.Agent;

namespace Rock.AI.Agent.Classes.Skills.CommunicationSkill;

/// <summary>
/// A single communication list in full detail.
/// </summary>
internal class CommunicationListDetailResult : EntityResultBase
{
    /// <summary>
    /// The administrative name of the list. Only returned to an internal audience;
    /// a public audience sees <see cref="PublicName"/> instead.
    /// </summary>
    [JsonIgnoreAudienceType( AudienceType.Public )]
    public string Name { get; set; }

    /// <summary>
    /// The public facing name shown to subscribers. Falls back to the
    /// administrative name when no public name is configured.
    /// </summary>
    public string PublicName { get; set; }

    /// <summary>
    /// The description of the list.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Indicates that the list is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The number of active members on the list. This is the raw membership count
    /// and does not account for whether each member can actually be reached. Only
    /// returned to an internal audience.
    /// </summary>
    [JsonIgnoreAudienceType( AudienceType.Public )]
    public int? ActiveMemberCount { get; set; }
}
