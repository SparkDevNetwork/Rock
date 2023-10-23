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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.CategoryDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class CategoryBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.EntityType that can use this Category.
        /// </summary>
        public ListItemBag EntityType { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier column that contains the value (see Rock.Model.Category.EntityTypeQualifierValue) that is used to narrow the scope of the Category.
        /// </summary>
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value that is used to narrow the scope of the Category to a subset or specific instance of an EntityType.
        /// </summary>
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the color of the highlight.
        /// </summary>
        public string HighlightColor { get; set; }

        /// <summary>
        /// Gets or sets the name of the icon CSS class. This property is only used for CSS based icons.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this Category is part of the Rock core system/framework.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Category
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parent category.
        /// </summary>
        public ListItemBag ParentCategory { get; set; }

        /// <summary>
        /// Gets or sets the root category.
        /// </summary>
        public Guid? RootCategoryGuid { get; set; }


    }
}
