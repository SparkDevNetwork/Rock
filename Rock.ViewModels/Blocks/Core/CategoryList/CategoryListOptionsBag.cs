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

namespace Rock.ViewModels.Blocks.Core.CategoryList
{
    /// <summary>
    /// The additional configuration options for the Category List block.
    /// </summary>
    public class CategoryListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block (grid) is visible.
        /// False when the current person is not authorized to configure
        /// categories, in which case the <see cref="BlockErrorMessage"/> is
        /// shown instead.
        /// </summary>
        public bool IsBlockVisible { get; set; }

        /// <summary>
        /// Gets or sets the message displayed to the user when the block is
        /// not visible (e.g., authorization failed).
        /// </summary>
        public string BlockErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity-type context is
        /// fixed by the block setting, URL parameter, or parent drill-down.
        /// When true, the entity-type filter in the grid header and the
        /// entity-type picker inside the add/edit modal are both hidden, and
        /// the Entity Type / Qualifier columns are excluded from the grid.
        /// </summary>
        public bool IsEntityTypeContextFixed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether hierarchy drill-down is
        /// enabled. When true, row click navigates to the same page with
        /// <c>?CategoryId=N</c>. When false, row click is a no-op. The Edit
        /// column is always rendered and is the sole way to open the modal
        /// in both modes.
        /// </summary>
        public bool IsHierarchyEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the security column should
        /// be visible. True only when the active entity type is secured.
        /// </summary>
        public bool IsSecurityColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets the entity type GUIDs that must be excluded from any
        /// entity-type picker rendered by the block (Block and ServiceJob
        /// categories are driven by code attribute decorations, not managed
        /// through this UI).
        /// </summary>
        public List<Guid> ExcludedEntityTypeGuids { get; set; }
    }
}
