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
using System.Linq;
using Rock.Utility;

namespace Rock.StatementGenerator
{
    /// <summary>
    /// A Pledge (or Multiple Pledges) for a specific Account
    /// </summary>
    public class PledgeSummary: RockDynamic
    {
        /// <summary>
        /// Gets or sets the pledge list.
        /// </summary>
        /// <value>
        /// The pledge list.
        /// </value>
        public List<Rock.Model.FinancialPledge> PledgeList { get; set; }

        /// <summary>
        /// Gets or sets the pledge start date.
        /// </summary>
        /// <value>
        /// The pledge start date.
        /// </value>
        public DateTime? PledgeStartDate => PledgeList.Min( a => a.StartDate );

        /// <summary>
        /// Gets or sets the pledge end date.
        /// </summary>
        /// <value>
        /// The pledge end date.
        /// </value>
        public DateTime? PledgeEndDate => PledgeList.Max( a => a.EndDate );

        /// <summary>
        /// Gets or sets the amount pledged.
        /// </summary>
        /// <value>
        /// The amount pledged.
        /// </value>
        public decimal AmountPledged => PledgeList.Sum( a => a.TotalAmount );

        /// <summary>
        /// Gets or sets the pledge account identifier.
        /// </summary>
        /// <value>
        /// The pledge account identifier.
        /// </value>
        public int AccountId
        {
            get
            {
                return Account.Id;
            }
        }

        /// <summary>
        /// Gets or sets the pledge account.
        /// </summary>
        /// <value>
        /// The pledge account.
        /// </value>
        public string AccountName
        {
            get
            {
                return Account.Name;
            }
        }

        /// <summary>
        /// Gets or sets the pledge account.
        /// </summary>
        /// <value>
        /// The pledge account.
        /// </value>
        public string AccountPublicName
        {
            get
            {
                return Account.PublicName;
            }
        }

        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        /// <value>
        /// The account.
        /// </value>
        public Rock.Model.FinancialAccount Account { get; set; }

        /// <summary>
        /// Gets the percent complete.
        /// </summary>
        /// <value>
        /// The percent complete.
        /// </value>
        public int PercentComplete => ( int ) ( ( this.AmountGiven * 100 ) / this.AmountPledged );

        /// <summary>
        /// Gets or sets the amount remaining.
        /// </summary>
        /// <value>
        /// The amount remaining.
        /// </value>
        public decimal AmountRemaining => ( this.AmountGiven > this.AmountPledged ) ? 0 : ( this.AmountPledged - this.AmountGiven );

        /// <summary>
        /// Gets or sets the amount given.
        /// </summary>
        /// <value>
        /// The amount given.
        /// </value>
        public decimal AmountGiven { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{this.AccountName} AmountGiven:{this.AmountGiven}, AmountPledged:{this.AmountPledged}";
        }
    }
}
