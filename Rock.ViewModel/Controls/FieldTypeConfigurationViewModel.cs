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

using System;
using System.Collections.Generic;

namespace Rock.ViewModel.Controls
{
    /// <summary>
    /// Contains information required to update a field type's configuration.
    /// </summary>
    public class FieldTypeConfigurationViewModel
    {
        /// <summary>
        /// Gets or sets the field type unique identifier.
        /// </summary>
        /// <value>The field type unique identifier.</value>
        public Guid FieldTypeGuid { get; set; }

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
        /// Gets or sets the default value currently set.
        /// </summary>
        /// <value>The default value currently set.</value>
        public string DefaultValue { get; set; }
    }

}
