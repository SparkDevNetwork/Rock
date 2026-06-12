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
    /// Carries a workflow form category's identifiers so the block can resolve the
    /// selected category from the integer Id carried in the page parameter.
    /// </summary>
    public class FormListCategoryBag
    {
        /// <summary>
        /// Gets or sets the integer identifier carried in the page parameter.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the obfuscated identifier.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier the forms are keyed by.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the category name, shown in the forms panel heading.
        /// </summary>
        public string Name { get; set; }
    }
}
