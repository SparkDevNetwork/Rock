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
//

using System.Collections.Generic;

using Rock.ViewModel.NonEntities;

namespace Rock.ViewModel.Controls
{
    /// <summary>
    /// Describes a field type configuration state. This provides the information
    /// required to edit a field type on a remote system.
    /// </summary>
    public class FieldTypeConfigurationPropertiesViewModel
    {
        /// <summary>
        /// Gets or sets the configuration properties that contain information
        /// describing a field type edit operation.
        /// </summary>
        /// <remarks>
        /// See: Rock.Field.IFieldType.GetClientEditConfigurationProperties()
        /// </remarks>
        /// <value>The configuration properties for a field edit operation.</value>
        public Dictionary<string, string> ConfigurationProperties { get; set; }

        /// <summary>
        /// Gets or sets the configuration options that describe the current
        /// selections when editing a field type.
        /// </summary>
        /// <remarks>
        /// See: Rock.Field.IFieldType.GetPublicConfigurationOptions()
        /// </remarks>
        /// <value>The configuration options.</value>
        public Dictionary<string, string> ConfigurationOptions { get; set; }

        /// <summary>
        /// Gets or sets the default attribute value view model that corresponds
        /// to the current <see cref="ConfigurationOptions"/>.
        /// </summary>
        /// <value>The default value information.</value>
        public PublicEditableAttributeValueViewModel DefaultValue { get; set; }
    }
}
