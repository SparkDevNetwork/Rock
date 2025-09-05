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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.StepProgramDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class ChartDataBag
    {
        /// <summary>
        /// Gets or sets the date labels for the chart data.
        /// </summary>
        public List<DateTime> DateLabels { get; set; }

        /// <summary>
        /// Gets or sets the time unit for the chart data (e.g. "day", "month", "year").
        /// </summary>
        public string TimeUnit { get; set; }

        /// <summary>
        /// Gets or sets the string labels for the chart data.
        /// </summary>
        public List<string> StringLabels { get; set; }

        /// <summary>
        /// Gets or sets the List of ListItemBags for the Campus Labels
        /// </summary>
        public List<ListItemBag> CampusLabels { get; set; }

        /// <summary>
        /// Gets or sets the series data for the chart.
        /// </summary>
        public List<SeriesBag> Series { get; set; }
    }
}
