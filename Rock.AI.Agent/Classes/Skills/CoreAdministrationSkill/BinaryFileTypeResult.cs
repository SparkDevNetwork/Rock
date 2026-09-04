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

using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;

/// <summary>
/// A single binary file type as it appears in a lookup.
/// </summary>
/// <remarks>
/// The set is bounded by configuration and small, so the whole of it is returned
/// without paging. There is no detail partner, so the description rides along here
/// and the Guid is carried directly, since a binary file type reference is stored
/// as a Guid in several places.
/// </remarks>
internal class BinaryFileTypeResult : EntityResultBase
{
    /// <summary>
    /// The name of the binary file type.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description of the binary file type.
    /// </summary>
    public string Description { get; set; }
}
