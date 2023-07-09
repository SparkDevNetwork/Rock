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

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler
{
    /// <summary>
    /// The outcome of a request to clone schedule occurrences.
    /// </summary>
    public class GroupSchedulerCloneSchedulesResponseBag
    {
        /// <summary>
        /// Gets or sets the source date range.
        /// </summary>
        /// <value>
        /// The source date range.
        /// </value>
        public string SourceDateRange { get; set; }

        /// <summary>
        /// Gets or sets the destination date range.
        /// </summary>
        /// <value>
        /// The destination date range.
        /// </value>
        public string DestinationDateRange { get; set; }

        /// <summary>
        /// Gets or sets whether there were any occurrences to clone.
        /// </summary>
        /// <value>
        /// Whether there were any occurrences to clone.
        /// </value>
        public bool AnyOccurrencesToClone { get; set; }

        /// <summary>
        /// Gets or sets the count of schedule occurrences cloned.
        /// </summary>
        /// <value>
        /// The count of schedule occurrences cloned.
        /// </value>
        public int OccurrencesClonedCount { get; set; }

        /// <summary>
        /// Gets or sets the count of individuals cloned.
        /// </summary>
        /// <value>
        /// The count of individuals cloned.
        /// </value>
        public int IndividualsClonedCount { get; set; }

        /// <summary>
        /// Gets or sets the explanation for any individuals that were skipped during the cloning attempt.
        /// </summary>
        /// <value>
        /// The explanation for any individuals that were skipped during the cloning attempt.
        /// </value>
        public string SkippedIndividualsExplanation { get; set; }
    }
}
