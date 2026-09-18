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
/// The configuration of a single defined type. This carries neither the type's
/// values nor the attribute definitions that apply to those values; both have
/// their own tools.
/// </summary>
internal class DefinedTypeDetailResult : EntityResultBase
{
    /// <summary>
    /// The name of the defined type.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description of the defined type.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Help text shown to an administrator editing this type's values.
    /// </summary>
    public string HelpText { get; set; }

    /// <summary>
    /// The category the defined type is filed under, or <c>null</c> when it is
    /// uncategorized.
    /// </summary>
    public KeyNameResult Category { get; set; }

    /// <summary>
    /// Indicates that the defined type is part of Rock's core configuration and
    /// cannot be deleted.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Indicates that the defined type is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Indicates that the values of this type can be filed under categories,
    /// which is what makes a category filter meaningful when listing them.
    /// </summary>
    public bool CategorizedValuesEnabled { get; set; }

    /// <summary>
    /// Indicates that each value carries its own security. When true, a list of
    /// this type's values has been filtered to what the current person may see,
    /// so a caller needs to know it to interpret a short list correctly.
    /// </summary>
    public bool EnableSecurityOnValues { get; set; }

    /// <summary>
    /// How many values the type has, before any security filtering.
    /// </summary>
    public int ValueCount { get; set; }
}
