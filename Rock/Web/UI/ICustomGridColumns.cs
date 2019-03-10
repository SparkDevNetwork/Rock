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

using Rock.Web.UI.Controls;

namespace Rock.Web.UI
{
    /// <summary>
    /// Interface for Blocks that have a grid that supports adding custom columns and sticky headers from Block Configuration.
    /// </summary>
    public interface ICustomGridColumns : ICustomGridOptions
    {
    }

    /// <summary>
    /// Config Class for blocks that support adding custom columns
    /// </summary>
    [Serializable]
    public class CustomGridColumnsConfig
    {
        /// <summary>
        /// The custom grid columns attribute key
        /// </summary>
        public const string AttributeKey = "core.CustomGridColumnsConfig";

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomGridColumnsConfig"/> class.
        /// </summary>
        public CustomGridColumnsConfig()
        {
            this.ColumnsConfig = new List<ColumnConfig>();
        }

        /// <summary>
        /// Gets or sets the columns configuration.
        /// </summary>
        /// <value>
        /// The columns configuration.
        /// </value>
        public List<ColumnConfig> ColumnsConfig { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class ColumnConfig
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
            public OffsetType PositionOffsetType { get; set; }

            /// <summary>
            /// Gets or sets the position offset relative to PositionOffsetType
            /// </summary>
            /// <value>
            /// The position offset.
            /// </value>
            public int PositionOffset { get; set; }
            
            /// <summary>
            /// Gets the grid column.
            /// </summary>
            /// <returns></returns>
            public LavaField GetGridColumn()
            {
                var lavaField = new LavaField();
                lavaField.HeaderText = this.HeaderText;
                lavaField.HeaderStyle.CssClass = this.HeaderClass;
                lavaField.ItemStyle.CssClass = this.ItemClass;
                lavaField.LavaTemplate = this.LavaTemplate;
                lavaField.ConvertToItemDictionary = false;
                return lavaField;
            }

            /// <summary>
            /// 
            /// </summary>
            public enum OffsetType
            {
                /// <summary>
                /// The first column
                /// </summary>
                FirstColumn,
                
                /// <summary>
                /// The last column
                /// </summary>
                LastColumn
            }
        }
    }
}
