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
/// A single field type as it appears in a lookup.
/// </summary>
/// <remarks>
/// Two fields per row is what keeps the whole set small enough to return without
/// paging and to hold in conversation history, which is the point of the lookup.
/// The class name and description come from the detail tool.
/// </remarks>
internal class FieldTypeResult : EntityResultBase
{
    /// <summary>
    /// The name of the field type.
    /// </summary>
    public string Name { get; set; }
}
