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
/// A single defined value in full detail, including its Guid. Rock stores a
/// defined value reference as a Guid in several places, so this is where a
/// caller goes to get one.
/// </summary>
internal class DefinedValueDetailResult : EntityResultBase
{
    /// <summary>
    /// The value text, which is what an administrator sees in a picker.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// The description of the value.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The order of the value relative to its siblings.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Indicates that the value is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The defined type this value belongs to.
    /// </summary>
    public KeyNameResult DefinedType { get; set; }

    /// <summary>
    /// The category the value is filed under, or <c>null</c> when the parent
    /// type does not use categorized values.
    /// </summary>
    public KeyNameResult Category { get; set; }
}
