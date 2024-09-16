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

using System.Collections.Generic;

namespace Rock.ViewModels.Core.Grid
{
    /// <summary>
    /// Information about a single dynamic field that has been defined on
    /// the grid. Dynamic fields are handled slightly differently than
    /// normal fields so these are handled differently than regular
    /// <see cref="FieldDefinitionBag"/> objects.
    /// </summary>
    public class DynamicFieldDefinitionBag
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
        /// Gets or sets the column type.
        /// </summary>
        /// <value>
        /// The column type.
        /// </value>
        public string ColumnType { get; set; }

        /// <summary>
        /// Gets or sets whether to hide this column from rendering on screen.
        /// It may still be include in exports and other operations.
        /// </summary>
        /// <value>
        /// Whether to hide this column from rendering on screen.
        /// </value>
        public bool HideOnScreen { get; set; }

        /// <summary>
        /// Gets or sets the minimum window size for the column to be displayed.
        /// </summary>
        /// <value>
        /// The minimum window size for the column to be displayed.
        /// </value>
        public string VisiblePriority { get; set; }

        /// <summary>
        /// Gets or sets the width of the column.
        /// </summary>
        /// <value>
        /// The width of the column.
        /// </value>
        public string Width { get; set; }

        /// <summary>
        /// Gets or sets whether to enable filtering of this column.
        /// </summary>
        /// <value>
        /// Whether to enable filtering of this column.
        /// </value>
        public bool EnableFiltering { get; set; }

        /// <summary>
        /// Gets or sets whether to exclude this field when exporting data
        /// to be downloaded by the individual.
        /// </summary>
        /// <value>
        /// Whether to exclude this field when exporting data.
        /// </value>
        public bool ExcludeFromExport { get; set; }

        /// <summary>
        /// Gets or sets property values for specific field types.
        /// </summary>
        /// <value>
        /// Property values for specific field types.
        /// </value>
        public Dictionary<string, object> FieldProperties { get; set; }
    }
}
