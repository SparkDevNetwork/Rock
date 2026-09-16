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
/// A single defined value as it appears in a list.
/// </summary>
/// <remarks>
/// This deliberately carries no Guid. Several workflow actions store a defined
/// value's Guid as a setting value, but a caller retrieves it from the detail
/// tool for the one value it settles on rather than paying for a Guid on every
/// row of every page it discards.
/// </remarks>
internal class DefinedValueResult : EntityResultBase
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
    /// The category the value is filed under. Populated only when the parent
    /// defined type has categorized values enabled, since a null category on
    /// every row of a type that does not use them is noise.
    /// </summary>
    public KeyNameResult Category { get; set; }
}
