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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetEntityTypeGuids API action of the EntityPicker control.
    /// </summary>
    public class EntityPickerGetFieldTypeConfigurationResultsBag
    {
        /// <summary>
        /// The GUID of the field type that is associated with the given entity
        /// </summary>
        public Guid FieldTypeGuid { get; set; }

        /// <summary>
        /// The anem of the field type that is associated with the given entity
        /// </summary>
        public string FieldTypeName { get; set; }

        /// <summary>
        /// The name of the field type that is associated with the given entity, pluralized
        /// </summary>
        public string FieldTypePluralName { get; set; }

        /// <summary>
        /// Configuration Values to use for the field's Edit component
        /// </summary>
        public Dictionary<string, string> ConfigurationValues { get; set; }
    }
}
