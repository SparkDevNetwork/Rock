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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// Describes a single item that can be displayed in a categorized value picker, both categories and value nodes.
    /// </summary>
    public class CategorizedValuePickerNodeBag
    {
        /// <summary>
        /// Gets or sets the generic identifier of this item.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the text that should be displayed to identify this item.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the child categories.
        /// </summary>
        public List<CategorizedValuePickerNodeBag> ChildCategories { get; set; }

        /// <summary>
        /// Gets or sets the child values.
        /// </summary>
        public List<CategorizedValuePickerNodeBag> ChildValues { get; set; }
    }
}
