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

namespace Rock.ViewModels.Blocks.WorkFlow.FormBuilder.FormAnalytics
{
    /// <summary>
    /// One named series of sparse, date-keyed data points for the Form Analytics
    /// line chart. The client densifies these into a continuous x-axis across the
    /// chart window via the <c>selectFilledOverDateRange</c> chart helper.
    /// </summary>
    public class FormAnalyticsSeriesBag
    {
        /// <summary>
        /// Gets or sets the user-visible series label (e.g. "Views", "Completions").
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the sparse points for this series. Only buckets with
        /// activity are included; the client zero-fills the rest of the window.
        /// </summary>
        public List<FormAnalyticsDataPointBag> Points { get; set; } = new List<FormAnalyticsDataPointBag>();
    }
}
