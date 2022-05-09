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

namespace Rock.ViewModels.Utility
{
    /// <summary>
    /// Provides the details about a category that an attribute belongs to.
    /// </summary>
    public class PublicAttributeCategoryBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the category.
        /// </summary>
        /// <value>The unique identifier of the category.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        /// <value>The name of the category.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order of the category. This provides ordering
        /// information when multiple attributes need to be grouped by
        /// category and then order the categories for display.
        /// </summary>
        /// <value>The order of the category.</value>
        public int Order { get; set; }
    }
}
