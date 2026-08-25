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

namespace Rock.AI.Agent.Classes.Skills.EventRegistrationSkill;

/// <summary>
/// Represents the result of a registration instance linkage, including the
/// registration instance, group, and event item occurrence.
/// </summary>
internal class RegistrationInstanceLinkageResult
{
    /// <summary>
    /// The registration instance associated with the linkage.
    /// </summary>
    public KeyNameResult RegistrationInstance { get; set; }

    /// <summary>
    /// The group associated with the linkage.
    /// </summary>
    public KeyNameResult Group { get; set; }

    /// <summary>
    /// The event item occurrence associated with the linkage.
    /// </summary>
    public KeyNameResult EventItemOccurrence { get; set; }
}
