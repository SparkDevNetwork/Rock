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

namespace Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;

/// <summary>
/// One configuration qualifier accepted by a field type.
/// </summary>
/// <remarks>
/// <see cref="Rock.Field.IFieldType.ConfigurationKeys"/> returns bare key names
/// with no indication of what a key does or what value format it takes, and many
/// field types never populated it at all. The description and example are
/// therefore supplemented by hand for the field types that authoring actually
/// touches. Where no supplement exists the description says the key is
/// undocumented rather than guessing.
/// </remarks>
internal class FieldTypeConfigurationKeyResult
{
    /// <summary>
    /// The qualifier key, used verbatim when creating an attribute.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// What the qualifier controls, or a statement that it is undocumented.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// A representative value showing the expected format, or <c>null</c> when
    /// the key is undocumented.
    /// </summary>
    public string ExampleValue { get; set; }
}
