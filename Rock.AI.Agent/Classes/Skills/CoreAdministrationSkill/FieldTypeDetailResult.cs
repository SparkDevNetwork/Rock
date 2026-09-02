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

using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;

/// <summary>
/// A single field type in full detail, including the configuration qualifiers it
/// accepts.
/// </summary>
internal class FieldTypeDetailResult : EntityResultBase
{
    /// <summary>
    /// The name of the field type.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The full class name of the field type. Output only, so that a caller can
    /// recognize a field type it reads elsewhere. No tool accepts a class name
    /// as a parameter.
    /// </summary>
    public string Class { get; set; }

    /// <summary>
    /// The description of the field type.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The configuration qualifiers this field type accepts when an attribute is
    /// created with it.
    /// </summary>
    public List<FieldTypeConfigurationKeyResult> ConfigurationKeys { get; set; }
}
