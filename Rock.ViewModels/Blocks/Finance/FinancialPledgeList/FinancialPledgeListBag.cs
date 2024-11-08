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

namespace Rock.ViewModels.Blocks.Finance.FinancialPledgeList
{
    /// <summary>
    /// 
    /// </summary>
    public class FinancialPledgeListBag
    {
        /// <summary>
        /// Gets or sets the pledge ID.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the person alias ID.
        /// </summary>
        public int? PersonAliasId { get; set; }
        /// <summary>
        /// Gets or sets the account ID.
        /// </summary>
        public int? AccountId { get; set; }
        /// <summary>
        /// Gets or sets the group ID.
        /// </summary>
        public int? GroupId { get; set; }
        /// <summary>
        /// Gets or sets the pledge frequency value ID.
        /// </summary>
        public int? PledgeFrequencyValueId { get; set; }
        /// <summary>
        /// Gets or sets the total amount.
        /// </summary>
        public decimal TotalAmount { get; set; }
        /// <summary>
        /// Gets or sets the pledge start date.
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Gets or sets the pledge end date.
        /// </summary>
        public DateTime EndDate { get; set; }
        /// <summary>
        /// Gets or sets the date and time when the pledge was last modified.
        /// </summary>
        public DateTime? ModifiedDateTime { get; set; }
    }
}
