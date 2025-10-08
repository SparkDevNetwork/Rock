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
namespace Rock.AI.Agent.Classes.Skills.PersonSkill
{
    /// <summary>
    /// Represents the summary of check-in data for a specific month.
    /// </summary>
    /// <remarks>
    /// This class contains details about the month, year, and the completion percentage of check-ins for that period.
    /// </remarks>
    internal class CheckInSummaryMonthResult
    {
        /// <summary>
        /// Gets or sets the name of the month.
        /// </summary>
        /// <value>
        /// The name of the month.
        /// </value>
        public int Month { get; set; }

        /// <summary>
        /// Gets or sets the year of the check-in summary.
        /// </summary>
        /// <value>
        /// The year of the check-in summary.
        /// </value>
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the completion percentage of check-ins for the month.
        /// </summary>
        /// <value>
        /// The completion percentage of check-ins for the month.
        /// </value>
        public decimal CompletionPercentage { get; set; }
    }
}
