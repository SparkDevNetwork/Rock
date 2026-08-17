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

namespace Rock.ViewModels.Blocks.CheckIn.Manager.AttendanceDetail
{
    /// <summary>
    /// One row of the change-history list on the Attendance Detail view panel.
    /// </summary>
    public class AttendanceDetailChangeHistoryBag
    {
        /// <summary>
        /// Gets or sets the URL of the configured Person Profile page for the
        /// person who created the history row. Null suppresses the link
        /// (falls back to plain text) when the person cannot be resolved or
        /// no profile page is configured.
        /// </summary>
        public string CreatedPersonUrl { get; set; }

        /// <summary>
        /// Gets or sets the display name of the person who created the
        /// history row.
        /// </summary>
        public string CreatedPersonName { get; set; }

        /// <summary>
        /// Gets or sets the elapsed-time string (e.g. "3 days ago") the row
        /// was recorded.
        /// </summary>
        public string CreatedDateTimeElapsed { get; set; }

        /// <summary>
        /// Gets or sets the free-text description of what changed.
        /// </summary>
        public string Description { get; set; }
    }
}
