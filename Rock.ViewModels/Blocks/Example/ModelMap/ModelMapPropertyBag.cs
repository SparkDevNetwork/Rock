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

namespace Rock.ViewModels.Blocks.Example.ModelMap
{
    /// <summary>
    /// Represents a property of a model class.
    /// </summary>
    public class ModelMapPropertyBag
    {
        /// <summary>
        /// Gets or sets the metadata token identifier of the property.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the XML documentation comments for the property.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this property is inherited from a base class.
        /// </summary>
        public bool IsInherited { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this property is virtual.
        /// </summary>
        public bool IsVirtual { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this property is available in Lava.
        /// </summary>
        public bool IsLavaInclude { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this property can be used as an attribute qualifier.
        /// </summary>
        public bool IsAttributeQualifier { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this property is obsolete.
        /// </summary>
        public bool IsObsolete { get; set; }

        /// <summary>
        /// Gets or sets the message explaining why the property is obsolete, if applicable.
        /// </summary>
        public string ObsoleteMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this property is not mapped to the database.
        /// </summary>
        public bool NotMapped { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this property is required.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this property is an enumeration.
        /// </summary>
        public bool IsEnum { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this property is a defined value.
        /// </summary>
        public bool IsDefinedValue { get; set; }

        /// <summary>
        /// Gets or sets the key-value pairs for enum or defined value descriptions.
        /// </summary>
        public Dictionary<string, string> KeyValues { get; set; }

        /// <summary>
        /// Gets or sets a standard description for the enum or defined type.
        /// </summary>
        public string EnumOrDefinedTypeDescription { get; set; }
    }
}
