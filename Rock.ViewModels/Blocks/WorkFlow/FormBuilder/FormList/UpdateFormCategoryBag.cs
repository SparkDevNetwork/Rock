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

namespace Rock.ViewModels.Blocks.WorkFlow.FormBuilder.FormList
{
    /// <summary>
    /// Represents the data needed to update a form category.
    /// </summary>
    public class UpdateFormCategoryBag
    {
        /// <summary>
        /// Gets or sets the category GUID.
        /// </summary>
        public Guid? CategoryGuid { get; set; }
        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the category description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the highlight color.
        /// </summary>
        public string HighlightColor { get; set; }

        /// <summary>
        /// Gets or sets the parent category GUID.
        /// </summary>
        public Guid? ParentCategoryGuid { get; set; }
    }
}

