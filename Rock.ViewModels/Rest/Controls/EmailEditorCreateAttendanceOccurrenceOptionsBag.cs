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

using System;

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// Bag containing the information to create an attendance occurrence in the Email Editor control.
    /// </summary>
    public class EmailEditorCreateAttendanceOccurrenceOptionsBag
    {
        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <remarks>This uses an integer identifier for backward compatibility with old communications.</remarks>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the date of the attendance occurrence.
        /// </summary>
        public DateTime OccurrenceDate { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <remarks>This uses an integer identifier for backward compatibility with old communications.</remarks>
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <remarks>This uses an integer identifier for backward compatibility with old communications.</remarks>
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the security grant token to use when performing
        /// authorization checks.
        /// </summary>
        /// <value>The security grant token.</value>
        public string SecurityGrantToken { get; set; }
    }
}