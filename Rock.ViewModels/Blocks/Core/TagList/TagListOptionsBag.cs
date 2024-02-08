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

namespace Rock.ViewModels.Blocks.Core.TagList
{
    /// <summary>
    /// The additional configuration options for the Tag List block.
    /// </summary>
    public class TagListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the reorder column on the grid should be visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the column should be visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsReorderColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the qualifier columns should visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the qualifier columns should be visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsQualifierColumnsVisible { get; set; }

        /// <summary>
        /// Gets or sets the current person.
        /// </summary>
        /// <value>
        /// The current person.
        /// </value>
        public ListItemBag CurrentPersonAlias { get; set; }
    }
}
