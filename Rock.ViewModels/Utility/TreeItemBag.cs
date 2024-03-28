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

namespace Rock.ViewModels.Utility
{
    /// <summary>
    /// Describes a single item that can be displayed in a tree view.
    /// </summary>
    public class TreeItemBag
    {
        /// <summary>
        /// Gets or sets the generic identifier of this item.
        /// </summary>
        /// <value>
        /// The generic identifier of this item.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the text that should be displayed to identify this item.
        /// </summary>
        /// <value>
        /// The text that should be displayed to identify this item.
        /// </value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the type of item represented by this instance. There is
        /// no pre-defined meaning to this, it is up to the item provider to
        /// decide what to do with it.
        /// </summary>
        /// <value>The type of item.</value>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a folder.
        /// A folder is an item that is intended to hold child items. This is
        /// a distinction from the <see cref="HasChildren"/> property which
        /// specifies if this item _currently_ has children or not.
        /// </summary>
        /// <value><c>true</c> if this instance is a folder; otherwise, <c>false</c>.</value>
        public bool IsFolder { get; set; }

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
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the children. A value of null indicates that the
        /// children should be lazy loaded by the caller.
        /// </summary>
        /// <value>
        /// The child tree items of this item.
        /// </value>
        public List<TreeItemBag> Children { get; set; }

        /// <summary>
        /// Gets or sets the child count. Optional.
        /// </summary>
        /// <value>
        /// The number of children this node has
        /// </value>
        public int? ChildCount { get; set; }
    }
}
