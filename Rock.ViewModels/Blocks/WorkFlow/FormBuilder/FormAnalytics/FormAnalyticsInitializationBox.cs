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

namespace Rock.ViewModels.Blocks.WorkFlow.FormBuilder.FormAnalytics
{
    /// <summary>
    /// Initial configuration the Form Analytics block sends to its Vue counterpart.
    /// </summary>
    public class FormAnalyticsInitializationBox
    {
        /// <summary>
        /// Gets or sets a value indicating whether the form was successfully resolved
        /// from the page parameter. False renders the "invalid form" notification.
        /// </summary>
        public bool CanView { get; set; }

        /// <summary>
        /// Gets or sets the friendly display name of the form (e.g. "DISC Request Form")
        /// for use in the panel title.
        /// </summary>
        public string FormName { get; set; }

        /// <summary>
        /// Gets or sets the per-user persisted SlidingDateRange delimited string. Empty
        /// when no preference has been saved (the editor falls back to "Current|Year").
        /// </summary>
        public string SlidingDateRangeDelimitedValues { get; set; }

        /// <summary>
        /// Gets or sets the initial KPIs and chart payload, computed against the user's
        /// saved SlidingDateRange (or the default "Current|Year" range).
        /// </summary>
        public FormAnalyticsChartDataBag ChartData { get; set; }
    }
}
