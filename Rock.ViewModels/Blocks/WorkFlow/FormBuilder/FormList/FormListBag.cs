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

namespace Rock.ViewModels.Blocks.WorkFlow.FormBuilder.FormList
{
    /// <summary>
    /// Represents the data for the Form List block.
    /// </summary>
    public class FormListBag : BlockBox
    {
        /// <summary>
        /// Gets or sets the collection of forms grouped by their category Guid.
        /// </summary>
        public Dictionary<Guid, List<FormListItemBag>> Forms { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user can add a new category.
        /// </summary>
        public bool CanAddNewCategory { get; set; }
    }
}
