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

using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.AttributeValues
{
    /// <summary>
    /// A group of fields rendered together in edit mode. When the block is
    /// grouping attributes by category each group represents one category and
    /// its name is rendered as a heading separator.
    /// </summary>
    public class AttributeValuesFieldGroupBag
    {
        /// <summary>
        /// Gets or sets the category name rendered as a heading above the
        /// group's fields, or <c>null</c> when no heading should be rendered.
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the ordered fields in this group.
        /// </summary>
        public List<AttributeValuesFieldBag> Fields { get; set; }
    }
}
