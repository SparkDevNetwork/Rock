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

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.GivingOverview
{
    /// <summary>
    /// Holds the initialization data for the Giving Overview block.
    /// </summary>
    public class GivingOverviewBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block content should be
        /// rendered. This is false when no person could be resolved.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person has any giving
        /// history within the last three years. When false only a "no giving
        /// data" message is displayed.
        /// </summary>
        public bool HasGivingData { get; set; }

        /// <summary>
        /// Gets or sets the display text for the first gift badge.
        /// </summary>
        public string FirstGiftText { get; set; }

        /// <summary>
        /// Gets or sets the tooltip (first gift date) for the first gift badge.
        /// </summary>
        public string FirstGiftTooltip { get; set; }

        /// <summary>
        /// Gets or sets the display text for the last gift badge.
        /// </summary>
        public string LastGiftText { get; set; }

        /// <summary>
        /// Gets or sets the tooltip (last gift date) for the last gift badge.
        /// </summary>
        public string LastGiftTooltip { get; set; }

        /// <summary>
        /// Gets or sets the number of gratitude alerts for the person's giving group.
        /// </summary>
        public int GratitudeAlertCount { get; set; }

        /// <summary>
        /// Gets or sets the number of follow-up alerts for the person's giving group.
        /// </summary>
        public int FollowUpAlertCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person is considered an
        /// inactive giver (no gifts since the configured cutoff).
        /// </summary>
        public bool IsInactiveGiver { get; set; }

        /// <summary>
        /// Gets or sets the month and year of the most recent gift, displayed
        /// in the inactive giver warning.
        /// </summary>
        public string InactiveLastGiftText { get; set; }

        /// <summary>
        /// Gets or sets the monthly giving amounts for the trailing 36 months,
        /// in ascending month order, used to render the giving by month chart.
        /// </summary>
        public List<MonthlyGivingBag> GivingByMonth { get; set; }

        /// <summary>
        /// Gets or sets the rendered HTML for the last 12 months giving
        /// statistics KPI section.
        /// </summary>
        public string GivingStatsKpiHtml { get; set; }

        /// <summary>
        /// Gets or sets the rendered HTML for the giving characteristics KPI section.
        /// </summary>
        public string GivingCharacteristicsKpiHtml { get; set; }

        /// <summary>
        /// Gets or sets the warning displayed when the giving characteristics
        /// are stale. Null when the characteristics are current.
        /// </summary>
        public string StaleWarningText { get; set; }

        /// <summary>
        /// Gets or sets the person's giving percentile (0-100).
        /// </summary>
        public int GivingPercentile { get; set; }

        /// <summary>
        /// Gets or sets the community view stage (1-10) to highlight for the
        /// person's giving percentile. Zero indicates no highlight.
        /// </summary>
        public int PercentileStage { get; set; }

        /// <summary>
        /// Gets or sets the person's giving bin (1-4). Zero indicates no highlight.
        /// </summary>
        public int GivingBin { get; set; }

        /// <summary>
        /// Gets or sets the rollover help text describing the person's
        /// percentile and bin in the community view.
        /// </summary>
        public string CommunityViewHelpText { get; set; }

        /// <summary>
        /// Gets or sets the collapsed yearly contribution summaries (current
        /// and previous year). The full history is retrieved with the
        /// GetYearlySummary block action.
        /// </summary>
        public List<ContributionYearSummaryBag> YearlySummary { get; set; }
    }
}
