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

using System.Collections.Generic;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;

/// <summary>
/// A single global attribute in full detail, including both its definition and
/// its current value. A global attribute is an attribute with no owning entity,
/// so its value is a single organization-wide setting rather than one per record.
/// </summary>
internal class GlobalAttributeDetailResult : EntityResultBase
{
    /// <summary>
    /// The programmatic key of the global attribute, such as <c>OrganizationName</c>.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The friendly name of the global attribute.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description of the global attribute.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The field type that governs how the value is edited and stored.
    /// </summary>
    public KeyNameResult FieldType { get; set; }

    /// <summary>
    /// Indicates that the global attribute requires a value.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Indicates that the global attribute is part of Rock's core configuration
    /// and cannot be deleted.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// The default value applied when no explicit value is set.
    /// </summary>
    public string DefaultValue { get; set; }

    /// <summary>
    /// The current value of the global attribute.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// The categories the global attribute is filed under.
    /// </summary>
    public List<KeyNameResult> Categories { get; set; }
}
