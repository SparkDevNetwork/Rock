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
    /// The options that can be passed to the SaveNewValue API action of
    /// the DefinedValuePicker control for adding a new Defined Value
    /// </summary>
    public class DefinedValuePickerSaveNewValueOptionsBag
    {
        /// <summary>
        /// The GUID of the defined type of the value we're saving
        /// </summary>
        public Guid DefinedTypeGuid { get; set; }

        /// <summary>
        /// The value property of the new defined value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The description property of the new defined value
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A collection of attribute values for the new defined value
        /// </summary>
        public Dictionary<string, string> AttributeValues{ get; set; }

        /// <summary>
        /// Gets or sets the security grant token to use when performing authorization checks.
        /// </summary>
        public string SecurityGrantToken { get; set; }

        /// <summary>
        /// Gets or sets the attribute identifier that should be updated. This
        /// must be null or represent a Defined Value field type attribute.
        /// </summary>
        public Guid? UpdateAttributeGuid { get; set; }
    }
}
