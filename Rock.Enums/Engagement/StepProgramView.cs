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
using System.ComponentModel;

namespace Rock.Enums.Engagement
{
    /// <summary>
    /// Defines the different views available for step charts.
    /// </summary>
    public enum StepProgramView
    {
        /// <summary>
        /// The default view showing Key Performance Indicators (KPIs).
        /// </summary>
        [Description( "KPIs" )]
        KPIs = 0,

        /// <summary>
        /// The view showing trends over time.
        /// </summary>
        Trends = 1,

        /// <summary>
        /// The view showing totals for the steps.
        /// </summary>
        Totals = 2,

        /// <summary>
        /// The view showing data segmented by campuses.
        /// </summary>
        Campuses = 3,

        /// <summary>
        /// The view showing the flow of engagement through different steps.
        /// </summary>
        Flow = 4,
    }
}
