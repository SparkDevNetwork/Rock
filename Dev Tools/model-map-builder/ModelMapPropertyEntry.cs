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

namespace Rock.ModelMapBuilder
{
    /// <summary>
    /// A property of a model, with its documentation, physical database schema,
    /// enum values, and defined type information.
    /// </summary>
    internal class ModelMapPropertyEntry
    {
        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the property's XML documentation comment.
        /// </summary>
        public ModelMapComment Comment { get; set; }

        /// <summary>
        /// Gets or sets the SQL data type of the mapped column (e.g. "nvarchar"),
        /// or <see langword="null"/> when the property is not mapped to a column.
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets the column length (character/binary length, or numeric
        /// precision for decimal types).
        /// </summary>
        public int? Length { get; set; }

        /// <summary>
        /// Gets or sets the numeric scale for decimal columns.
        /// </summary>
        public int? Scale { get; set; }

        /// <summary>
        /// Gets or sets whether the mapped column allows nulls.
        /// </summary>
        public bool? IsNullable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the mapped column is part of the primary key.
        /// </summary>
        public bool IsPrimaryKey { get; set; }

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
        /// Gets or sets the enum values keyed by their numeric value, when
        /// <see cref="IsEnum"/> is <see langword="true"/>.
        /// </summary>
        public Dictionary<string, string> EnumValues { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this property references a defined value.
        /// </summary>
        public bool IsDefinedValue { get; set; }

        /// <summary>
        /// Gets or sets the defined type information (name, guid, and system
        /// values) when <see cref="IsDefinedValue"/> is <see langword="true"/>.
        /// </summary>
        public ModelMapDefinedTypeInfo DefinedType { get; set; }

        /// <summary>
        /// Determines whether <see cref="Comment"/> should be serialized.
        /// </summary>
        public bool ShouldSerializeComment()
        {
            return Comment != null && !Comment.IsEmpty;
        }

        /// <summary>
        /// Determines whether <see cref="DataType"/> should be serialized.
        /// </summary>
        public bool ShouldSerializeDataType()
        {
            return DataType != null;
        }

        /// <summary>
        /// Determines whether <see cref="Length"/> should be serialized.
        /// </summary>
        public bool ShouldSerializeLength()
        {
            return Length.HasValue;
        }

        /// <summary>
        /// Determines whether <see cref="Scale"/> should be serialized.
        /// </summary>
        public bool ShouldSerializeScale()
        {
            return Scale.HasValue;
        }

        /// <summary>
        /// Determines whether <see cref="IsNullable"/> should be serialized.
        /// </summary>
        public bool ShouldSerializeIsNullable()
        {
            return IsNullable.HasValue;
        }

        /// <summary>
        /// Determines whether <see cref="EnumValues"/> should be serialized
        /// (omitted when the property is not an enum).
        /// </summary>
        public bool ShouldSerializeEnumValues()
        {
            return EnumValues != null;
        }

        /// <summary>
        /// Determines whether <see cref="DefinedType"/> should be serialized
        /// (omitted when the property is not a defined value).
        /// </summary>
        public bool ShouldSerializeDefinedType()
        {
            return DefinedType != null;
        }
    }
}
