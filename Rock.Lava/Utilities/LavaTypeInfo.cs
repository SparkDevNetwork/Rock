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

namespace Rock.Lava
{
    /// <summary>
    /// Information about the Lava-related properties of a data type.
    /// </summary>
    public class LavaTypeInfo
    {
        /// <summary>
        /// Gets or sets a flag indicating if this type can be accessed in a Lava template.
        /// </summary>
        public bool IsLavaType { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this is a primitive type with a single value, rather than a complex type with multiple properties.
        /// </summary>
        public bool IsPrimitiveType { get; set; }

        /// <summary>
        /// Gets or sets the names of the properties of this data type that are visible to Lava.
        /// </summary>
        public List<string> VisiblePropertyNames { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this data type is decorated with the [LavaType] attribute.
        /// </summary>
        public bool HasLavaTypeAttribute { get; set; }
    }
}