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

namespace Rock.ViewModels.Blocks.Finance.FinancialPledgeDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class FinancialPledgeDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the select group type unique identifier.
        /// </summary>
        /// <value>
        /// The select group type unique identifier.
        /// </value>
        public Guid? SelectGroupTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the groups.
        /// </summary>
        /// <value>
        /// The groups.
        /// </value>
        public List<ListItemBag> Groups { get; set; }

        /// <summary>
        /// Gets or sets the type of the group.
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        public string GroupType { get; set; }
    }
}
