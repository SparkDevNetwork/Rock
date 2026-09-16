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

namespace Rock.ModelMapBuilder
{
    /// <summary>
    /// The defined type a defined-value property points at, along with its
    /// system-defined values.
    /// </summary>
    /// <remarks>
    /// Only system defined values are included; non-system values vary by
    /// installation and are therefore not part of a source-controlled model map.
    /// </remarks>
    internal class ModelMapDefinedTypeInfo
    {
        /// <summary>
        /// Gets or sets the defined type's unique identifier. This always comes
        /// from the property's <c>DefinedValueAttribute</c> and is available even
        /// when the defined type could not be resolved from the database.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the defined type's name, resolved from the database.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the system-defined values belonging to this defined type.
        /// </summary>
        public List<ModelMapDefinedValueInfo> Values { get; set; }
    }
}
