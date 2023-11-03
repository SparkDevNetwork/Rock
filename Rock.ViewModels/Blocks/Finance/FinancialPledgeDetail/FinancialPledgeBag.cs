﻿// <copyright>
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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.FinancialPledgeDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class FinancialPledgeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the Rock.Model.FinancialAccount or account that the pledge is being directed toward.
        /// </summary>
        public ListItemBag Account { get; set; }

        /// <summary>
        /// Gets or sets the end date of the pledge period.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.Group.
        /// </summary>
        public ListItemBag Group { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.PersonAlias.
        /// </summary>
        public ListItemBag PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the pledge frequency Rock.Model.DefinedValue. This is how often the Rock.Model.Person who is 
        /// making the pledge promises to give the Rock.Model.FinancialPledge.TotalAmount
        /// </summary>
        public ListItemBag PledgeFrequencyValue { get; set; }

        /// <summary>
        /// Gets or sets the start date of the pledge period.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the pledge amount that is promised to be given.
        /// </summary>
        public decimal? TotalAmount { get; set; }
    }
}
