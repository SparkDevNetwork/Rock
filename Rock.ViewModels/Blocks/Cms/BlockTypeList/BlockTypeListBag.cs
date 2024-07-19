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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.BlockTypeList
{
    /// <summary>
    /// 
    /// </summary>
    public class BlockTypeListBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the category of the BlockType.  Blocks will be grouped by category when displayed to user
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the category of the BlockType.  Blocks will be grouped by category when displayed to user
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the user defined description of the BlockType.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this BlockType was created by and is a part of the Rock core system/framework. This property is required.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the name of the BlockType.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets relative path to the .Net ASCX UserControl that provides the HTML Markup and code for the BlockType.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets usage count of blocks.
        /// </summary>
        public int BlocksCount { get; set; }

        /// <summary>
        /// Gets or sets status.
        /// </summary>
        public string Status { get; set; }
    }
}
