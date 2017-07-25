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

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class TreeViewItem
    {
        /// <summary>
        /// 
        /// </summary>
        public enum GetCountsType
        {
            /// <summary>
            /// none
            /// </summary>
            None = 0,

            /// <summary>
            /// child groups
            /// </summary>
            ChildGroups = 1,

            /// <summary>
            /// group members
            /// </summary>
            GroupMembers = 2
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeViewItem"/> class.
        /// </summary>
        public TreeViewItem()
        {
            IsActive = true;
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has children.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has children; otherwise, <c>false</c>.
        /// </value>
        public bool HasChildren { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets an image url.
        /// </summary>
        /// <value>
        /// The image url.
        /// </value>
        public string IconSmallUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the count information depending on the setting for GetCountsType
        /// </summary>
        /// <value>
        /// The count information.
        /// </value>
        public int? CountInfo { get; set; }

        /// <summary>
        /// Gets or sets the children (leave this null to have rockTree.js lazy load the children as needed)
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        public List<TreeViewItem> Children { get; set; }
    }
}
