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

using Rock.ViewModels.Utility;
using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Finance.BusinessDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class BusinessDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the search keys.
        /// </summary>
        /// <value>
        /// The search keys.
        /// </value>
        public List<SearchKeyBag> SearchKeys { get; set; }

        /// <summary>
        /// Gets or sets the search types list.
        /// </summary>
        /// <value>
        /// The search types list.
        /// </value>
        public List<ListItemBag> SearchTypesList { get; set; }

        /// <summary>
        /// Gets or sets the tag category unique identifier.
        /// </summary>
        /// <value>
        /// The tag category unique identifier.
        /// </value>
        public Guid? TagCategoryGuid { get; set; }
    }
}
