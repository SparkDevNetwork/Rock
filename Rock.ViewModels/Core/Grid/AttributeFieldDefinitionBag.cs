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

using System;

namespace Rock.ViewModels.Core.Grid
{
    /// <summary>
    /// Information about a single attribute field that has been defined on
    /// the grid. Attributes are handled slightly differently than normal
    /// fields so these are handled differently than regular
    /// <see cref="FieldDefinitionBag"/> objects.
    /// </summary>
    public class AttributeFieldDefinitionBag
    {
        /// <summary>
        /// Gets or sets the name of the field. This corresponds to the key name
        /// in the row data object.
        /// </summary>
        /// <value>The name of the field.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the title to use for the column.
        /// </summary>
        /// <value>The title to use for the column.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the field type unique identifier. This is the field type
        /// that provides logic for the attribute.
        /// </summary>
        /// <value>The field type unique identifier.</value>
        public Guid FieldTypeGuid { get; set; }
    }
}
