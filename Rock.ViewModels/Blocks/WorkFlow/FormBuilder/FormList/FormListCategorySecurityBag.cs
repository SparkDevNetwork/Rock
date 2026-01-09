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

namespace Rock.ViewModels.Blocks.WorkFlow.FormBuilder.FormList
{
    /// <summary>
    /// Contains security information for a category in the form list.
    /// </summary>
    public class FormListCategorySecurityBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the current user can edit the category.
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user can delete the category.
        /// </summary>
        public bool CanDelete { get; set; }
    }
}


