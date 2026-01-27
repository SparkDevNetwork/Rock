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
    /// The options for the Defined Type Picker's "get defined types" endpoint.
    /// </summary>
    public class DefinedTypePickerGetDefinedTypesOptionsBag
    {
        /// <summary>
        /// List of Defined Type GUIDs to include in the picker. If null or empty, include all defined types.
        /// </summary>
        public List<Guid> DefinedTypes { get; set; }

        /// <summary>
        /// List of Defined Type GUIDs to exclude from the picker.
        /// </summary>
        public List<Guid> ExcludeDefinedTypes { get; set; }

        /// <summary>
        /// Whether to sort by name of the defined type. Otherwise sort by order then name.
        /// </summary>
        public bool IsSortedByName { get; set; }
    }
}


