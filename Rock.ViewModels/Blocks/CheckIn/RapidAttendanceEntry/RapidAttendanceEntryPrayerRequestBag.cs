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

namespace Rock.ViewModels.Blocks.CheckIn.RapidAttendanceEntry
{
    /// <summary>
    /// A prayer request entered for an individual on the entry screen.
    /// </summary>
    public class RapidAttendanceEntryPrayerRequestBag
    {
        /// <summary>
        /// Gets or sets the text of the request.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the selected category, or null to use the configured default.
        /// </summary>
        public Guid? CategoryGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the request is flagged as urgent.
        /// </summary>
        public bool IsUrgent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the request may display on the public website.
        /// </summary>
        public bool IsPublic { get; set; }
    }
}
