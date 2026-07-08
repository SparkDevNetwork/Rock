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

namespace Rock.ViewModels.Blocks.Core.CategoryTreeView
{
    /// <summary>
    /// The Category Tree View block's configured settings.
    /// </summary>
    public class CategoryTreeViewBlockAttributesBag
    {
        /// <summary>
        /// Gets or sets the entity type whose categories the tree displays.
        /// </summary>
        public Guid? EntityTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the display name for the entity type; falls back to the entity type's own name when blank.
        /// </summary>
        public string EntityTypeFriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the category the tree treats as its root, when one is configured.
        /// </summary>
        public Guid? RootCategoryGuid { get; set; }

        /// <summary>
        /// Gets or sets the entity-type qualifier column that scopes the categories.
        /// </summary>
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the entity-type qualifier value that scopes the categories.
        /// </summary>
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the query string parameter the selected category's IdKey is written to.
        /// </summary>
        public string PageParameterKey { get; set; }

        /// <summary>
        /// Gets or sets whether the tree shows only categories rather than the categorized entities beneath them.
        /// </summary>
        public bool ShowOnlyCategories { get; set; }

        /// <summary>
        /// Gets or sets whether entity items with a blank name are shown.
        /// </summary>
        public bool ShowUnnamedEntityItems { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class used for items that have no icon of their own.
        /// </summary>
        public string DefaultIconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the categories to exclude from the tree.
        /// </summary>
        public List<Guid> ExcludeCategoryGuids { get; set; }

        /// <summary>
        /// Gets or sets the optional title for the panel that wraps the tree; when set, the panel also shows the add actions.
        /// </summary>
        public string PanelTitle { get; set; }
    }
}
