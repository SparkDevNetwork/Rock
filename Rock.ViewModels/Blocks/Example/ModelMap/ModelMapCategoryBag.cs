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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Example.ModelMap
{
    /// <summary>
    /// Represents a category of models in the Model Map.
    /// </summary>
    public class ModelMapCategoryBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the category.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the list of models of this category.
        /// </summary>
        public List<ListItemBag> Models { get; set; }
    }
}
