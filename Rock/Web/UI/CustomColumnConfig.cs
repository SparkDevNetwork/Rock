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

using Rock.Attribute;
using Rock.Enums.Core.Grid;

namespace Rock.Web.UI
{
    /// <summary>
    /// Contains the configuration for a custom column in a grid. These are
    /// defined by the administrator in Block Settings.
    /// </summary>
    [Serializable]
    public class CustomColumnConfig
    {
        /// <summary>
        /// Gets or sets the header text.
        /// </summary>
        /// <value>
        /// The header text.
        /// </value>
        public string HeaderText { get; set; }

        /// <summary>
        /// Gets or sets the header class.
        /// </summary>
        /// <value>
        /// The header class.
        /// </value>
        public string HeaderClass { get; set; }

        /// <summary>
        /// Gets or sets the item class.
        /// </summary>
        /// <value>
        /// The item class.
        /// </value>
        public string ItemClass { get; set; }

        /// <summary>
        /// Gets or sets the lava template.
        /// </summary>
        /// <value>
        /// The lava template.
        /// </value>
        public string LavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets the type of the position offset.
        /// </summary>
        /// <value>
        /// The type of the position offset.
        /// </value>
        public ColumnPositionAnchor PositionOffsetType { get; set; }

        /// <summary>
        /// Gets or sets the position offset relative to PositionOffsetType
        /// </summary>
        /// <value>
        /// The position offset.
        /// </value>
        public int PositionOffset { get; set; }

#if NET472_OR_GREATER
        /// <summary>
        /// Gets the grid column.
        /// </summary>
        /// <returns></returns>
        [RockInternal( "19.0", true )]
        public Rock.Web.UI.Controls.LavaField GetGridColumn()
        {
            var lavaField = new Rock.Web.UI.Controls.LavaField();

            lavaField.HeaderText = this.HeaderText;
            lavaField.HeaderStyle.CssClass = this.HeaderClass;
            lavaField.ItemStyle.CssClass = this.ItemClass;
            lavaField.LavaTemplate = this.LavaTemplate;
            lavaField.ConvertToItemDictionary = false;

            return lavaField;
        }
#endif
    }
}
