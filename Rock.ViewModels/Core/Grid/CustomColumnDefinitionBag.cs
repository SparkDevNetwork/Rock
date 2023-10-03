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

using Rock.Enums.Core.Grid;

namespace Rock.ViewModels.Core.Grid
{
    /// <summary>
    /// Defines the structure of a custom column that will be incorporated
    /// into a Grid.
    /// </summary>
    public class CustomColumnDefinitionBag
    {
        /// <summary>
        /// Gets or sets the text to display in the column header.
        /// </summary>
        /// <value>The header text.</value>
        public string HeaderText { get; set; }

        /// <summary>
        /// Gets or sets the header class. This will be applied to the th element.
        /// </summary>
        /// <value>The header class.</value>
        public string HeaderClass { get; set; }

        /// <summary>
        /// Gets or sets the item class. This will be applied to all td elements.
        /// </summary>
        /// <value>The item class.</value>
        public string ItemClass { get; set; }

        /// <summary>
        /// Gets or sets the name of the field that contains the text data.
        /// </summary>
        /// <value>The name of the field that contains the text data.</value>
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating where this column is anchored.
        /// </summary>
        /// <value>A value indicating where this column is anchored.</value>
        public ColumnPositionAnchor Anchor { get; set; }

        /// <summary>
        /// Gets or sets the position offset. This should be a positive number.
        /// </summary>
        /// <value>The position offset.</value>
        public int PositionOffset { get; set; }
    }
}
