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

using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.GivingAutomationConfiguration
{
    /// <summary>
    /// The editable giving-automation settings shown on the configuration page.
    /// </summary>
    public class GivingAutomationConfigurationBag
    {
        #region General Settings

        /// <summary>
        /// Gets or sets a value indicating whether the giving automation job is enabled.
        /// </summary>
        public bool IsGivingAutomationEnabled { get; set; }

        /// <summary>
        /// Gets or sets the days of the week (as <see cref="System.DayOfWeek"/> numbers)
        /// on which giving group classifications are updated.
        /// </summary>
        public List<string> DaysToUpdateClassifications { get; set; }

        /// <summary>
        /// Gets or sets the GUIDs of the financial transaction types that qualify for giving automation.
        /// </summary>
        public List<string> SelectedTransactionTypeGuids { get; set; }

        /// <summary>
        /// Gets or sets the account selection mode: <c>AllTaxDeductible</c> or <c>Custom</c>.
        /// </summary>
        public string AccountType { get; set; }

        /// <summary>
        /// Gets or sets the specific financial accounts used when <see cref="AccountType"/> is <c>Custom</c>.
        /// </summary>
        public List<ListItemBag> SelectedAccounts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether child accounts of the selected accounts are included.
        /// </summary>
        public bool IsIncludeChildAccounts { get; set; }

        #endregion

        #region Giving Journey Settings

        /// <summary>
        /// Gets or sets the days of the week (as <see cref="System.DayOfWeek"/> numbers)
        /// on which giving journeys are updated.
        /// </summary>
        public List<string> DaysToUpdateGivingJourneys { get; set; }

        /// <summary>
        /// Gets or sets the lower bound of the contribution-count range that defines a New Giver.
        /// </summary>
        public int? NewGiverContributionCountMinimum { get; set; }

        /// <summary>
        /// Gets or sets the upper bound of the contribution-count range that defines a New Giver.
        /// </summary>
        public int? NewGiverContributionCountMaximum { get; set; }

        /// <summary>
        /// Gets or sets the number of days within which a New Giver's first gift must have occurred.
        /// </summary>
        public int? NewGiverFirstGaveDays { get; set; }

        /// <summary>
        /// Gets or sets the number of days within which a Consistent Giver must have a qualifying gift.
        /// </summary>
        public int? ConsistentGiverLastGaveDays { get; set; }

        /// <summary>
        /// Gets or sets the maximum mean gift frequency (in days) for a Consistent Giver.
        /// </summary>
        public int? ConsistentGiverMeanFrequency { get; set; }

        /// <summary>
        /// Gets or sets the number of days within which an Occasional Giver must have a qualifying gift.
        /// </summary>
        public int? OccasionalGiverLastGaveDays { get; set; }

        /// <summary>
        /// Gets or sets the maximum mean gift frequency (in days) for an Occasional Giver.
        /// </summary>
        public int? OccasionalGiverMeanFrequency { get; set; }

        /// <summary>
        /// Gets or sets the number of days without a gift that defines a Lapsed Giver.
        /// </summary>
        public int? LapsedGiverNoGiftDays { get; set; }

        /// <summary>
        /// Gets or sets the mean gift frequency (in days) threshold for a Lapsed Giver.
        /// </summary>
        public int? LapsedGiverMeanFrequency { get; set; }

        #endregion

        #region Alerting Settings

        /// <summary>
        /// Gets or sets the number of days that must pass before any alert can be triggered again.
        /// </summary>
        public int? GlobalRepeatPreventionDurationDays { get; set; }

        /// <summary>
        /// Gets or sets the number of days that must pass before a gratitude alert can be triggered again.
        /// </summary>
        public int? GratitudeRepeatPreventionDurationDays { get; set; }

        /// <summary>
        /// Gets or sets the number of days that must pass before a follow-up alert can be triggered again.
        /// </summary>
        public int? FollowupRepeatPreventionDurationDays { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the grid data for the configured financial transaction alert types.
        /// </summary>
        public GridDataBag AlertTypes { get; set; }
    }
}
