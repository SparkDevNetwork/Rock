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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Reporting.CalendarDimensionSettings
{
    /// <summary>
    /// Contains the settings that can be edited for the Calendar Dimension Settings block.
    /// </summary>
    public class CalendarDimensionSettingsBag
    {
        /// <summary>
        /// Gets or sets the start date for the analytics source date range.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date for the analytics source date range.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the month number (1-12) on which the fiscal year begins.
        /// </summary>
        public int FiscalStartMonth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the giving month should use
        /// the Sunday date rather than the calendar date.
        /// </summary>
        public bool IsGivingMonthUseSundayDate { get; set; }
    }
}
