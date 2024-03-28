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

namespace Rock.ViewModels.Blocks.Cms.BlockTypeDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class BlockTypeBag : EntityBagBase
    {
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
        public ListItemBag EntityType { get; set; }

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
        /// Gets or sets a flag indicating if this Block exists.
        /// </summary>
        public bool IsBlockExists { get; set; }

        /// <summary>
        /// Gets or sets the name of the fully qualified page referencing Block Type.
        /// </summary>
        public List<string> Pages { get; set; }

        /// <summary>
        /// Gets or sets the name of the Layouts having the Block of the given Block Type.
        /// </summary>
        public List<string> Layouts { get; set; }

        /// <summary>
        /// Gets or sets the name of the Sites having the Block of the given Block Type.
        /// </summary>
        public List<string> Sites { get; set; }
    }
}
