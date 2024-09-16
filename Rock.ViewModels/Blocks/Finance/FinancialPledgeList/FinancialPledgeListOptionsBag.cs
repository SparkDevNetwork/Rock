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

namespace Rock.ViewModels.Blocks.Finance.FinancialPledgeList
{
    /// <summary>
    /// 
    /// </summary>
    public class FinancialPledgeListOptionsBag
    {
        /// <summary>
        /// Determines if the accounts column should be displayed.
        /// </summary>
        public bool ShowAccountColumn { get; set; }

        /// <summary>
        /// Determines if the last modified date column should be displayed.
        /// </summary>
        public bool ShowLastModifiedDateColumn { get; set; }

        /// <summary>
        /// Determines if the group column should be displayed.
        /// </summary>
        public bool ShowGroupColumn { get; set; }

        /// <summary>
        /// Determines if the results should be limited to pledges for the current person.
        /// </summary>
        public bool LimitPledgesToCurrentPerson { get; set; }

        /// <summary>
        /// Determines if the account summary should be displayed at the bottom of the list.
        /// </summary>
        public bool ShowAccountSummary { get; set; }

        /// <summary>
        /// Determines if the amount column should be displayed.
        /// </summary>
        public bool HideAmount { get; set; }

        /// <summary>
        /// Determines if the person filter should be displayed.
        /// </summary>
        public bool ShowPersonFilter { get; set; }

        /// <summary>
        /// Determines if the account filter should be displayed.
        /// </summary>
        public bool ShowAccountFilter { get; set; }

        /// <summary>
        /// Determines if the date range filter should be displayed.
        /// </summary>
        public bool ShowDateRangeFilter { get; set; }

        /// <summary>
        /// Determines if the last modified date range filter should be displayed.
        /// </summary>
        public bool ShowLastModifiedFilter { get; set; }
    }
}
