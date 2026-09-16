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

namespace Rock.ViewModels.Blocks.Crm.BulkUpdate
{
    /// <summary>
    /// The outcome of a bulk update run, surfaced back to the UI via the final
    /// TaskActivityProgress TaskCompleted payload. Every selected person lands in exactly
    /// one of three buckets, so <see cref="SuccessCount"/> + <see cref="IssuesCount"/> +
    /// <see cref="FailedCount"/> equals <see cref="TotalCount"/>.
    /// </summary>
    public class BulkUpdateResultBag
    {
        /// <summary>
        /// Gets or sets the total number of persons in the requested update.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the number of persons whose every requested action was applied
        /// without issue.
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Gets or sets the number of persons who were processed but had at least one
        /// requested action that could not be applied (e.g. a step prerequisite was unmet
        /// or a group membership was invalid). The applied actions still committed; the
        /// per-person reasons are in <see cref="PersonResults"/>.
        /// </summary>
        public int IssuesCount { get; set; }

        /// <summary>
        /// Gets or sets the number of persons who could not be processed at all and
        /// therefore have no changes: a batch-level error rolled their changes back, or the
        /// person could not be found (deleted or merged after selection). The explanatory
        /// messages are in <see cref="Errors"/>.
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        /// Gets or sets the per-person detail for everyone counted in
        /// <see cref="IssuesCount"/>. Persons whose update was fully applied are not listed
        /// (a clean run sends an empty list).
        /// </summary>
        public List<BulkUpdatePersonResultBag> PersonResults { get; set; } = new List<BulkUpdatePersonResultBag>();

        /// <summary>
        /// Gets or sets the run-level error messages that are not tied to a single named
        /// person: a batch-level exception, or an aggregate "N individuals could not be
        /// found" notice. Each <see cref="FailedCount"/> person is accounted for here.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
    }
}
